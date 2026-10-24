using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// CurrentIntent.Move'dan 8 yönlü hareket (FR-10). Rigidbody2D Obstacle katmanına çarpar (FR-12).
    /// Hız = Stats.SpeedPx × SpeedMultiplier; çapraz hareket aynı hızdadır.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
        Rigidbody2D _body;
        Character _character;
        Vector2 _dashVelocity;
        int _dashStepsLeft;

        /// <summary>Birim/sn.</summary>
        public Vector2 Velocity => _body.linearVelocity;
        public bool IsDashing => _dashStepsLeft > 0;

        /// <summary>Engellere çarparak kısa itme (Hamza'nın atılması çağırır, SRS 3.4).</summary>
        public void Dash(Vector2 direction, float distancePx, float duration)
        {
            if (direction.sqrMagnitude < 0.0001f || duration <= 0f || !_character.IsAlive) return;
            // Süre fizik adımı olarak baştan sayılır: kayan noktayla süre saymak fazladan bir adım attırıyordu (~178 px).
            _dashStepsLeft = Mathf.Max(1, Mathf.RoundToInt(duration / Time.fixedDeltaTime));
            _dashVelocity = direction.normalized * (Units.ToUnits(distancePx) / (_dashStepsLeft * Time.fixedDeltaTime));
        }

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _character = GetComponent<Character>();
        }

        void FixedUpdate()
        {
            if (!_character.IsAlive)
            {
                _dashStepsLeft = 0;
                return;
            }
            if (IsDashing)
            {
                _body.linearVelocity = _dashVelocity;
                _dashStepsLeft--;
                return;
            }
            Vector2 move = _character.CurrentIntent.Move;
            if (move.sqrMagnitude > 1f) move.Normalize();
            float speed = Units.ToUnits(_character.Stats.SpeedPx * _character.SpeedMultiplier);
            _body.linearVelocity = move * speed;
        }
    }
}
