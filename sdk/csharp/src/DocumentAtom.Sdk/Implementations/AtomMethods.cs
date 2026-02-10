namespace DocumentAtom.Sdk.Implementations
{
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Image;
    using DocumentAtom.Sdk.Interfaces;

    /// <summary>
    /// Implementation of document atomization methods.
    /// </summary>
    public class AtomMethods : IAtomMethods
    {
        #region Private-Members

        private readonly DocumentAtomSdk _Sdk;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the atom methods implementation.
        /// </summary>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        public AtomMethods(DocumentAtomSdk sdk)
        {
            _Sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
        }

        #endregion

        #region Public-Methods

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessCsv(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/csv";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessExcel(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/excel";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessHtml(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/html";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessJson(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/json";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessMarkdown(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/markdown";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessOcr(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/ocr";

            ExtractionResult? extractionResult = await _Sdk.PostAsync<ExtractionResult>(url, data, cancellationToken).ConfigureAwait(false);

            if (extractionResult == null)
                return null;

            List<Atom> atoms = new List<Atom>();

            if (extractionResult.TextElements != null)
            {
                foreach (TextElement textElement in extractionResult.TextElements)
                {
                    if (!string.IsNullOrEmpty(textElement.Text))
                    {
                        atoms.Add(new Atom
                        {
                            Type = AtomTypeEnum.Text,
                            Text = textElement.Text,
                            Length = textElement.Text.Length,
                            BoundingBox = BoundingBox.FromRectangle(textElement.Bounds)
                        });
                    }
                }
            }

            if (extractionResult.Tables != null)
            {
                foreach (TableStructure table in extractionResult.Tables)
                {
                    Atom tableAtom = Atom.FromTableStructure(table);
                    if (tableAtom != null)
                    {
                        atoms.Add(tableAtom);
                    }
                }
            }

            if (extractionResult.Lists != null)
            {
                foreach (ListStructure list in extractionResult.Lists)
                {
                    if (list.Items != null && list.Items.Count > 0)
                    {
                        atoms.Add(new Atom
                        {
                            Type = AtomTypeEnum.List,
                            UnorderedList = list.IsOrdered ? null : list.Items,
                            OrderedList = list.IsOrdered ? list.Items : null,
                            Length = list.Items.Sum(s => s?.Length ?? 0),
                            BoundingBox = BoundingBox.FromRectangle(list.Bounds)
                        });
                    }
                }
            }

            return atoms;
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessPdf(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/pdf";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessPng(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/png";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessPowerPoint(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/powerpoint";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessRtf(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/rtf";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessText(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/text";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessWord(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/word";
            if (extractOcr)
                url += "?ocr=true";

            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessXml(byte[] data, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/xml";
            return await _Sdk.PostAsync<List<Atom>>(url, data, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}