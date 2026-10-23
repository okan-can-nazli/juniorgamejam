using System.Collections.Generic;
using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Bots
{
    /// <summary>
    /// Bot karar mantığı (SRS Bölüm 5). Bot.prefab kökünde. Oyuncuyla aynı kurallar: bot yalnızca CharacterIntent üretir (BOT-01).
    /// Aşama 1 sürümü: Topla ve Saldır durumları, doğrudan yürüme (yol bulma Aşama 2'de).
    /// </summary>
    public class BotBrain : MonoBehaviour, IIntentSource
    {
        public enum BotState { Collect, Attack }

        Character _self;
        float _nextDecisionTime;

        // Karar (BOT-03: her decisionIntervalSec'te bir)
        Character _target;
        float _targetSeenTime;
        IPickup _pickupGoal;
        float _onPickupSince = -1f;
        ITree _treeGoal;
        Vector2? _moveGoal;

        // Envanteri dolu olduğu için toplanamayan yapraklar: üstüne gelinip yerde kalanlar bir daha seçilmez.
        readonly HashSet<IPickup> _ignoredPickups = new HashSet<IPickup>();

        public BotState State { get; private set; } = BotState.Collect;

        void Awake() => _self = GetComponent<Character>();

        public CharacterIntent ReadIntent(Character self)
        {
            if (Time.time >= _nextDecisionTime)
            {
                _nextDecisionTime = Time.time + Services.Config.bots.decisionIntervalSec;
                Decide();
            }
            return State == BotState.Attack ? AttackIntent() : CollectIntent();
        }

        void Decide()
        {
            var bots = Services.Config.bots;
            var previous = _target;
            _target = PickTarget(Units.ToUnits(bots.enemyNearPx));
            if (_target != previous) _targetSeenTime = Time.time;
            State = _target != null ? BotState.Attack : BotState.Collect;
            if (State == BotState.Collect) ChooseCollectGoal();
        }

        // BOT-08: (yığın kademesi × ağırlık) − mesafe en yüksek görünen rakip. Oyuncu ile bot ayırt edilmez.
        Character PickTarget(float maxDistance)
        {
            var bots = Services.Config.bots;
            Character best = null;
            float bestScore = float.MinValue;
            foreach (var other in Services.Match.Characters)
            {
                if (!CanPerceive(other)) continue;
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

        // BOT-02: yalnızca algı yarıçapındaki ve görünen (çalılıkta gizli olmayan) karakterler.
        bool CanPerceive(Character other)
        {
            if (other == null || other == _self || !other.IsAlive) return false;
            float perception = Units.ToUnits(Services.Config.bots.perceptionRadiusPx);
            if ((other.Position - _self.Position).sqrMagnitude > perception * perception) return false;
            return Services.Visibility.CanSee(_self, other);
        }

        // BOT-07: en yakın yaprak/ganimet → alan içindeki en yakın kesilebilir ağaç → ulu ağaç.
        void ChooseCollectGoal()
        {
            float perceptionPx = Services.Config.bots.perceptionRadiusPx;
            var previousPickup = _pickupGoal;
            _pickupGoal = null;
            _treeGoal = null;
            _moveGoal = null;

            float best = float.MaxValue;
            foreach (var pickup in Services.World.PickupsNear(_self.Position, perceptionPx))
            {
                if (_ignoredPickups.Contains(pickup)) continue;
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
            foreach (var tree in Services.World.TreesNear(_self.Position, perceptionPx))
            {
                if (!tree.Choppable || tree.Felled || !Services.Zone.IsInside(tree.Position)) continue;
                float d = (tree.Position - _self.Position).sqrMagnitude;
                if (d < best) { best = d; _treeGoal = tree; }
            }
            if (_treeGoal == null) _treeGoal = Services.World.GreatTree;
            if (_treeGoal != null) _moveGoal = _treeGoal.Position;
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
                intent.Move = toGoal.normalized;
                intent.Aim = intent.Move;
                return intent;
            }

            if (_treeGoal != null && !_treeGoal.Felled)
            {
                intent.Aim = toGoal.sqrMagnitude > 0.0001f ? toGoal.normalized : _self.Facing;
                if (WithinAxeReachOfTree(_treeGoal, toGoal.magnitude))
                    intent.Swing = _treeGoal.Choppable;   // ulu ağaç kesilmez; yanında beklenir
                else
                    intent.Move = intent.Aim;
            }
            return intent;
        }

        CharacterIntent AttackIntent()
        {
            var intent = new CharacterIntent();
            if (_target == null || !_target.IsAlive) return intent;
            var axe = Services.Config.axe;
            Vector2 toTarget = _target.Position - _self.Position;
            float distance = toTarget.magnitude;
            Vector2 direction = distance > 0.0001f ? toTarget / distance : _self.Facing;
            float range = Units.ToUnits(axe.rangePx);

            intent.Aim = direction;
            // Hedef menzile bir karakter yarıçapı kadar girene dek yaklaş, sonra yerinde vur (BOT-06).
            float stopDistance = range - Units.ToUnits(Services.Config.character.radiusPx);
            if (distance > stopDistance) intent.Move = direction;

            // BOT-09: yeni hedefi gördükten firstSwingDelaySec sonra, hedef menzil ve yay içindeyken savur.
            bool delayPassed = Time.time - _targetSeenTime >= Services.Config.bots.firstSwingDelaySec;
            bool inArc = Vector2.Angle(_self.Facing, direction) <= axe.arcDeg * 0.5f;
            intent.Swing = delayPassed && distance <= range && inArc;
            return intent;
        }

        // Gövdeye çarpan bot gövdenin merkezine ulaşamaz; menzil gövdenin kenarına göre ölçülür (Axe ile aynı kural).
        bool WithinAxeReachOfTree(ITree tree, float centerDistance)
        {
            var map = Services.Config.map;
            float trunkPx = ReferenceEquals(tree, Services.World.GreatTree) ? map.greatTreeTrunkRadiusPx : map.treeTrunkRadiusPx;
            float reach = Units.ToUnits(Services.Config.axe.rangePx + trunkPx);
            return centerDistance <= reach;
        }
    }
}
