using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace SaleStarReminder
{
    // RiskUI (bubbet.riskui) replaces the game's HUD prefab with its own. Nothing here references RiskUI's assembly.
    internal static class RiskUICompat
    {
        // The game instantiates the HUD prefab, so a RiskUI HUD object is named "RiskUI(Clone)".
        private const string HudPrefabName = "RiskUI";
        // RiskUI's ChildLocator name for the stack along the top edge: the boss bar, the spectator label, and the item
        // inventory when RiskUI's Inventory setting puts it at the top.
        private const string TopCenterClusterName = "TopCenterCluster";

        // True only when RiskUI is installed and enabled: it swaps in its prefab when a HUD is created, and its Enabled
        // setting only applies to HUDs created after the change, so the HUD itself is the reliable signal.
        public static bool IsRiskUIHud(HUD hud)
        {
            return hud && hud.name.StartsWith(HudPrefabName);
        }

        // RiskUI's own labels (health, money, cooldowns, boss name) use the font its asset bundle ships, while its
        // HGTextMeshProUGUI labels keep the game font. Copies the former from the money counter. Call after the text's
        // Awake, which is where HGTextMeshProUGUI applies the game font.
        public static void ApplyFont(HUD hud, TMP_Text text)
        {
            TextMeshProUGUI source = hud.moneyText ? hud.moneyText.targetText : null;
            if (!source || !source.font) return;

            text.font = source.font;
            text.fontSharedMaterial = source.fontSharedMaterial;
        }

        public static RectTransform FindTopCenterCluster(HUD hud)
        {
            ChildLocator locator = hud.GetComponent<ChildLocator>();
            return locator ? locator.FindChild(TopCenterClusterName) as RectTransform : null;
        }
    }
}
