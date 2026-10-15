using System.Collections.Generic;
using RoR2;

namespace SaleStarReminder
{
    // Ranks Sale Star compatible interactables by their name token. Which prefabs are compatible was checked against
    // the game's assets; the ranking itself is a design choice.
    internal static class ChestTiers
    {
        public const string RustyLockboxToken = "LOCKBOX_NAME";

        private static readonly Dictionary<string, ChestTier> ByToken = new Dictionary<string, ChestTier>
        {
            { "EQUIPMENTBARREL_NAME", ChestTier.Lesser },
            { "HOLLOWCHEST_NAME", ChestTier.Lesser },

            { "CHEST1_NAME", ChestTier.Small },
            { "CHEST1_STEALTHED_NAME", ChestTier.Small },
            { "CATEGORYCHEST_DAMAGE_NAME", ChestTier.Small },
            { "CATEGORYCHEST_HEALING_NAME", ChestTier.Small },
            { "CATEGORYCHEST_UTILITY_NAME", ChestTier.Small },

            { "CASINOCHEST_NAME", ChestTier.Adaptive },

            { "CHEST2_NAME", ChestTier.Large },
            { "CATEGORYCHEST2_DAMAGE_NAME", ChestTier.Large },
            { "CATEGORYCHEST2_HEALING_NAME", ChestTier.Large },
            { "CATEGORYCHEST2_UTILITY_NAME", ChestTier.Large },
            { RustyLockboxToken, ChestTier.Large },

            { "GOLDCHEST_NAME", ChestTier.Legendary },
        };

        public static ChestTier Get(PurchaseInteraction chest)
        {
            string token = chest.displayNameToken;
            return token != null && ByToken.TryGetValue(token, out ChestTier tier) ? tier : ChestTier.Unknown;
        }

        // Spending the star on one of these is what the mocking message is for.
        public static bool IsMockable(ChestTier tier)
        {
            return tier == ChestTier.Lesser || tier == ChestTier.Small;
        }
    }
}
