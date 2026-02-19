namespace DocumentAtom.Core.Chunking
{
    using SharpToken;

    /// <summary>
    /// Splits text at paragraph boundaries (double newline), grouping paragraphs to fill a token budget.
    /// Overlap is measured in paragraph count.
    /// </summary>
    public static class ParagraphChunker
    {
        #region Public-Methods

        /// <summary>
        /// Chunk text by paragraph boundaries, grouping paragraphs to fill a token budget.
        /// </summary>
        /// <param name="text">Text to chunk.</param>
        /// <param name="config">Chunking configuration.</param>
        /// <param name="encoding">Token encoding instance.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> Chunk(string text, ChunkingConfiguration config, GptEncoding encoding)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            string[] paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            paragraphs = paragraphs.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

            if (paragraphs.Length == 0) return new List<string> { text };

            int tokenLimit = config.FixedTokenCount;
            int overlapParagraphs = GetOverlapParagraphCount(config);

            List<string> chunks = new List<string>();
            int paraIndex = 0;

            while (paraIndex < paragraphs.Length)
            {
                List<string> currentParagraphs = new List<string>();
                int currentTokens = 0;

                while (paraIndex < paragraphs.Length)
                {
                    string paragraph = paragraphs[paraIndex];
                    int paraTokens = encoding.Encode(paragraph).Count;

                    if (currentTokens + paraTokens > tokenLimit && currentParagraphs.Count > 0)
                        break;

                    currentParagraphs.Add(paragraph);
                    currentTokens += paraTokens;
                    paraIndex++;
                }

                chunks.Add(string.Join("\n\n", currentParagraphs));

                if (overlapParagraphs > 0 && paraIndex < paragraphs.Length)
                {
                    paraIndex -= Math.Min(overlapParagraphs, currentParagraphs.Count - 1);
                    if (paraIndex < 0) paraIndex = 0;
                }
            }

            return chunks;
        }

        #endregion

        #region Private-Methods

        private static int GetOverlapParagraphCount(ChunkingConfiguration config)
        {
            if (config.OverlapPercentage.HasValue)
                return Math.Max(1, (int)(config.OverlapPercentage.Value * 5));
            return config.OverlapCount;
        }

        #endregion
    }
}
