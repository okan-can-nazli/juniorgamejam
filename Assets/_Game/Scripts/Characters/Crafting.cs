using LeafBrawl.Shared;
using UnityEngine;

namespace LeafBrawl.Characters
{
    /// <summary>
    /// Tarif üretimi (SRS 3.7, FR-43–FR-46). Tuşlar CurrentIntent.Craft ile, envanter kartı Request ile başlatır.
    /// Üretim 1,5 sn sürer; hasar gelirse iptal olur; yapraklar tamamlanınca düşülür.
    /// </summary>
    public class Crafting : MonoBehaviour
    {
        Character _character;
        float _elapsed;

        public RecipeId? Current { get; private set; }
        public bool IsCrafting => Current.HasValue;
        public float Progress01 => IsCrafting ? Mathf.Clamp01(_elapsed / Services.Config.crafting.durationSec) : 0f;

        /// <summary>Bir sonraki seviyenin (veya iksirin) maliyeti. En üst seviyede boş maliyet döner.</summary>
        public LeafCost Cost(RecipeId recipe)
        {
            var recipes = Services.Config.recipes;
            if (recipe == RecipeId.Potion) return recipes.potion;
            int level = _character.Stats.Level(ToStat(recipe));
            if (level >= _character.Stats.MaxLevel) return default;
            var table = recipe == RecipeId.Power ? recipes.power : recipe == RecipeId.Speed ? recipes.speed : recipes.health;
            return table[level - 1];
        }

        public bool CanCraft(RecipeId recipe, out CraftFail fail)
        {
            fail = default;
            if (IsCrafting) fail = CraftFail.AlreadyCrafting;
            else if (recipe != RecipeId.Potion && _character.Stats.Level(ToStat(recipe)) >= _character.Stats.MaxLevel) fail = CraftFail.MaxLevel;
            else if (!_character.Inventory.Has(Cost(recipe))) fail = CraftFail.NotEnoughLeaves;
            else if (!_character.IsSafe) fail = CraftFail.NotSafe;
            else return true;
            return false;
        }

        /// <summary>Üretimi başlatır. Başlamazsa CraftRejected yayınlar ve false döner.</summary>
        public bool Request(RecipeId recipe)
        {
            if (!_character.IsAlive || Services.Match.State != MatchState.Playing) return false;
            if (!CanCraft(recipe, out var fail))
            {
                GameEvents.RaiseCraftRejected(_character, recipe, fail);
                return false;
            }
            Current = recipe;
            _elapsed = 0f;
            GameEvents.RaiseCraftStarted(_character, recipe);
            return true;
        }

        /// <summary>Hasar veya ölümde Health çağırır. Yaprak harcanmaz.</summary>
        public void Cancel()
        {
            if (!IsCrafting) return;
            var recipe = Current.Value;
            Current = null;
            _elapsed = 0f;
            GameEvents.RaiseCraftCanceled(_character, recipe);
        }

        public static StatType ToStat(RecipeId recipe) => recipe switch
        {
            RecipeId.Power => StatType.Power,
            RecipeId.Speed => StatType.Speed,
            _ => StatType.Health,
        };

        void Awake() => _character = GetComponent<Character>();

        void Update()
        {
            if (_character.CurrentIntent.Craft is RecipeId requested) Request(requested);
            if (!IsCrafting || Services.Match.State != MatchState.Playing) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= Services.Config.crafting.durationSec) Complete();
        }

        void Complete()
        {
            var recipe = Current.Value;
            var cost = Cost(recipe);
            Current = null;
            _elapsed = 0f;
            if (!_character.Inventory.Has(cost))
            {
                GameEvents.RaiseCraftCanceled(_character, recipe);
                return;
            }
            _character.Inventory.Spend(cost);
            if (recipe == RecipeId.Potion)
            {
                _character.Health.Heal(Services.Config.recipes.potionHeal);
                GameEvents.RaiseCraftCompleted(_character, recipe, 0);
                return;
            }
            var stat = ToStat(recipe);
            int newLevel = _character.Stats.Level(stat) + 1;
            _character.Stats.SetLevel(stat, newLevel);
            GameEvents.RaiseCraftCompleted(_character, recipe, newLevel);
        }
    }
}
