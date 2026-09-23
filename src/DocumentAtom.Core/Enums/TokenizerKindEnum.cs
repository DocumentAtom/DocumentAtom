namespace DocumentAtom.Core.Enums
{
    /// <summary>
    /// Tokenizer family used to measure chunk sizes.
    /// </summary>
    public enum TokenizerKindEnum
    {
        /// <summary>
        /// Resolve the tokenizer automatically from the model identifier and API format.
        /// This is the default.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// OpenAI cl100k_base byte-pair encoding (GPT-3.5/GPT-4, text-embedding-3-*).
        /// </summary>
        Cl100kBase = 1,

        /// <summary>
        /// OpenAI o200k_base byte-pair encoding (GPT-4o and o-series models).
        /// </summary>
        O200kBase = 2,

        /// <summary>
        /// BERT WordPiece encoding (MiniLM, BGE, E5, and other sentence-transformer models).
        /// </summary>
        BertWordPiece = 3
    }
}
