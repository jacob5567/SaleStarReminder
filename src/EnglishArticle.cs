using System;

namespace SaleStarReminder
{
    // Picks "a" or "an" by the sound of the next word, not its first letter: "a Utility Chest", "an Adaptive Chest",
    // "an hour". Covers the common exceptions; chest names from other mods can still be named, so it isn't limited to
    // the game's own.
    internal static class EnglishArticle
    {
        // Start with a vowel letter but a consonant sound ("you", "wuh").
        private static readonly string[] ConsonantSoundPrefixes =
        {
            "uni", "use", "usu", "uti", "ure", "uri", "uro", "ubiq", "eu", "ewe", "one", "once",
        };

        // Start with a consonant letter but a vowel sound (silent "h").
        private static readonly string[] VowelSoundPrefixes =
        {
            "hour", "honest", "honor", "honour", "heir",
        };

        public static string For(string phrase)
        {
            string word = phrase.TrimStart().ToLowerInvariant();
            if (word.Length == 0) return "a";
            if (StartsWithAny(word, ConsonantSoundPrefixes)) return "a";
            if (StartsWithAny(word, VowelSoundPrefixes)) return "an";
            return "aeiou".IndexOf(word[0]) >= 0 ? "an" : "a";
        }

        private static bool StartsWithAny(string word, string[] prefixes)
        {
            foreach (string prefix in prefixes)
            {
                if (word.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
