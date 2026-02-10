import io
from pathlib import Path

import document_atom_sdk

document_atom_sdk.configure(
    endpoint="http://YOUR_SERVER_URL_HERE:PORT",
)


def extract_from_file_path():
    """
    Method 1: File path (string) - simplest way
    Just pass the file path as a string. The SDK will read the file automatically.
    """
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        "path/to/your/file.pdf", ocr=True
    )
    print(result)


# extract_from_file_path()


def extract_from_absolute_path():
    """Extract atoms using an absolute file path."""
    file_path = Path(__file__).parent / "demo_files" / "sample.pdf"
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        str(file_path), ocr=True
    )
    print(result)


# extract_from_absolute_path()


def extract_from_bytes():
    """
    Method 2: Bytes - read file into memory first
    Read the file as bytes, then pass the bytes with a filename.
    Useful when you already have file content in memory.
    """
    with open("path/to/your/file.pdf", "rb") as f:
        file_bytes = f.read()

    # Note: filename parameter is required when passing bytes
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
        file_bytes, filename="file.pdf", ocr=True
    )
    print(result)


# extract_from_bytes()


def extract_from_bytesio():
    """
    Method 3: BytesIO - file-like object in memory
    Create a BytesIO object from file content. Useful for streaming or
    when working with file-like objects.
    """
    with open("path/to/your/file.pdf", "rb") as f:
        file_data = io.BytesIO(f.read())
        file_data.name = "file.pdf"

    # Note: filename parameter is required when BytesIO doesn't have a name attribute
    result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(file_data, ocr=True)
    print(result)


# extract_from_bytesio()


def extract_from_binary_io():
    """
    Method 4: BinaryIO - file-like object
    Pass an open file handle directly.
    """
    with open("path/to/your/file.pdf", "rb") as file_handle:
        result = document_atom_sdk.AtomExtraction.extract_atoms_pdf(
            file_handle, ocr=True
        )
        print(result)


# extract_from_binary_io()
