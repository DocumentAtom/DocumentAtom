namespace DocumentAtom.Core.Api
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Request envelope for atomization API endpoints.
    /// Contains optional processing settings and base64-encoded document data.
    /// </summary>
    public class AtomRequest
    {
        #region Public-Members

        /// <summary>
        /// Processing settings for the atomization operation.
        /// When null, server defaults are used.
        /// </summary>
        [JsonPropertyName("Settings")]
        public ApiProcessorSettings? Settings { get; set; } = null;

        /// <summary>
        /// Base64-encoded document data.
        /// </summary>
        [JsonPropertyName("Data")]
        public string Data
        {
            get
            {
                return _Data;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Data));
                _Data = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Data = string.Empty;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Request envelope for atomization API endpoints.
        /// </summary>
        public AtomRequest()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Decode the base64-encoded data to a byte array.
        /// </summary>
        /// <returns>Byte array of the decoded data.</returns>
        /// <exception cref="FormatException">Thrown when the data is not valid base64.</exception>
        public byte[] GetDataBytes()
        {
            return Convert.FromBase64String(_Data);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
