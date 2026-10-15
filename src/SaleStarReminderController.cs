using RoR2;
using RoR2.UI;
using UnityEngine;

namespace SaleStarReminder
{
    // One per HUD. Shows the reminder while the local player holds a usable Sale Star, and notices when the star is
    // spent so it can make fun of a wasted one.
    //
    // Read-only: never call PurchaseInteraction.OnInteractionBegin, SetAvailable or SetUnavailableTemporarily,
    // Inventory.ItemTransformation.TryTransform, or Util.CheckRoll. The spend is detected from the synced inventory.
    internal sealed class SaleStarReminderController : MonoBehaviour
    {
        // The inventory change reaches a client a round trip after the interact press, by which point the interactor
        // has moved off the opened chest. This is how long the last aimed-at chest still counts as the one used.
        private const float UsedChestGrace = 1.5f;
        private const string MostValuablePrefix = "Most valuable chest: ";

        private readonly ChestScanner scanner = new ChestScanner();
        private HUD hud;
        private ReminderBanner banner;

        private CharacterBody driverBody;
        private InteractionDriver driver;
        private GameObject aimedObject;
        private PurchaseInteraction aimedChest;
        private PurchaseInteraction lastAimedChest;
        private float lastAimedTime = float.NegativeInfinity;

        private Inventory trackedInventory;
        private int lastStars;
        private int lastConsumed;

        private float mockUntil = float.NegativeInfinity;
        private string mockLine;
        private string mockSubline;

        private PurchaseInteraction describedBest;
        private string bestSubline;

        private void Awake()
        {
            hud = GetComponent<HUD>();
        }

        private void Update()
        {
            if (!banner)
            {
                if (!hud.mainContainer) return;
                banner = ReminderBanner.Create(hud);
            }

            PluginConfig settings = SaleStarReminderPlugin.Settings;
            // The local player's own master and body, not whoever the HUD is spectating: only your own star matters.
            LocalUser user = hud.localUserViewer;
            CharacterMaster master = user?.cachedMaster;
            Inventory inventory = master ? master.inventory : null;
            CharacterBody body = user?.cachedBody;

            scanner.Refresh(inventory);
            TrackAimedChest(body);
            DetectStarUse(inventory, settings);

            if (Time.time < mockUntil)
            {
                banner.ShowMock(mockLine, mockSubline);
                return;
            }

            bool holdsStar = body && inventory
                && inventory.GetItemCountEffective(DLC2Content.Items.LowerPricedChests) > 0;
            bool show = settings.ShowReminder.Value
                && ((holdsStar && scanner.AnyUsable) || settings.DebugAlwaysShow.Value);
            if (show)
            {
                banner.ShowReminder(settings.ShowMostValuableChest.Value ? DescribeBest(scanner.Best) : null);
            }
            else
            {
                banner.Hide();
            }
        }

        private string DescribeBest(PurchaseInteraction best)
        {
            if (best != describedBest)
            {
                describedBest = best;
                bestSubline = best ? MostValuablePrefix + ChestScanner.GetDisplayName(best) : null;
            }
            return bestSubline;
        }

        // Remembers the last Sale Star chest the player aimed at, which is the one a spent star went into.
        private void TrackAimedChest(CharacterBody body)
        {
            if (body != driverBody)
            {
                driverBody = body;
                driver = body ? body.GetComponent<InteractionDriver>() : null;
            }

            GameObject target = driver ? driver.currentInteractable : null;
            if (target != aimedObject)
            {
                aimedObject = target;
                PurchaseInteraction chest = target ? target.GetComponent<PurchaseInteraction>() : null;
                aimedChest = chest && chest.saleStarCompatible ? chest : null;
            }
            if (aimedChest)
            {
                lastAimedChest = aimedChest;
                lastAimedTime = Time.time;
            }
        }

        // A spend turns every Sale Star into the consumed item at once. Stage-start regen goes the other way, and
        // losing the item some other way doesn't add consumed stars, so neither counts. Totals ignore item disabling.
        private void DetectStarUse(Inventory inventory, PluginConfig settings)
        {
            int stars = inventory ? inventory.GetItemCountTotal(DLC2Content.Items.LowerPricedChests) : 0;
            int consumed = inventory ? inventory.GetItemCountTotal(DLC2Content.Items.LowerPricedChestsConsumed) : 0;
            bool sameInventory = inventory == trackedInventory;
            bool used = sameInventory && inventory && stars < lastStars && consumed > lastConsumed;
            trackedInventory = inventory;
            lastStars = stars;
            lastConsumed = consumed;

            if (used)
            {
                OnStarUsed(settings);
            }
        }

        private void OnStarUsed(PluginConfig settings)
        {
            PurchaseInteraction used = lastAimedChest && Time.time - lastAimedTime <= UsedChestGrace
                ? lastAimedChest
                : null;
            if (!used)
            {
                if (settings.DebugLogging.Value)
                {
                    SaleStarReminderPlugin.Log.LogInfo("Sale Star spent, but no recently aimed-at chest to blame.");
                }
                return;
            }

            ChestTier tier = ChestTiers.Get(used);
            if (settings.DebugLogging.Value)
            {
                SaleStarReminderPlugin.Log.LogInfo(
                    $"Sale Star spent on {used.gameObject.name} ({used.displayNameToken}, tier {tier}).");
            }
            if (!settings.MockSmallChest.Value || !ChestTiers.IsMockable(tier)) return;

            bool includeVulgar = settings.MockIncludeVulgar.Value;
            string line = MockLines.PickSmallChest(includeVulgar);
            if (line == null) return;

            string subline = null;
            if (settings.ShowMostValuableChest.Value)
            {
                PurchaseInteraction better = scanner.GetBestExcluding(used);
                if (better && ChestTiers.Get(better) > ChestTier.Small)
                {
                    subline = MockLines.PickBetterChest(includeVulgar, ChestScanner.GetDisplayName(better));
                }
            }

            mockLine = line;
            mockSubline = subline;
            mockUntil = Time.time + settings.MockDurationSeconds.Value;
        }
    }
}
