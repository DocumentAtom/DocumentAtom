namespace DocumentAtom.Core.Enums
{
    /// <summary>
    /// Policy for chunks that fall below the configured minimum token count.
    /// Only applied when a positive minimum chunk token count is configured.
    /// </summary>
    public enum SmallChunkModeEnum
    {
        /// <summary>
        /// Keep small chunks as-is.  This is the default.
        /// </summary>
        Keep = 0,

        /// <summary>
        /// Merge a small chunk forward into the following chunk when the combined size fits the budget.
        /// </summary>
        MergeForward = 1,

        /// <summary>
        /// Drop chunks smaller than the configured minimum.
        /// </summary>
        Drop = 2
    }
}
