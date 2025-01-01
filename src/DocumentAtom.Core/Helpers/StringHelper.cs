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
        /// <returns>Substrings.</returns>
        public static IEnumerable<string> GetSubstringsFromString(string str, int maximumLength, int shiftSize)
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
                string substring = GetFullWordsFromRange(str, startPosition, endPosition);

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
        /// <returns>String.</returns>
        public static string GetFullWordsFromRange(
            string str,
            int start,
            int end)
        {
            if (String.IsNullOrEmpty(str)) return null;

            // Ensure positions are within bounds
            start = Math.Max(0, start);
            end = Math.Min(str.Length, end);

            // If start is mid-word, move to the beginning of the word
            while (start > 0 && !IsSafeWhitespace(str[start - 1]))
                start--;

            // If end is mid-word, move back to the end of the previous word
            if (end < str.Length)
            {
                while (end > start && !IsSafeWhitespace(str[end - 1]))
                    end--;
            }

            // Ensure we have a valid range
            if (start >= end)
                return string.Empty;

            // Extract and clean the substring
            return str.Substring(start, end - start).Trim();
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
