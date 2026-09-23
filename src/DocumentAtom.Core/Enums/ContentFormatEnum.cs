namespace DocumentAtom.Core.Enums
{
    /// <summary>
    /// Content format that selects the separator ladder used by the Recursive chunking strategy.
    /// Ignored unless <see cref="ChunkStrategyEnum.Recursive"/> is in effect and no explicit
    /// separator list is supplied.
    /// </summary>
    public enum ContentFormatEnum
    {
        /// <summary>
        /// Plain prose.  This is the default.
        /// </summary>
        Plain = 0,

        /// <summary>
        /// Markdown documents (headings, lists, code fences).
        /// </summary>
        Markdown = 1,

        /// <summary>
        /// C# source code.
        /// </summary>
        CSharp = 2,

        /// <summary>
        /// Python source code.
        /// </summary>
        Python = 3,

        /// <summary>
        /// JavaScript source code.
        /// </summary>
        JavaScript = 4,

        /// <summary>
        /// Java source code.
        /// </summary>
        Java = 5
    }
}
