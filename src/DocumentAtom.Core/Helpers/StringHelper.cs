using DocumentAtom.Core.Helpers;
using System.Text;

namespace DocumentAtom.Core
{
    /// <summary>
    /// String helpers.
    /// </summary>
    public static class StringHelper
    {
        #region Public-Methods

        /// <summary>
        /// Remove binary data from a string.
        /// </summary>
        /// <param name="input">String.</param>
        /// <returns>String.</returns>
        public static string RemoveBinaryData(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return new string(input.Where(c =>
                !char.IsControl(c) ||
                char.IsWhiteSpace(c)).ToArray());
        }

        /// <summary>
        /// Extract substrings from a given string.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="maximumLength">Maximum length.</param>
        /// <param name="shiftSize">Shift size.</param>
        /// <param name="maxWords">Maximum number of words to retrieve.</param>
        /// <returns>Substrings.</returns>
        public static IEnumerable<string> GetSubstringsFromString(string str, int maximumLength, int shiftSize, int maxWords)
        {
            if (String.IsNullOrEmpty(str)) yield break;
            str = str.Trim();
            if (String.IsNullOrEmpty(str)) yield break;

            // Validate parameters
            if (maximumLength <= 0) throw new ArgumentException("Maximum length must be positive", nameof(maximumLength));
            if (shiftSize <= 0) throw new ArgumentException("Shift size must be positive", nameof(shiftSize));

            // If the string is shorter than maximum length, return it as is
            if (str.Length <= maximumLength)
            {
                yield return str;
                yield break;
            }

            int startPosition = 0;

            while (startPosition < str.Length)
            {
                // Calculate the potential end position
                int endPosition = Math.Min(startPosition + maximumLength, str.Length);

                // Get substring that ends on a word boundary
                string substring = GetFullWordsFromRange(str, startPosition, endPosition, maxWords);

                if (!String.IsNullOrEmpty(substring))
                {
                    yield return substring;

                    // Calculate the new start position based on shift size
                    // Find the nearest word boundary after shifting
                    int newStartPosition = startPosition + shiftSize;

                    // Ensure we don't get stuck if shift size is too small
                    if (newStartPosition <= startPosition)
                    {
                        newStartPosition = startPosition + 1;
                    }

                    // Adjust to nearest word boundary
                    while (newStartPosition > 0 &&
                           newStartPosition < str.Length &&
                           !IsSafeWhitespace(str[newStartPosition - 1]))
                    {
                        newStartPosition--;
                    }

                    startPosition = newStartPosition;
                }
                else
                {
                    // Fallback: move forward by one character if we couldn't get a valid substring
                    startPosition++;
                }
            }
        }

        /// <summary>
        /// Retrieve full words from within a range within a supplied string.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <param name="maxWords">Maximum number of words to retrieve.</param>
        /// <returns>String.</returns>
        public static string GetFullWordsFromRange(
            string str,
            int start,
            int end,
            int maxWords)
        {
            if (String.IsNullOrEmpty(str)) return null;

            start = Math.Max(0, Math.Min(str.Length - 1, start));
            end = Math.Max(start, Math.Min(str.Length - 1, end));

            // Adjust start position to include whole words
            while (start > 0 && !IsSafeWhitespace(str[start - 1])) start--;

            // Track word count while scanning
            int wordCount = 0;
            bool inWord = false;
            int adjustedEnd = start;

            // Scan forward from start to count words and find adjusted end position
            for (int i = start; i <= end && i < str.Length; i++)
            {
                if (!IsSafeWhitespace(str[i]))
                {
                    if (!inWord)
                    {
                        inWord = true;
                        wordCount++;
                        if (wordCount > maxWords)
                        {
                            while (adjustedEnd > start && !IsSafeWhitespace(str[adjustedEnd])) adjustedEnd--;
                            break;
                        }
                    }
                    adjustedEnd = i;
                }
                else
                {
                    inWord = false;
                }
            }

            // If we hit the end without exceeding maxWords, adjust end position for partial words
            if (wordCount <= maxWords && adjustedEnd == end && adjustedEnd < str.Length - 1)
            {
                while (adjustedEnd > start && !IsSafeWhitespace(str[adjustedEnd + 1])) adjustedEnd--;
            }

            // Additional safety checks before substring
            if (start > adjustedEnd || start >= str.Length) return string.Empty;

            int length = adjustedEnd - start + 1;
            if (length <= 0 || start + length > str.Length) return string.Empty;

            return str.Substring(start, length);
        }

        #endregion

        #region Private-Methods

        private static bool IsSafeWhitespace(char c)
        {
            // Check if the character is a standard ASCII whitespace character
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        #endregion
    }
}
