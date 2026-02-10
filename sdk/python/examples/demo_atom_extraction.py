import document_atom_sdk

document_atom_sdk.configure(
    endpoint="http://YOUR_SERVER_URL_HERE:PORT",
)


def validate_connectivity():
    """Validate connectivity to the DocumentAtom API."""
    result = document_atom_sdk.Connectivity.validate_connectivity()
    print(f"Connectivity check: {result}")


# validate_connectivity()


def detect_file_type():
    """Detect the type of a file."""
    result = document_atom_sdk.TypeDetection.detect_type("path/to/your/file.pdf")
    print(result)


# detect_file_type()


def extract_atoms_csv():
    """Extract atoms from a CSV file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_csv("path/to/your/file.csv")
    print(result)


# extract_atoms_csv()


def extract_atoms_excel():
    """Extract atoms from an Excel file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_excel(
        "path/to/your/file.xlsx"
    )
    print(result)


# extract_atoms_excel()


def extract_atoms_html():
    """Extract atoms from an HTML file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_html(
        "path/to/your/file.html"
    )
    print(result)


# extract_atoms_html()


def extract_atoms_json():
    """Extract atoms from a JSON file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_json(
        "path/to/your/file.json"
    )
    print(result)


# extract_atoms_json()


def extract_atoms_markdown():
    """Extract atoms from a Markdown file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_markdown(
        "path/to/your/file.md"
    )
    print(result)


# extract_atoms_markdown()


def extract_atoms_ocr():
    """Extract atoms using OCR."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_ocr(
        "path/to/your/image.png"
    )
    print(result)


# extract_atoms_ocr()


def extract_atoms_pdf():
    """Extract atoms from a PDF file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf("path/to/your/file.pdf")
    print(result)


# extract_atoms_pdf()


def extract_atoms_pdf_with_ocr():
    """Extract atoms from a PDF file with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        "path/to/your/file.pdf", ocr=True
    )
    print(result)


# extract_atoms_pdf_with_ocr()


def extract_atoms_png():
    """Extract atoms from a PNG file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_png(
        "path/to/your/image.png"
    )
    print(result)


# extract_atoms_png()


def extract_atoms_powerpoint():
    """Extract atoms from a PowerPoint file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_powerpoint(
        "path/to/your/file.pptx"
    )
    print(result)


# extract_atoms_powerpoint()


def extract_atoms_powerpoint_with_ocr():
    """Extract atoms from a PowerPoint file with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_powerpoint(
        "path/to/your/file.pptx", ocr=True
    )
    print(result)


# extract_atoms_powerpoint_with_ocr()


def extract_atoms_rtf():
    """Extract atoms from an RTF file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_rtf("path/to/your/file.rtf")
    print(result)


# extract_atoms_rtf()


def extract_atoms_rtf_with_ocr():
    """Extract atoms from an RTF file with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_rtf(
        "path/to/your/file.rtf", ocr=True
    )
    print(result)


# extract_atoms_rtf_with_ocr()


def extract_atoms_text():
    """Extract atoms from a text file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_text(
        "path/to/your/file.txt"
    )
    print(result)


# extract_atoms_text()


def extract_atoms_word():
    """Extract atoms from a Word file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_word(
        "path/to/your/file.docx"
    )
    print(result)


# extract_atoms_word()


def extract_atoms_xml():
    """Extract atoms from an XML file."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_xml("path/to/your/file.xml")
    print(result)


# extract_atoms_xml()


def extract_atoms_generic():
    """Extract atoms using the generic method with format type."""
    result = document_atom_sdk.AtomExtraction.extract_atoms(
        "path/to/your/file.pdf", format_type="pdf", ocr=True
    )
    print(result)


# extract_atoms_generic()
