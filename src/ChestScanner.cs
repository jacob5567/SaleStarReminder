using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace SaleStarReminder
{
    // Finds the unopened chests on the stage that a Sale Star works on, and picks the most valuable one. Reads only
    // SyncVars (available, cost, costType, displayNameToken) and the prefab's saleStarCompatible flag, so it gives the
    // same answer on clients as on the host.
    internal sealed class ChestScanner
    {
        private const string CategorySeparator = " - ";
        private const string ChestWord = "Chest";
        private const string SmallChestToken = "CHEST1_NAME";
        private const string SmallChestName = "Small Chest";

        private int scannedInstanceCount = -1;
        private int scannedCandidateCount = -1;
        private bool scannedHasKey;
        private PurchaseInteraction best;

        public bool AnyUsable => scannedCandidateCount > 0;

        public PurchaseInteraction Best => best;

        // Counting candidates is a cheap pass; the best chest is only re-picked when that count, the instance count or
        // the key changes, or the cached best stops being a candidate.
        public void Refresh(Inventory inventory)
        {
            bool hasKey = HasRustedKey(inventory);
            List<PurchaseInteraction> all = InstanceTracker.GetInstancesList<PurchaseInteraction>();
            int candidateCount = 0;
            foreach (PurchaseInteraction chest in all)
            {
                if (IsCandidate(chest, hasKey))
                {
                    candidateCount++;
                }
            }

            bool bestStillValid = candidateCount == 0 || (best && IsCandidate(best, hasKey));
            if (all.Count == scannedInstanceCount && candidateCount == scannedCandidateCount && hasKey == scannedHasKey
                && bestStillValid)
            {
                return;
            }

            scannedInstanceCount = all.Count;
            scannedCandidateCount = candidateCount;
            scannedHasKey = hasKey;
            best = FindBest(all, hasKey, null);
        }

        // The best candidate other than exclude, e.g. the chest the star was just spent on, which may still read as
        // available on a client until its SyncVar arrives.
        public PurchaseInteraction GetBestExcluding(PurchaseInteraction exclude)
        {
            if (best != exclude) return best;
            return FindBest(InstanceTracker.GetInstancesList<PurchaseInteraction>(), scannedHasKey, exclude);
        }

        // In English, the plain small chest reads as "Small Chest" rather than just "Chest", and category chests
        // ("Chest - Utility", "Large Chest - Utility") read as "Utility Chest" and "Large Utility Chest". Other languages
        // keep the game's names, since they don't form them the same way.
        public static string GetDisplayName(PurchaseInteraction chest)
        {
            string name = Language.GetString(chest.displayNameToken);
            if (Language.currentLanguageName != "en") return name;
            if (chest.displayNameToken == SmallChestToken) return SmallChestName;

            int dash = name.IndexOf(CategorySeparator, StringComparison.Ordinal);
            if (dash < 0) return name;
            string chestPart = name.Substring(0, dash);
            if (!chestPart.EndsWith(ChestWord, StringComparison.Ordinal)) return name;
            string category = name.Substring(dash + CategorySeparator.Length).Trim();
            return chestPart.Substring(0, chestPart.Length - ChestWord.Length) + category + " " + ChestWord;
        }

        // Logs every purchasable object on the stage and how the mod sees it, to check the tier table in game.
        public static void DumpAll(Inventory inventory)
        {
            bool hasKey = HasRustedKey(inventory);
            List<PurchaseInteraction> all = InstanceTracker.GetInstancesList<PurchaseInteraction>();
            StringBuilder text = new StringBuilder();
            text.AppendLine($"{all.Count} purchasable objects, Rusted Key held: {hasKey}");
            foreach (PurchaseInteraction chest in all)
            {
                if (!chest) continue;
                text.AppendLine($"  {chest.gameObject.name}: token={chest.displayNameToken}"
                    + $" compatible={chest.saleStarCompatible} available={chest.available}"
                    + $" cost={chest.cost} costType={chest.costType} tier={ChestTiers.Get(chest)}"
                    + $" candidate={IsCandidate(chest, hasKey)}");
            }
            SaleStarReminderPlugin.Log.LogInfo(text.ToString());
        }

        // Deliberately not CostTypeDef.IsAffordable or PurchaseInteraction.CanBeAffordedByInteractor: for money chests
        // that would also drop the ones the player can't afford yet. Only the lockbox needs something the player holds.
        private static bool IsCandidate(PurchaseInteraction chest, bool hasKey)
        {
            return chest && chest.saleStarCompatible && chest.available
                && (hasKey || chest.displayNameToken != ChestTiers.RustyLockboxToken);
        }

        private static bool HasRustedKey(Inventory inventory)
        {
            return inventory && inventory.GetItemCountEffective(RoR2Content.Items.TreasureCache) > 0;
        }

        private static PurchaseInteraction FindBest(List<PurchaseInteraction> all, bool hasKey, PurchaseInteraction exclude)
        {
            PurchaseInteraction found = null;
            ChestTier foundTier = ChestTier.Unknown;
            foreach (PurchaseInteraction chest in all)
            {
                if (chest == exclude || !IsCandidate(chest, hasKey)) continue;
                ChestTier tier = ChestTiers.Get(chest);
                if (!found || tier > foundTier
                    || (tier == foundTier && chest.costType == found.costType && chest.cost > found.cost))
                {
                    found = chest;
                    foundTier = tier;
                }
            }
            return found;
        }
    }
}
