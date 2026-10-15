using UnityEngine;

namespace SaleStarReminder
{
    // Lines that make fun of the player for spending a Sale Star on a small chest, an Equipment Barrel or a Concept
    // Reliquary while a Large Chest or better was still on the stage. One is picked at random each time. Fill these in.
    internal static class MockLines
    {
        // The headline.
        public static readonly string[] SmallChest =
        {
            "TODO: small-chest line 1",
            "TODO: small-chest line 2",
        };

        public static readonly string[] SmallChestVulgar =
        {
            "TODO: vulgar small-chest line 1",
            "TODO: vulgar small-chest line 2",
        };

        // The line under it, shown only with ShowMostValuableChest on. {0} is replaced with the name of the best chest
        // that was still on the stage, e.g. "Legendary Chest".
        public static readonly string[] BetterChest =
        {
            "TODO: you could have used it on the {0}",
        };

        public static readonly string[] BetterChestVulgar =
        {
            "TODO: vulgar line about the {0}",
        };

        public static string PickSmallChest(bool includeVulgar)
        {
            return Pick(SmallChest, SmallChestVulgar, includeVulgar);
        }

        public static string PickBetterChest(bool includeVulgar, string chestName)
        {
            // Replace rather than string.Format, so a stray brace in a line can't throw.
            return Pick(BetterChest, BetterChestVulgar, includeVulgar)?.Replace("{0}", chestName);
        }

        // Every line in the pool is equally likely. UnityEngine.Random is local to this client and never touches the
        // run's RNG streams.
        private static string Pick(string[] clean, string[] vulgar, bool includeVulgar)
        {
            int vulgarCount = includeVulgar ? vulgar.Length : 0;
            int total = clean.Length + vulgarCount;
            if (total == 0) return null;
            int index = Random.Range(0, total);
            return index < clean.Length ? clean[index] : vulgar[index - clean.Length];
        }
    }
}
