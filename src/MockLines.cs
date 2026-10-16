using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SaleStarReminder
{
    // Lines that make fun of the player for spending a Sale Star on a small chest, an Equipment Barrel or a Concept
    // Reliquary while a Large Chest or better was still on the stage. They're read from MockLines.json, which ships next
    // to the DLL, so they can be changed without rebuilding. One is picked at random each time.
    //
    // smallChest is the headline. betterChest is the line under it, shown only with ShowMostValuableChest on; {0} in it
    // is replaced with the name of the best chest that was still on the stage, e.g. "Legendary Chest". Each has a clean
    // list and a vulgar list, and the vulgar one is only used with MockIncludeVulgar on.
    internal static class MockLines
    {
        public const string FileName = "MockLines.json";

        private static string[] smallChest = Array.Empty<string>();
        private static string[] smallChestVulgar = Array.Empty<string>();
        private static string[] betterChest = Array.Empty<string>();
        private static string[] betterChestVulgar = Array.Empty<string>();

        // A missing or unreadable file leaves every list empty, which turns the mocking message off instead of failing.
        public static void Load(string directory)
        {
            string path = Path.Combine(directory, FileName);
            if (!File.Exists(path))
            {
                SaleStarReminderPlugin.Log.LogWarning($"{path} is missing, so the mocking message is off.");
                return;
            }

            LinesFile file;
            try
            {
                file = JsonUtility.FromJson<LinesFile>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                SaleStarReminderPlugin.Log.LogError($"Couldn't read {path}, so the mocking message is off: {e.Message}");
                return;
            }
            if (file == null)
            {
                SaleStarReminderPlugin.Log.LogWarning($"{path} is empty, so the mocking message is off.");
                return;
            }

            smallChest = Clean(file.smallChest?.clean);
            smallChestVulgar = Clean(file.smallChest?.vulgar);
            betterChest = Clean(file.betterChest?.clean);
            betterChestVulgar = Clean(file.betterChest?.vulgar);
            if (SaleStarReminderPlugin.Settings.DebugLogging.Value)
            {
                SaleStarReminderPlugin.Log.LogInfo($"Loaded mocking lines from {path}: {smallChest.Length} + "
                    + $"{smallChestVulgar.Length} vulgar headlines, {betterChest.Length} + {betterChestVulgar.Length}"
                    + " vulgar better-chest lines.");
            }
        }

        public static string PickSmallChest(bool includeVulgar)
        {
            return Pick(smallChest, smallChestVulgar, includeVulgar);
        }

        public static string PickBetterChest(bool includeVulgar, string chestName)
        {
            // Replace rather than string.Format, so a stray brace in a line can't throw.
            return Pick(betterChest, betterChestVulgar, includeVulgar)?.Replace("{0}", chestName);
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

        private static string[] Clean(string[] lines)
        {
            if (lines == null) return Array.Empty<string>();
            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        }

        // The JSON layout, for JsonUtility. Field names must match the file's keys exactly. JsonUtility fills the fields
        // by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [Serializable]
        internal sealed class LinesFile
        {
            public LineSet smallChest;
            public LineSet betterChest;
        }

        [Serializable]
        internal sealed class LineSet
        {
            public string[] clean;
            public string[] vulgar;
        }
#pragma warning restore CS0649
    }
}
