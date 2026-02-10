import document_atom_sdk

document_atom_sdk.configure(
    endpoint="http://YOUR_SERVER_URL_HERE:PORT",
)


def extract_pdf_with_ocr():
    """Extract atoms from PDF with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        "path/to/your/file.pdf", ocr=True
    )
    print(result)


# extract_pdf_with_ocr()


def extract_pdf_without_ocr():
    """Extract atoms from PDF without OCR (default behavior)."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        "path/to/your/file.pdf", ocr=False
    )
    print(result)


# extract_pdf_without_ocr()


def extract_powerpoint_with_ocr():
    """Extract atoms from PowerPoint with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_powerpoint(
        "path/to/your/file.pptx", ocr=True
    )
    print(result)


# extract_powerpoint_with_ocr()


def extract_rtf_with_ocr():
    """Extract atoms from RTF with OCR enabled."""
    result = document_atom_sdk.AtomExtraction.extract_atoms_rtf(
        "path/to/your/file.rtf", ocr=True
    )
    print(result)


# extract_rtf_with_ocr()


def extract_using_generic_method_with_ocr():
    """Extract atoms using the generic method with OCR parameter."""
    # For PDF
    result = document_atom_sdk.AtomExtraction.extract_atoms(
        "path/to/your/file.pdf", format_type="pdf", ocr=True
    )
    print(result)

    # For PowerPoint
    result = document_atom_sdk.AtomExtraction.extract_atoms(
        "path/to/your/file.pptx", format_type="powerpoint", ocr=True
    )
    print(result)

    # For RTF
    result = document_atom_sdk.AtomExtraction.extract_atoms(
        "path/to/your/file.rtf", format_type="rtf", ocr=True
    )
    print(result)


# extract_using_generic_method_with_ocr()
