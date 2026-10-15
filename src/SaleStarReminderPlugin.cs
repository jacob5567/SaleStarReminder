using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;

namespace SaleStarReminder
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RiskOfOptionsCompat.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class SaleStarReminderPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "sevenstack.SaleStarReminder";
        public const string PluginName = "SaleStarReminder";
        public const string PluginVersion = PluginVersionInfo.Version;

        internal static ManualLogSource Log;
        internal static PluginConfig Settings;

        private void Awake()
        {
            Log = Logger;
            Settings = new PluginConfig(Config);

            if (RiskOfOptionsCompat.Enabled)
            {
                RiskOfOptionsCompat.Register(Settings);
            }

            HUD.onHudTargetChangedGlobal += OnHudTargetChanged;
        }

        private void OnDestroy()
        {
            HUD.onHudTargetChangedGlobal -= OnHudTargetChanged;
        }

        private void Update()
        {
            if (Settings.DebugLogging.Value && Settings.DumpChestsKey.Value.IsDown())
            {
                CharacterMaster master = LocalUserManager.GetFirstLocalUser()?.cachedMaster;
                ChestScanner.DumpAll(master ? master.inventory : null);
            }
        }

        // Fires whenever any HUD changes the body it is showing, so each HUD gets exactly one controller.
        private static void OnHudTargetChanged(HUD hud)
        {
            if (hud && !hud.GetComponent<SaleStarReminderController>())
            {
                hud.gameObject.AddComponent<SaleStarReminderController>();
            }
        }
    }
}
