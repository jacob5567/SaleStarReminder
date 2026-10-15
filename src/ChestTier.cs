namespace SaleStarReminder
{
    // How good a Sale Star chest is to spend the star on, lowest first. Unknown covers compatible chests that aren't in
    // ChestTiers' table (modded or unused ones), so they still count as usable but never win "most valuable".
    internal enum ChestTier
    {
        Unknown,
        Lesser,
        Small,
        Adaptive,
        Large,
        Legendary,
    }
}
