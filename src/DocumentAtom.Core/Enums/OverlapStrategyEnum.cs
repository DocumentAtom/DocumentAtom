namespace DocumentAtom.Core.Enums
{
    /// <summary>
    /// Strategy for handling overlap boundaries between chunks.
    /// </summary>
    public enum OverlapStrategyEnum
    {
        /// <summary>
        /// Mechanical overlap by token count.
        /// </summary>
        SlidingWindow = 0,

        /// <summary>
        /// Adjust overlap to nearest sentence end.
        /// </summary>
        SentenceBoundaryAware = 1,

        /// <summary>
        /// Adjust overlap to nearest paragraph boundary.
        /// </summary>
        SemanticBoundaryAware = 2
    }
}
