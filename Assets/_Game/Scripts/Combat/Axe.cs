using LeafBrawl.Characters;
using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Combat
{
    /// <summary>
    /// Balta savurma ve vuruş çözümü (SRS 3.3, FR-20–FR-22). CharacterCombat.prefab üzerinde, karakterin çocuğunda durur.
    /// Savuruş: Swung + MarkSwing → hitDelaySec sonra hasar → cooldownSec dolunca yeni savuruş.
    /// Savurma süresi bekleme süresi kadardır; bu sürede karakter swingSpeedMultiplier hızla yürür.
    /// </summary>
    public class Axe : MonoBehaviour
    {
        Character _self;
        AbilityController _abilities;
        int _obstacleMask, _treeMask;
        readonly RaycastHit2D[] _lineHits = new RaycastHit2D[8];
        readonly Collider2D[] _treeHits = new Collider2D[16];

        float _swingStartTime = float.NegativeInfinity;
        bool _hitPending;

        /// <summary>Savurma başladıktan sonra bekleme süresi dolana kadar true.</summary>
        public bool IsSwinging => Time.time - _swingStartTime < Services.Config.axe.cooldownSec;

        void Awake()
        {
            _self = GetComponentInParent<Character>();
            _abilities = GetComponent<AbilityController>();
            _obstacleMask = LayerMask.GetMask("Obstacle");
            _treeMask = LayerMask.GetMask("Tree");
        }

        void Update()
        {
            var axe = Services.Config.axe;

            if (!_self.IsAlive)
            {
                _hitPending = false;
                _self.SpeedMultiplier = 1f;
                return;
            }

            // Hasar anı: savurma başlangıcından hitDelaySec sonra (yön o anki bakış yönü).
            if (_hitPending && Time.time - _swingStartTime >= axe.hitDelaySec)
            {
                _hitPending = false;
                ResolveHit();
            }

            // FR-20: basılı tutuldukça bekleme bittikçe yeniden savrulur. Üretim sırasında (SRS 3.7) ve ulti hazırlanırken saldırı yok.
            bool ultiWindingUp = _abilities != null && _abilities.UltiWindingUp;
            if (!IsSwinging && _self.CurrentIntent.Swing && !_self.Crafting.IsCrafting && !ultiWindingUp)
                StartSwing();

            // Hız çarpanını yalnızca burası yazar: ulti hazırlanırken 0 (SRS 3.4), savururken %50 (FR-22).
            _self.SpeedMultiplier = ultiWindingUp ? 0f : IsSwinging ? axe.swingSpeedMultiplier : 1f;
        }

        void StartSwing()
        {
            _swingStartTime = Time.time;
            _hitPending = true;
            _self.MarkSwing();
            GameEvents.RaiseSwung(_self);
        }

        void ResolveHit()
        {
            var axe = Services.Config.axe;
            Vector2 origin = _self.Position;
            Vector2 facing = _self.Facing;
            float range = Units.ToUnits(axe.rangePx);
            float halfArc = axe.arcDeg * 0.5f;
            int damage = _self.Stats.Damage;

            // Karakterler: yay içindeki bütün canlı rakipler, her biri bir kez (SRS 3.3).
            foreach (var target in Services.Match.Characters)
            {
                if (target == null || target == _self || !target.IsAlive) continue;
                Vector2 toTarget = target.Position - origin;
                if (toTarget.sqrMagnitude > range * range) continue;
                if (!InArc(facing, toTarget, halfArc)) continue;
                if (IsBlocked(origin, target.Position, null)) continue;

                target.ApplyDamage(new DamageInfo { Amount = damage, Source = _self });
                GameEvents.RaiseHitLanded(_self, target, damage);
            }

            HitTrees(origin, facing, range, halfArc, damage, axe.maxTreesPerSwing);
        }

        // En fazla maxTrees ağaç: yay içindeki en yakın gövde(ler). Menzil gövdenin kenarına göre ölçülür,
        // çünkü karakter gövdenin içine giremez (ulu ağaç gövdesi 40 px, menzil 60 px).
        void HitTrees(Vector2 origin, Vector2 facing, float range, float halfArc, int damage, int maxTrees)
        {
            if (maxTrees <= 0) return;
            int count = Physics2D.OverlapCircle(origin, range, ContactFilterFor(_treeMask), _treeHits);

            for (int hitIndex = 0; hitIndex < maxTrees; hitIndex++)
            {
                ITree best = null;
                float bestDistance = float.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    var tree = _treeHits[i] != null ? _treeHits[i].GetComponentInParent<ITree>() : null;
                    if (tree == null || tree.Felled) continue;
                    Vector2 closest = _treeHits[i].ClosestPoint(origin);
                    Vector2 toTree = closest - origin;
                    if (!InArc(facing, toTree, halfArc)) continue;
                    if (IsBlocked(origin, closest, tree)) continue;
                    float distance = toTree.sqrMagnitude;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = tree;
                    }
                }
                if (best == null) return;
                best.ApplyDamage(new DamageInfo { Amount = damage, Source = _self });
                for (int i = 0; i < count; i++)
                    if (_treeHits[i] != null && ReferenceEquals(_treeHits[i].GetComponentInParent<ITree>(), best))
                        _treeHits[i] = null;
            }
        }

        static bool InArc(Vector2 facing, Vector2 toTarget, float halfArcDeg)
        {
            // Hedef tam üstündeyse yön belirsiz; vurulmuş sayılır.
            if (toTarget.sqrMagnitude < 0.0001f) return true;
            return Vector2.Angle(facing, toTarget) <= halfArcDeg;
        }

        // Merkezden hedefe çizgi bir gövdeden veya kayadan geçiyorsa vuruş işlemez (SRS 3.3).
        // Hedef ağacın kendi gövdesi engel sayılmaz.
        bool IsBlocked(Vector2 from, Vector2 to, ITree ignoreTree)
        {
            int count = Physics2D.Linecast(from, to, ContactFilterFor(_obstacleMask), _lineHits);
            for (int i = 0; i < count; i++)
            {
                var hit = _lineHits[i].collider;
                if (ignoreTree != null && ReferenceEquals(hit.GetComponentInParent<ITree>(), ignoreTree)) continue;
                return true;
            }
            return false;
        }

        static ContactFilter2D ContactFilterFor(int mask)
        {
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(mask);
            return filter;
        }

#if UNITY_EDITOR
        // Editörde savurma yayı ve menzil (Scene görünümünde, oyun içinde görünmez).
        void OnDrawGizmos()
        {
            if (_self == null || !Application.isPlaying) return;
            var axe = Services.Config.axe;
            float range = Units.ToUnits(axe.rangePx);
            Vector3 origin = _self.Position;
            Vector3 facing = _self.Facing;
            Gizmos.color = IsSwinging ? new Color(1f, 0.3f, 0.2f, 0.9f) : new Color(1f, 1f, 1f, 0.35f);
            Vector3 left = Quaternion.Euler(0, 0, axe.arcDeg * 0.5f) * facing * range;
            Vector3 right = Quaternion.Euler(0, 0, -axe.arcDeg * 0.5f) * facing * range;
            Gizmos.DrawLine(origin, origin + left);
            Gizmos.DrawLine(origin, origin + right);
            const int segments = 12;
            Vector3 previous = origin + right;
            for (int i = 1; i <= segments; i++)
            {
                float angle = -axe.arcDeg * 0.5f + axe.arcDeg * i / segments;
                Vector3 next = origin + Quaternion.Euler(0, 0, angle) * facing * range;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
#endif
    }
}
