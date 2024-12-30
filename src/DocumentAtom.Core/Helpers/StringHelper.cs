namespace DocumentAtom.Core
{
    /// <summary>
    /// String helpers.
    /// </summary>
    public static class StringHelper
    {
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
    }
}
