namespace DocumentAtom.Core.Chunking
{
    using SharpToken;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Splits text at boundaries defined by a user-supplied regular expression.
    /// Compiled with RegexOptions.Compiled | RegexOptions.Multiline and a 5-second timeout for ReDoS protection.
    /// </summary>
    public static class RegexChunker
    {
        #region Public-Methods

        /// <summary>
        /// Chunk text by regex-defined boundaries.
        /// </summary>
        /// <param name="text">Text to chunk.</param>
        /// <param name="config">Chunking configuration.  RegexPattern must be non-empty.</param>
        /// <param name="encoding">Token encoding instance.</param>
        /// <returns>List of chunked text segments.</returns>
        /// <exception cref="ArgumentException">Thrown when RegexPattern is null or empty.</exception>
        public static List<string> Chunk(string text, ChunkingConfiguration config, GptEncoding encoding)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            if (string.IsNullOrEmpty(config.RegexPattern))
                throw new ArgumentException("RegexPattern is required when using RegexBased strategy.");

            Regex regex = new Regex(
                config.RegexPattern,
                RegexOptions.Compiled | RegexOptions.Multiline,
                TimeSpan.FromSeconds(5));

            string[] segments = regex.Split(text);
            List<string> filtered = segments
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (filtered.Count == 0) return new List<string> { text };

            return filtered;
        }

        #endregion
    }
}
