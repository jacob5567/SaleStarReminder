using BepInEx.Configuration;
using UnityEngine;

namespace SaleStarReminder
{
    internal sealed class PluginConfig
    {
        public readonly ConfigEntry<bool> ShowReminder;
        public readonly ConfigEntry<float> ReminderFontSize;
        public readonly ConfigEntry<float> SubheaderFontSize;
        public readonly ConfigEntry<float> ReminderOffsetY;
        public readonly ConfigEntry<float> NearChestScale;
        public readonly ConfigEntry<bool> PulseNearChest;

        public readonly ConfigEntry<bool> ShowMostValuableChest;

        public readonly ConfigEntry<bool> MockSmallChest;
        public readonly ConfigEntry<bool> MockIncludeVulgar;
        public readonly ConfigEntry<float> MockDurationSeconds;

        public readonly ConfigEntry<bool> DebugLogging;
        public readonly ConfigEntry<bool> DebugAlwaysShow;
        public readonly ConfigEntry<KeyboardShortcut> DumpChestsKey;

        public PluginConfig(ConfigFile config)
        {
            ShowReminder = config.Bind("Reminder", "ShowReminder", true,
                "Show large red text at the top of the screen while you hold an unused Sale Star and the stage has a chest it works on.");
            ReminderFontSize = config.Bind("Reminder", "ReminderFontSize", 80f,
                "Font size of the main alert (UNUSED SALE STAR, or the line making fun of you), in UI points.");
            SubheaderFontSize = config.Bind("Reminder", "SubheaderFontSize", 20f,
                "Font size of the smaller line under the main alert, in UI points.");
            ReminderOffsetY = config.Bind("Reminder", "ReminderOffsetY", 8f,
                "Gap between the bottom of the item box at the top of the screen and the reminder text, in UI pixels.");
            NearChestScale = config.Bind("Reminder", "NearChestScale", 1.3f,
                "How much bigger the main alert gets while the game circles a chest your Sale Star works on (when you look at it up close). 1 turns this off.");
            PulseNearChest = config.Bind("Reminder", "PulseNearChest", true,
                "While the game circles a chest your Sale Star works on, fade the main alert in and out, twice per second. Turn this off if any pulsing or flashing bothers you.");

            ShowMostValuableChest = config.Bind("MostValuableChest", "ShowMostValuableChest", false,
                "Under the reminder, name the most valuable unopened chest on the stage that the Sale Star works on. This includes chests you haven't found yet.");

            MockSmallChest = config.Bind("Mock", "MockSmallChest", false,
                "Make fun of you for a few seconds after you spend a Sale Star on a small chest, an Equipment Barrel or a Concept Reliquary while a Large Chest, Rusty Lockbox or Legendary Chest was still available. With ShowMostValuableChest on, also names that chest.");
            MockIncludeVulgar = config.Bind("Mock", "MockIncludeVulgar", true,
                "Also pick from the vulgar lines when making fun of you.");
            MockDurationSeconds = config.Bind("Mock", "MockDurationSeconds", 6f,
                "How long the mocking message stays on screen, in seconds.");

            DebugLogging = config.Bind("Debug", "DebugLogging", false,
                "Log each Sale Star use the mod detects, and enable the chest dump key.");
            DebugAlwaysShow = config.Bind("Debug", "DebugAlwaysShow", false,
                "Show the reminder even without a Sale Star or a usable chest, for testing.");
            DumpChestsKey = config.Bind("Debug", "DumpChestsKey", new KeyboardShortcut(KeyCode.F8),
                "With DebugLogging on, press to log every purchasable object in the current stage and how the mod ranks it.");
        }
    }
}
