using LeafBrawl.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Klavye + fareden CharacterIntent üretir (SRS 6.1, FR-10, FR-11, FR-40, FR-43). Player.prefab kökünde durur.
    /// Tab envanter panelini açar/kapatır; panel açıkken savurma ve 1–4 tuşları çalışmaz.
    /// </summary>
    public class PlayerIntentSource : MonoBehaviour, IIntentSource
    {
        [SerializeField] InputActionAsset actions;

        InputActionAsset _actions;   // bileşene ait kopya; paylaşılan asset'in durumu sahneler arasında karışmaz
        InputActionMap _map;
        InputAction _move, _aim, _swing, _dash, _shield, _ulti, _inventory;
        readonly InputAction[] _craft = new InputAction[4];
        Vector2 _lastAim = Vector2.down;
        Character _character;

        public bool InventoryOpen { get; private set; }

        public CharacterIntent ReadIntent(Character self)
        {
            Vector2 move = _move.ReadValue<Vector2>();
            if (move.sqrMagnitude > 1f) move.Normalize();

            var camera = Camera.main;
            if (camera != null)
            {
                Vector2 mouseWorld = camera.ScreenToWorldPoint(_aim.ReadValue<Vector2>());
                Vector2 toMouse = mouseWorld - self.Position;
                if (toMouse.sqrMagnitude > 0.0001f) _lastAim = toMouse.normalized;
            }

            RecipeId? craft = null;
            if (!InventoryOpen)
                for (int i = 0; i < _craft.Length; i++)
                    if (_craft[i].WasPressedThisFrame()) { craft = (RecipeId)i; break; }

            return new CharacterIntent
            {
                Move = move,
                Aim = _lastAim,
                Swing = _swing.IsPressed() && !InventoryOpen,
                Dash = _dash.WasPressedThisFrame(),
                Shield = _shield.WasPressedThisFrame(),
                Ulti = _ulti.WasPressedThisFrame(),
                Craft = craft,
            };
        }

        void Awake()
        {
            _character = GetComponent<Character>();
            _actions = Instantiate(actions);
            _map = _actions.FindActionMap("Player", true);
            _move = _map.FindAction("Move", true);
            _aim = _map.FindAction("Aim", true);
            _swing = _map.FindAction("Swing", true);
            _dash = _map.FindAction("Dash", true);
            _shield = _map.FindAction("Shield", true);
            _ulti = _map.FindAction("Ulti", true);
            _inventory = _map.FindAction("Inventory", true);
            for (int i = 0; i < _craft.Length; i++) _craft[i] = _map.FindAction("Craft" + (i + 1), true);
        }

        // Ölünce de açık kalır (Tab vb.); nesneyle birlikte kopya da yok edilir.
        void OnEnable() => _map.Enable();
        void OnDestroy() => Destroy(_actions);

        void Update()
        {
            var state = Services.Match.State;
            // Oyuncu ölünce veya maç bitince panel kapanır; sonuç ekranının üstünde kalmasın (FR-07).
            if (InventoryOpen && (state == MatchState.Ended || !_character.IsAlive))
            {
                InventoryOpen = false;
                GameEvents.RaiseInventoryToggled(false);
                return;
            }
            if (!_inventory.WasPressedThisFrame()) return;
            if (state == MatchState.Paused || state == MatchState.Ended || !_character.IsAlive) return;
            InventoryOpen = !InventoryOpen;
            GameEvents.RaiseInventoryToggled(InventoryOpen);
        }
    }
}
