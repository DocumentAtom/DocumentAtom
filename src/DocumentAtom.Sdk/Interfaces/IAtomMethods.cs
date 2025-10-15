namespace DocumentAtom.Sdk.Interfaces
{
    using DocumentAtom.Core.Atoms;

    /// <summary>
    /// Interface for document atomization methods.
    /// </summary>
    public interface IAtomMethods
    {
        /// <summary>
        /// Process CSV document and extract atoms.
        /// </summary>
        /// <param name="data">CSV file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessCsv(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process Excel document and extract atoms.
        /// </summary>
        /// <param name="data">Excel file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessExcel(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process HTML document and extract atoms.
        /// </summary>
        /// <param name="data">HTML file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessHtml(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process JSON document and extract atoms.
        /// </summary>
        /// <param name="data">JSON file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessJson(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process Markdown document and extract atoms.
        /// </summary>
        /// <param name="data">Markdown file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessMarkdown(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process OCR on image and extract atoms.
        /// </summary>
        /// <param name="data">Image file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessOcr(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process PDF document and extract atoms.
        /// </summary>
        /// <param name="data">PDF file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessPdf(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process PNG image and extract atoms.
        /// </summary>
        /// <param name="data">PNG file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessPng(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process PowerPoint document and extract atoms.
        /// </summary>
        /// <param name="data">PowerPoint file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessPowerPoint(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process RTF document and extract atoms.
        /// </summary>
        /// <param name="data">RTF file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessRtf(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process text document and extract atoms.
        /// </summary>
        /// <param name="data">Text file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessText(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process Word document and extract atoms.
        /// </summary>
        /// <param name="data">Word file data as byte array.</param>
        /// <param name="extractOcr">Whether to extract text from images using OCR.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessWord(byte[] data, bool extractOcr = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process XML document and extract atoms.
        /// </summary>
        /// <param name="data">XML file data as byte array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of extracted atoms.</returns>
        Task<List<Atom>?> ProcessXml(byte[] data, CancellationToken cancellationToken = default);
    }
}