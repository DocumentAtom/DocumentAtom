namespace DocumentAtom.Core.Chunking
{
    using SharpToken;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Splits text at sentence boundaries, grouping sentences to fill a token budget.
    /// Overlap is measured in sentence count.
    /// </summary>
    public static class SentenceChunker
    {
        #region Private-Members

        private static readonly Regex _SentencePattern = new Regex(
            @"(?<=[.!?])\s+",
            RegexOptions.Compiled);

        #endregion

        #region Public-Methods

        /// <summary>
        /// Chunk text by sentence boundaries.
        /// </summary>
        /// <param name="text">Text to chunk.</param>
        /// <param name="config">Chunking configuration.</param>
        /// <param name="encoding">Token encoding instance.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> Chunk(string text, ChunkingConfiguration config, GptEncoding encoding)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            string[] sentences = _SentencePattern.Split(text);
            sentences = sentences.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (sentences.Length == 0) return new List<string> { text };

            int tokenLimit = config.FixedTokenCount;
            int overlapSentences = GetOverlapSentenceCount(config);

            List<string> chunks = new List<string>();
            int sentenceIndex = 0;

            while (sentenceIndex < sentences.Length)
            {
                List<string> currentSentences = new List<string>();
                int currentTokens = 0;

                while (sentenceIndex < sentences.Length)
                {
                    string sentence = sentences[sentenceIndex];
                    int sentenceTokens = encoding.Encode(sentence).Count;

                    if (currentTokens + sentenceTokens > tokenLimit && currentSentences.Count > 0)
                        break;

                    currentSentences.Add(sentence);
                    currentTokens += sentenceTokens;
                    sentenceIndex++;
                }

                chunks.Add(string.Join(" ", currentSentences));

                if (overlapSentences > 0 && sentenceIndex < sentences.Length)
                {
                    sentenceIndex -= Math.Min(overlapSentences, currentSentences.Count - 1);
                    if (sentenceIndex < 0) sentenceIndex = 0;
                }
            }

            return chunks;
        }

        #endregion

        #region Private-Methods

        private static int GetOverlapSentenceCount(ChunkingConfiguration config)
        {
            if (config.OverlapPercentage.HasValue)
                return Math.Max(1, (int)(config.OverlapPercentage.Value * 5));
            return config.OverlapCount;
        }

        #endregion
    }
}
