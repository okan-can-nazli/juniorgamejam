using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace LeafBrawl.Bots
{
    /// <summary>
    /// Bot karar mantığı (SRS Bölüm 5). Bot.prefab kökünde. Oyuncuyla aynı kurallar: bot yalnızca CharacterIntent üretir (BOT-01).
    /// 4 durum, öncelik sırasıyla: Alana dön > Kaç > Saldır > Topla (BOT-03). Karar decisionIntervalSec'te bir verilir;
    /// hareket, nişan ve savurma her karede bu karara göre üretilir.
    /// </summary>
    public class BotBrain : MonoBehaviour, IIntentSource
    {
        public enum BotState { ReturnToZone, Flee, Attack, Collect }

        Character _self;
        readonly BotPerception _perception = new BotPerception();
        readonly BotNavigator _navigator = new BotNavigator();
        float _nextDecisionTime;

        // Karar
        Character _target;          // Saldır: hedef · Kaç: kaçılan rakip
        float _targetSeenTime;
        IPickup _pickupGoal;
        float _onPickupSince = -1f;
        ITree _treeGoal;
        Vector2? _moveGoal;
        RecipeId? _craftRequest;
        StatType[] _priority;

        // Envanteri dolu olduğu için toplanamayan yapraklar: üstüne gelinip yerde kalanlar bir daha seçilmez.
        readonly HashSet<IPickup> _ignoredPickups = new HashSet<IPickup>();

        public BotState State { get; private set; } = BotState.Collect;
        /// <summary>Saldır'da hedef, Kaç'ta kaçılan rakip (debug ve görsel için).</summary>
        public Character Target => _target;

        void Awake() => _self = GetComponent<Character>();

        public CharacterIntent ReadIntent(Character self)
        {
            if (Time.time >= _nextDecisionTime)
            {
                _nextDecisionTime = Time.time + Services.Config.bots.decisionIntervalSec;
                Decide();
            }

            // BOT-10: üretim sırasında yerinde durur (balta ve yetenekler üretimde zaten çalışmaz).
            if (_self.Crafting.IsCrafting)
            {
                _navigator.TrackStuck(_self.Position, false);
                return new CharacterIntent { Aim = _self.Facing };
            }

            var intent = State switch
            {
                BotState.ReturnToZone => ReturnToZoneIntent(),
                BotState.Flee => FleeIntent(),
                BotState.Attack => AttackIntent(),
                _ => CollectIntent(),
            };
            UseAbilities(ref intent);

            if (_craftRequest.HasValue)
            {
                intent.Craft = _craftRequest;
                _craftRequest = null;
            }

            _navigator.TrackStuck(_self.Position, intent.Move.sqrMagnitude > 0f);
            return intent;
        }

        // ---------------- Karar ----------------

        void Decide()
        {
            var bots = Services.Config.bots;
            _perception.Refresh(_self);
            float hp = HpRatio();
            var nearEnemy = _perception.NearestEnemy(_self.Position, Units.ToUnits(bots.enemyNearPx));

            BotState next;
            if (_perception.ZoneDanger)
                next = BotState.ReturnToZone;                                                   // BOT-04
            else if (nearEnemy != null && hp < (State == BotState.Flee ? bots.fleeExitHpRatio : bots.fleeHpRatio))
                next = BotState.Flee;                                                           // BOT-05 (%50'de çıkar)
            else if (hp >= bots.fleeHpRatio && nearEnemy != null)
                next = BotState.Attack;                                                         // BOT-06
            else
                next = BotState.Collect;                                                        // BOT-07
            State = next;

            var previous = _target;
            _target = State == BotState.Attack ? PickTarget(Units.ToUnits(bots.enemyNearPx))
                : State == BotState.Flee ? nearEnemy
                : null;
            if (_target != previous) _targetSeenTime = Time.time;

            if (State == BotState.Collect) ChooseCollectGoal();
            _craftRequest = ChooseCraft();
        }

        // BOT-08: (yığın kademesi × ağırlık) − mesafe en yüksek görünen rakip. Oyuncu ile bot ayırt edilmez.
        Character PickTarget(float maxDistance)
        {
            var bots = Services.Config.bots;
            Character best = null;
            float bestScore = float.MinValue;
            foreach (var other in _perception.Enemies)
            {
                float distance = Vector2.Distance(_self.Position, other.Position);
                if (distance > maxDistance) continue;
                float score = (int)other.PileTier * bots.targetTierWeight - Units.ToPx(distance);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = other;
                }
            }
            return best;
        }

        // BOT-07: alan içindeki en yakın görünen yaprak/ganimet → alan içindeki en yakın kesilebilir ağaç → ulu ağaç.
        void ChooseCollectGoal()
        {
            var previousPickup = _pickupGoal;
            _pickupGoal = null;
            _treeGoal = null;
            _moveGoal = null;

            float best = float.MaxValue;
            foreach (var pickup in _perception.Pickups)
            {
                if (pickup == null || _ignoredPickups.Contains(pickup) || !SafeToGo(pickup.Position)) continue;
                float d = (pickup.Position - _self.Position).sqrMagnitude;
                if (d < best) { best = d; _pickupGoal = pickup; }
            }
            if (_pickupGoal != previousPickup) _onPickupSince = -1f;
            if (_pickupGoal != null)
            {
                _moveGoal = _pickupGoal.Position;
                return;
            }

            best = float.MaxValue;
            foreach (var tree in _perception.Trees)
            {
                if (tree == null || !tree.Choppable || tree.Felled || !SafeToGo(tree.Position)) continue;
                float d = (tree.Position - _self.Position).sqrMagnitude;
                if (d < best) { best = d; _treeGoal = tree; }
            }
            if (_treeGoal == null) _treeGoal = Services.World.GreatTree;
            if (_treeGoal != null) _moveGoal = _treeGoal.Position;
        }

        // BOT-10: güvendeyken önce can iksiri (can potionHpRatio altında ve yaprak yetiyorsa), değilse profil sırasındaki
        // ilk üretilebilir geliştirme. Yalnızca Topla ve Kaç durumlarında (Saldır'da hedefe yürürken durmasın).
        RecipeId? ChooseCraft()
        {
            if (State == BotState.Attack || State == BotState.ReturnToZone) return null;
            var crafting = _self.Crafting;
            if (crafting.IsCrafting || !_self.IsSafe) return null;

            if (HpRatio() < Services.Config.bots.potionHpRatio && crafting.CanCraft(RecipeId.Potion, out _))
                return RecipeId.Potion;
            foreach (var stat in Priority())
            {
                var recipe = ToRecipe(stat);
                if (crafting.CanCraft(recipe, out _)) return recipe;
            }
            return null;
        }

        // ---------------- Durumların girdisi ----------------

        // BOT-04: merkeze doğru git; alan dışındayken atılma açıksa kullan.
        CharacterIntent ReturnToZoneIntent()
        {
            var intent = new CharacterIntent();
            intent.Move = _navigator.Steer(_self.Position, Services.Zone.Center);
            intent.Aim = intent.Move.sqrMagnitude > 0f ? intent.Move : _self.Facing;
            if (_perception.OutsideZone && AbilityReady(AbilityId.Dash) && intent.Move.sqrMagnitude > 0f) intent.Dash = true;
            return intent;
        }

        // BOT-05: rakipten uzaklaşıp en yakın çalılığa git; rakip fleeDashEnemyPx'ten yakınsa atıl;
        // rakip balta menzilindeyse kaçarken de ona vur.
        CharacterIntent FleeIntent()
        {
            var bots = Services.Config.bots;
            var intent = new CharacterIntent();
            Vector2 pos = _self.Position;
            if (_target == null || !_target.IsAlive) return CollectIntent();

            Vector2 away = pos - _target.Position;
            float distance = away.magnitude;
            away = distance > 0.0001f ? away / distance : -_self.Facing;

            Vector2 goal = Services.Visibility.NearestBush(pos) ?? (_target.Position + away * Units.ToUnits(bots.enemyNearPx));
            intent.Move = _navigator.Steer(pos, goal);
            intent.Aim = intent.Move.sqrMagnitude > 0f ? intent.Move : away;

            if (AbilityReady(AbilityId.Dash) && distance < Units.ToUnits(bots.fleeDashEnemyPx) && intent.Move.sqrMagnitude > 0f)
                intent.Dash = true;

            if (distance <= Units.ToUnits(Services.Config.axe.rangePx))
            {
                intent.Aim = -away;
                intent.Swing = CanSwingAt(_target);
            }
            return intent;
        }

        // BOT-06: hedefe yaklaş, menzil ve yay içindeyken vur; atılma açıksa 160–300 px'te mesafeyi kapat.
        CharacterIntent AttackIntent()
        {
            var bots = Services.Config.bots;
            var intent = new CharacterIntent();
            if (_target == null || !_target.IsAlive) return CollectIntent();

            Vector2 toTarget = _target.Position - _self.Position;
            float distance = toTarget.magnitude;
            Vector2 direction = distance > 0.0001f ? toTarget / distance : _self.Facing;
            float range = Units.ToUnits(Services.Config.axe.rangePx);

            intent.Aim = direction;
            // Hedef menzile bir karakter yarıçapı kadar girene dek yaklaş, sonra yerinde vur.
            float stopDistance = range - Units.ToUnits(Services.Config.character.radiusPx);
            if (distance > stopDistance) intent.Move = _navigator.Steer(_self.Position, _target.Position);

            float distancePx = Units.ToPx(distance);
            if (AbilityReady(AbilityId.Dash) && distancePx >= bots.attackDashMinPx && distancePx <= bots.attackDashMaxPx
                && NavGrid.Get().LineClear(_self.Position, _target.Position))
            {
                intent.Move = direction;
                intent.Dash = true;
            }

            intent.Swing = CanSwingAt(_target);
            return intent;
        }

        CharacterIntent CollectIntent()
        {
            var intent = new CharacterIntent();
            if (_moveGoal == null) return intent;
            Vector2 toGoal = _moveGoal.Value - _self.Position;

            if (_pickupGoal != null)
            {
                // Yaprağın toplama yarıçapındayız ama yaprak düşme süresinden uzun süredir yerde:
                // envanter o tür için dolu, bu yaprağı bir daha seçme.
                float pickupRadius = Units.ToUnits(Services.Config.inventory.pickupRadiusPx);
                if (toGoal.sqrMagnitude <= pickupRadius * pickupRadius)
                {
                    if (_onPickupSince < 0f) _onPickupSince = Time.time;
                    else if (Time.time - _onPickupSince > Services.Config.leaves.fallMaxSec)
                    {
                        _ignoredPickups.Add(_pickupGoal);
                        _pickupGoal = null;
                        _moveGoal = null;
                    }
                    return intent;
                }
                intent.Move = _navigator.Steer(_self.Position, _moveGoal.Value);
                intent.Aim = intent.Move.sqrMagnitude > 0f ? intent.Move : _self.Facing;
                return intent;
            }

            if (_treeGoal != null && !_treeGoal.Felled)
            {
                intent.Aim = toGoal.sqrMagnitude > 0.0001f ? toGoal.normalized : _self.Facing;
                if (WithinAxeReachOfTree(_treeGoal, toGoal.magnitude))
                    intent.Swing = _treeGoal.Choppable;   // ulu ağaç kesilmez; yanında beklenir
                else
                    intent.Move = _navigator.Steer(_self.Position, _moveGoal.Value);
            }
            return intent;
        }

        // ---------------- Yetenekler (BOT-13) ----------------

        // Kalkan: can shieldHpRatio altında ve shieldEnemyPx içinde rakip varken. Ulti: ultiEnemyPx içinde en az bir rakip varken.
        void UseAbilities(ref CharacterIntent intent)
        {
            var bots = Services.Config.bots;
            Vector2 pos = _self.Position;
            if (AbilityReady(AbilityId.Shield) && HpRatio() < bots.shieldHpRatio
                && _perception.NearestEnemy(pos, Units.ToUnits(bots.shieldEnemyPx)) != null)
                intent.Shield = true;
            if (AbilityReady(AbilityId.Ulti) && _perception.NearestEnemy(pos, Units.ToUnits(bots.ultiEnemyPx)) != null)
                intent.Ulti = true;
        }

        bool AbilityReady(AbilityId ability) => _self.Abilities != null && _self.Abilities.State(ability) == AbilityState.Ready;

        // ---------------- Yardımcılar ----------------

        // BOT-09: yeni hedefi gördükten firstSwingDelaySec sonra, hedef menzil ve yay içindeyken savur.
        bool CanSwingAt(Character target)
        {
            var axe = Services.Config.axe;
            Vector2 toTarget = target.Position - _self.Position;
            if (toTarget.sqrMagnitude > Units.ToUnits(axe.rangePx) * Units.ToUnits(axe.rangePx)) return false;
            if (Time.time - _targetSeenTime < Services.Config.bots.firstSwingDelaySec) return false;
            return toTarget.sqrMagnitude < 0.0001f || Vector2.Angle(_self.Facing, toTarget) <= axe.arcDeg * 0.5f;
        }

        // Gövdeye çarpan bot gövdenin merkezine ulaşamaz; menzil gövdenin kenarına göre ölçülür (Axe ile aynı kural).
        bool WithinAxeReachOfTree(ITree tree, float centerDistance)
        {
            var map = Services.Config.map;
            float trunkPx = ReferenceEquals(tree, Services.World.GreatTree) ? map.greatTreeTrunkRadiusPx : map.treeTrunkRadiusPx;
            return centerDistance <= Units.ToUnits(Services.Config.axe.rangePx + trunkPx);
        }

        // Topla hedefi alan içinde ve zoneLookaheadSec boyunca içeride kalacak olmalı; yoksa bot Alana dön ile
        // Topla arasında gidip gelir (BOT-04 önceliği).
        static bool SafeToGo(Vector2 pos)
        {
            var zone = Services.Zone;
            return zone.IsInside(pos) && !zone.WillBeOutsideWithin(pos, Services.Config.bots.zoneLookaheadSec);
        }

        float HpRatio() => _self.Health.Max > 0 ? (float)_self.Health.Current / _self.Health.Max : 0f;

        // 5.1: profil sırası balance.json'dan. Profil verilmemişse (ör. sandbox'ta elle konan bot) Saldırgan sayılır.
        StatType[] Priority()
        {
            if (_priority != null) return _priority;
            var bots = Services.Config.bots;
            var names = (_self.Profile ?? BotProfile.Aggressive) switch
            {
                BotProfile.Agile => bots.priorityAgile,
                BotProfile.Tough => bots.priorityTough,
                _ => bots.priorityAggressive,
            };
            _priority = new StatType[names.Length];
            for (int i = 0; i < names.Length; i++) _priority[i] = (StatType)System.Enum.Parse(typeof(StatType), names[i]);
            return _priority;
        }

        static RecipeId ToRecipe(StatType stat) => stat switch
        {
            StatType.Power => RecipeId.Power,
            StatType.Speed => RecipeId.Speed,
            _ => RecipeId.Health,
        };

#if UNITY_EDITOR
        // Debug: Scene görünümünde botun durumu ve hedef çizgisi; F4 ile aç/kapa (yalnızca editör).
        static bool _showDebug;
        static int _toggleFrame = -1;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f4Key.wasPressedThisFrame && _toggleFrame != Time.frameCount)
            {
                _toggleFrame = Time.frameCount;
                _showDebug = !_showDebug;
            }
        }

        void OnDrawGizmos()
        {
            if (!_showDebug || !Application.isPlaying || _self == null || !_self.IsAlive) return;
            Vector3 pos = _self.Position;
            Gizmos.color = State switch
            {
                BotState.ReturnToZone => Color.cyan,
                BotState.Flee => Color.yellow,
                BotState.Attack => Color.red,
                _ => Color.green,
            };
            if (_target != null) Gizmos.DrawLine(pos, _target.Position);
            else if (_moveGoal.HasValue) Gizmos.DrawLine(pos, _moveGoal.Value);
            if (_navigator.CurrentWaypoint.HasValue) Gizmos.DrawWireSphere(_navigator.CurrentWaypoint.Value, Units.ToUnits(Services.Config.character.radiusPx) * 0.5f);
            string label = State + (_navigator.Unsticking ? " (takıldı)" : "") + (_self.Crafting.IsCrafting ? " (üretiyor)" : "");
            UnityEditor.Handles.Label(pos + Vector3.up * Units.ToUnits(Services.Config.character.radiusPx * 2f), label);
        }
#endif
    }
}
