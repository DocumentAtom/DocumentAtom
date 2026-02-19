namespace DocumentAtom.Core.Chunking
{
    using DocumentAtom.Core.Enums;
    using SharpToken;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Splits text into chunks of a fixed token count with optional overlap.
    /// Supports all three overlap strategies: SlidingWindow, SentenceBoundaryAware, and SemanticBoundaryAware.
    /// Uses SharpToken cl100k_base encoding for tokenization.
    /// </summary>
    public static class FixedTokenChunker
    {
        #region Public-Methods

        /// <summary>
        /// Chunk text into fixed-token-count segments with optional overlap.
        /// </summary>
        /// <param name="text">Text to chunk.</param>
        /// <param name="config">Chunking configuration.</param>
        /// <param name="encoding">Token encoding instance.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> Chunk(string text, ChunkingConfiguration config, GptEncoding encoding)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            List<int> tokens = encoding.Encode(text);
            if (tokens.Count == 0) return new List<string>();

            int chunkSize = config.FixedTokenCount;
            int overlapTokens = GetOverlapTokenCount(chunkSize, config);

            List<string> chunks = new List<string>();
            int position = 0;

            while (position < tokens.Count)
            {
                int end = Math.Min(position + chunkSize, tokens.Count);
                List<int> chunkTokens = tokens.GetRange(position, end - position);
                string chunkText = encoding.Decode(chunkTokens);
                chunks.Add(chunkText);

                if (end >= tokens.Count) break;

                int advance = chunkSize - overlapTokens;
                if (advance <= 0) advance = 1;

                if (config.OverlapStrategy == OverlapStrategyEnum.SentenceBoundaryAware && overlapTokens > 0)
                {
                    position = AdjustToSentenceBoundary(text, tokens, position + advance, encoding);
                }
                else if (config.OverlapStrategy == OverlapStrategyEnum.SemanticBoundaryAware && overlapTokens > 0)
                {
                    position = AdjustToParagraphBoundary(text, tokens, position + advance, encoding);
                }
                else
                {
                    position += advance;
                }
            }

            return chunks;
        }

        #endregion

        #region Private-Methods

        private static int GetOverlapTokenCount(int chunkSize, ChunkingConfiguration config)
        {
            if (config.OverlapPercentage.HasValue)
                return (int)(chunkSize * config.OverlapPercentage.Value);
            return config.OverlapCount;
        }

        private static int AdjustToSentenceBoundary(string text, List<int> tokens, int tokenPosition, GptEncoding encoding)
        {
            string decodedUpToPos = encoding.Decode(tokens.GetRange(0, Math.Min(tokenPosition, tokens.Count)));
            int lastSentenceEnd = FindLastSentenceEnd(decodedUpToPos);

            if (lastSentenceEnd > 0)
            {
                string upToSentence = decodedUpToPos.Substring(0, lastSentenceEnd);
                List<int> sentenceTokens = encoding.Encode(upToSentence);
                return sentenceTokens.Count;
            }

            return tokenPosition;
        }

        private static int AdjustToParagraphBoundary(string text, List<int> tokens, int tokenPosition, GptEncoding encoding)
        {
            string decodedUpToPos = encoding.Decode(tokens.GetRange(0, Math.Min(tokenPosition, tokens.Count)));
            int lastParaEnd = decodedUpToPos.LastIndexOf("\n\n", StringComparison.Ordinal);

            if (lastParaEnd > 0)
            {
                string upToPara = decodedUpToPos.Substring(0, lastParaEnd + 2);
                List<int> paraTokens = encoding.Encode(upToPara);
                return paraTokens.Count;
            }

            return tokenPosition;
        }

        private static int FindLastSentenceEnd(string text)
        {
            Regex sentenceEnd = new Regex(@"[.!?][\s]", RegexOptions.RightToLeft);
            Match match = sentenceEnd.Match(text);
            if (match.Success)
                return match.Index + 1;
            return -1;
        }

        #endregion
    }
}
