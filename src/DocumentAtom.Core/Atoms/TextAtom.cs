namespace DocumentAtom.Core.Atoms
{
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;

    /// <summary>
    /// A text atom is a self-contained unit of text from a document.
    /// </summary>
    public class TextAtom : AtomBase<TextAtom>
    {
        #region Public-Members

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A text atom is a self-contained unit of text from a document.
        /// </summary>
        public TextAtom()
        {

        }

        /// <summary>
        /// Produce an atom with quarks, if chunking is enabled.
        /// </summary>
        /// <param name="text">Text content.</param>
        /// <param name="position">Atom position.</param>
        /// <param name="settings">Chunking settings.</param>
        /// <returns>Text atom.</returns>
        public static TextAtom FromContent(string text, int position, ChunkingSettings settings)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (settings == null) settings = new ChunkingSettings();

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            TextAtom atom = new TextAtom
            {
                Type = AtomTypeEnum.Text,
                Position = position,
                Length = text.Length,
                Text = text,
                MD5Hash = HashHelper.MD5Hash(bytes),
                SHA1Hash = HashHelper.SHA1Hash(bytes),
                SHA256Hash = HashHelper.SHA256Hash(bytes)
            };

            if (settings.Enable)
            {
                if (text.Length >= settings.MaximumLength)
                {
                    int quarkPosition = 0;

                    IEnumerable<string> subStrings = StringHelper.GetSubstringsFromString(text, settings.MaximumLength, settings.ShiftSize);

                    foreach (string substring in subStrings)
                    {
                        if (!string.IsNullOrEmpty(substring))
                        {
                            byte[] substringBytes = Encoding.UTF8.GetBytes(substring);

                            TextAtom quark = new TextAtom
                            {
                                Type = AtomTypeEnum.Text,
                                Position = quarkPosition,
                                Length = substring.Length,
                                Text = substring,
                                MD5Hash = HashHelper.MD5Hash(substringBytes),
                                SHA1Hash = HashHelper.SHA1Hash(substringBytes),
                                SHA256Hash = HashHelper.SHA256Hash(substringBytes),
                                Quarks = null
                            };

                            atom.Quarks.Add(quark);
                            quarkPosition++;
                        }
                    }
                }
            }

            return atom;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
