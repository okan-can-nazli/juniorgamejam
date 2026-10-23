using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Furkan'ın CharacterVisual'ı gelene kadar geçici görsel: atkı renginde daire ve bakış oku.
    /// Sprite'lar gelince Character.prefab'tan kaldırılır.
    /// </summary>
    public class PlaceholderView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] Transform arrow;

        Character _character;

        void Awake() => _character = GetComponentInParent<Character>();

        void LateUpdate()
        {
            bool visible = _character.IsAlive && _character.VisibleToPlayer;
            body.enabled = visible;
            arrow.gameObject.SetActive(visible);
            if (!visible) return;

            Color color = _character.Scarf switch
            {
                ScarfColor.Blue => new Color(0.25f, 0.5f, 1f),
                ScarfColor.Purple => new Color(0.6f, 0.3f, 0.9f),
                ScarfColor.White => new Color(0.92f, 0.92f, 0.92f),
                _ => new Color(0.15f, 0.15f, 0.15f),
            };
            color.a = _character.IsPlayer && _character.IsInBush ? Services.Config.bushes.playerInBushAlpha : 1f;
            body.color = color;
            float angle = Mathf.Atan2(_character.Facing.y, _character.Facing.x) * Mathf.Rad2Deg;
            arrow.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
