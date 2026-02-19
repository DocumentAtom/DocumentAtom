namespace DocumentAtom.Sdk.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Core.Api;
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
        public async Task<List<Atom>?> ProcessCsv(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/csv", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessExcel(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/excel", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessHtml(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/html", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessJson(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/json", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessMarkdown(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/markdown", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessOcr(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/atom/ocr";
            AtomRequest request = BuildAtomRequest(data, settings);

            ExtractionResult? extractionResult = await _Sdk.PostJsonAsync<ExtractionResult>(url, request, cancellationToken).ConfigureAwait(false);

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
        public async Task<List<Atom>?> ProcessPdf(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/pdf", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessPng(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/png", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessPowerPoint(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/powerpoint", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessRtf(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/rtf", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessText(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/text", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessWord(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/word", data, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<Atom>?> ProcessXml(byte[] data, ApiProcessorSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return await PostAtomRequest("/atom/xml", data, settings, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Private-Methods

        private AtomRequest BuildAtomRequest(byte[] data, ApiProcessorSettings? settings)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            return new AtomRequest
            {
                Data = Convert.ToBase64String(data),
                Settings = settings
            };
        }

        private async Task<List<Atom>?> PostAtomRequest(
            string route,
            byte[] data,
            ApiProcessorSettings? settings,
            CancellationToken cancellationToken)
        {
            string url = _Sdk.Endpoint + route;
            AtomRequest request = BuildAtomRequest(data, settings);
            return await _Sdk.PostJsonAsync<List<Atom>>(url, request, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
