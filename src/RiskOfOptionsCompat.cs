using System.IO;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace SaleStarReminder
{
    // Every RiskOfOptions type is touched only inside NoInlining methods, so the mod loads fine without it.
    internal static class RiskOfOptionsCompat
    {
        public const string GUID = "com.rune580.riskofoptions";
        private const string IconResource = "SaleStarReminder.icon.png";

        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(GUID);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Register(PluginConfig settings)
        {
            ModSettingsManager.SetModDescription(
                "Reminds you in large red text while you hold an unused Sale Star.");
            Sprite icon = LoadIcon();
            if (icon)
            {
                ModSettingsManager.SetModIcon(icon);
            }

            ModSettingsManager.AddOption(new CheckBoxOption(settings.ShowReminder));
            ModSettingsManager.AddOption(new StepSliderOption(settings.ReminderFontSize,
                new StepSliderConfig { min = 20f, max = 80f, increment = 1f, FormatString = "{0:0}" }));
            ModSettingsManager.AddOption(new StepSliderOption(settings.ReminderOffsetY,
                new StepSliderConfig { min = -50f, max = 300f, increment = 1f, FormatString = "{0:0}px" }));

            ModSettingsManager.AddOption(new CheckBoxOption(settings.ShowMostValuableChest));

            ModSettingsManager.AddOption(new CheckBoxOption(settings.MockSmallChest));
            ModSettingsManager.AddOption(new CheckBoxOption(settings.MockIncludeVulgar));
            ModSettingsManager.AddOption(new StepSliderOption(settings.MockDurationSeconds,
                new StepSliderConfig { min = 1f, max = 15f, increment = 0.5f, FormatString = "{0:0.0}s" }));

            ModSettingsManager.AddOption(new CheckBoxOption(settings.DebugLogging));
            ModSettingsManager.AddOption(new CheckBoxOption(settings.DebugAlwaysShow));
        }

        // The Thunderstore icon, embedded in the DLL by the csproj.
        private static Sprite LoadIcon()
        {
            using (Stream stream = typeof(RiskOfOptionsCompat).Assembly.GetManifestResourceStream(IconResource))
            {
                if (stream == null) return null;
                using (MemoryStream bytes = new MemoryStream())
                {
                    stream.CopyTo(bytes);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(bytes.ToArray())) return null;
                    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}
