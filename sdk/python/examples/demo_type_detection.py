import io

import document_atom_sdk

document_atom_sdk.configure(
    endpoint="http://YOUR_SERVER_URL_HERE:PORT",
)


def detect_type_from_file_path():
    """Detect file type using a file path."""
    result = document_atom_sdk.TypeDetection.detect_type("path/to/your/file.pdf")
    print(f"Detected file type: {result}")


# detect_type_from_file_path()


def detect_type_from_bytes():
    """Detect file type from bytes."""
    with open("path/to/your/file.pdf", "rb") as f:
        file_bytes = f.read()

    result = document_atom_sdk.TypeDetection.detect_type(
        file_bytes, filename="file.pdf"
    )
    print(f"Detected file type: {result}")


# detect_type_from_bytes()


def detect_type_from_bytesio():
    """Detect file type from BytesIO object."""
    with open("path/to/your/file.pdf", "rb") as f:
        file_data = io.BytesIO(f.read())
        file_data.name = "file.pdf"

    result = document_atom_sdk.TypeDetection.detect_type(file_data)
    print(f"Detected file type: {result}")


# detect_type_from_bytesio()
