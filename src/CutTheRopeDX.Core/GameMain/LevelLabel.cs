namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Text for the in-game level label shown when a level starts.
    /// </summary>
    /// <param name="Primary">Large upper line, or <see langword="null"/> when no label should be shown.</param>
    /// <param name="Secondary">Smaller middle line, or <see langword="null"/> when only the primary line applies.</param>
    /// <param name="Tertiary">Smallest bottom line (the level creator), or <see langword="null"/> when absent.</param>
    internal readonly record struct LevelLabelText(string Primary, string Secondary, string Tertiary = null);

    /// <summary>
    /// Decides which lines the in-game level label shows. Performs no localization lookups.
    /// </summary>
    internal static class LevelLabel
    {
        /// <summary>
        /// Builds the label lines for a level start.
        /// </summary>
        /// <param name="isCustomLevel">Whether a custom level is being playtested.</param>
        /// <param name="displayName">Resolved level name, or <see langword="null"/>/empty when the level has none.</param>
        /// <param name="levelWord">Localized word for "Level".</param>
        /// <param name="packAndLevelNumbers">Pack and level numbers, such as <c>1 - 1</c>.</param>
        /// <returns>The lines to render.</returns>
        public static LevelLabelText Resolve(
            bool isCustomLevel,
            string displayName,
            string levelWord,
            string packAndLevelNumbers)
        {
            bool hasName = !string.IsNullOrWhiteSpace(displayName);

            // Playtest runs have no meaningful pack or level number, so the numbering is dropped
            // entirely and an unnamed level shows no label at all.
            return isCustomLevel
                ? hasName ? new LevelLabelText(displayName, null) : new LevelLabelText(null, null)
                : hasName
                    ? new LevelLabelText(displayName, levelWord + " " + packAndLevelNumbers)
                    : new LevelLabelText(packAndLevelNumbers, levelWord);
        }

        /// <summary>
        /// Builds the three-line "Going UP" label: level number, level name, level creator.
        /// </summary>
        /// <param name="goingUpWord">Localized prefix, such as <c>Going UP Level</c>.</param>
        /// <param name="levelNumber">One-based level number within the pack.</param>
        /// <param name="displayName">Resolved level name, or <see langword="null"/>/empty.</param>
        /// <param name="creatorName">Resolved creator name, or <see langword="null"/>/empty.</param>
        /// <returns>The lines to render.</returns>
        public static LevelLabelText ResolveGoingUp(string goingUpWord, string levelNumber, string displayName, string creatorName)
        {
            return new LevelLabelText(
                goingUpWord + " " + levelNumber,
                string.IsNullOrWhiteSpace(displayName) ? null : displayName,
                string.IsNullOrWhiteSpace(creatorName) ? null : creatorName);
        }
    }
}
