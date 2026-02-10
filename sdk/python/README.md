<img src="https://raw.githubusercontent.com/jchristn/DocumentAtom/refs/heads/main/assets/icon.png" width="256" height="256">

# DocumentAtom Python SDK

[![PyPI Version](https://img.shields.io/pypi/v/sdk-python.svg?style=flat)](https://pypi.org/project/sdk-python/) [![PyPI Downloads](https://pepy.tech/badge/sdk-python)](https://pepy.tech/project/sdk-python)

DocumentAtom Python SDK is a Python library that provides a simple and efficient way to interact with DocumentAtom server instances. It enables document atomization, type detection, and health monitoring through a clean, async-capable API.

**IMPORTANT** - DocumentAtom Python SDK assumes you have deployed the DocumentAtom REST server. If you are integrating a DocumentAtom library directly into your code, use of this SDK is not necessary.

## Overview

DocumentAtom Python SDK allows you to:

- **Process Documents**: Extract atoms from various document formats (PDF, Word, Excel, PowerPoint, HTML, Markdown, etc.)
- **Detect Document Types**: Automatically identify document types based on content analysis
- **Validate Connectivity**: Check server connection and health
- **Flexible Input**: Support for file paths, bytes, and file-like objects

## Installation

```bash
pip install sdk-python
```

Or install from source:

```bash
git clone https://github.com/DocumentAtom/sdk-python.git
cd sdk-python
pip install -e .
```

## Quick Start

### Basic Usage

```python
from sdk_python import DocumentAtomClient

# Initialize the SDK with base URL
client = DocumentAtomClient(base_url="http://localhost:8000")

# Or use default settings (localhost:8000)
client = DocumentAtomClient()

# Check server connectivity
try:
    client.validate_connectivity()
    print("Server is healthy")
except ConnectionError as e:
    print(f"Connection error: {e}")

# Process a document
result = client.atom_extraction.extract_pdf("document.pdf", ocr=True)

if result and result.atoms:
    for atom in result.atoms:
        print(f"Atom Type: {atom.atom_type}, Content: {atom.content}")
```

## API Reference

### DocumentAtomClient Class

The main SDK class that provides access to all functionality.

#### Constructor

```python
DocumentAtomClient(
    base_url: Optional[str] = None,
    hostname: Optional[str] = None,
    port: Optional[int] = None,
    protocol: Optional[str] = None,
    configuration: Optional[Configuration] = None
)
```

- `base_url`: Full server endpoint URL (e.g., "http://localhost:8000")
- `hostname`: Server hostname (default: "localhost")
- `port`: Server port (default: 8000)
- `protocol`: Protocol to use (default: "http")
- `configuration`: Configuration object

#### Properties

- `configuration`: Configuration object
- `type_detection`: TypeDetection resource instance
- `atom_extraction`: AtomExtraction resource instance

#### Main API Groups

- `type_detection`: Document type detection methods
- `atom_extraction`: Document processing methods

### Document Processing (Atom Extraction Methods)

Process various document types and extract atoms:

#### Supported Document Types

- **CSV**: `extract_csv(file_input, filename=None)`
- **Excel**: `extract_excel(file_input, filename=None)`
- **HTML**: `extract_html(file_input, filename=None)`
- **JSON**: `extract_json(file_input, filename=None)`
- **Markdown**: `extract_markdown(file_input, filename=None)`
- **OCR**: `extract_ocr(file_input, filename=None)`
- **PDF**: `extract_pdf(file_input, ocr=None, filename=None)`
- **PNG**: `extract_png(file_input, filename=None)`
- **PowerPoint**: `extract_powerpoint(file_input, ocr=None, filename=None)`
- **RTF**: `extract_rtf(file_input, ocr=None, filename=None)`
- **Text**: `extract_text(file_input, filename=None)`
- **Word**: `extract_word(file_input, filename=None)`
- **XML**: `extract_xml(file_input, filename=None)`

**Note**: `file_input` can be a file path (str), bytes, or file-like object (BinaryIO/BytesIO). When using bytes or file-like objects, provide the `filename` parameter.

#### Example: Processing Multiple Document Types

```python
from sdk_python import DocumentAtomClient

client = DocumentAtomClient()

# Process a PDF document
pdf_result = client.atom_extraction.extract_pdf("document.pdf", ocr=True)

# Process a Word document
word_result = client.atom_extraction.extract_word("document.docx")

# Process an Excel spreadsheet
excel_result = client.atom_extraction.extract_excel("spreadsheet.xlsx")

# Process from bytes
with open("document.pdf", "rb") as f:
    pdf_bytes = f.read()
pdf_result = client.atom_extraction.extract_pdf(pdf_bytes, filename="document.pdf")
```

### Type Detection

Automatically detect document types:

```python
from sdk_python import DocumentAtomClient

client = DocumentAtomClient()

# Detect document type
result = client.type_detection.detect("unknown-document")

if result:
    print(f"File Type: {result.file_type}")
    print(f"Confidence: {result.confidence}")
    if result.metadata:
        print(f"Metadata: {result.metadata}")

# Detect from bytes
with open("document.pdf", "rb") as f:
    doc_bytes = f.read()
result = client.type_detection.detect(doc_bytes, filename="document.pdf")
```

### Connectivity Validation

Check server connectivity:

```python
from sdk_python import DocumentAtomClient, ConnectionError

client = DocumentAtomClient()

try:
    is_connected = client.validate_connectivity()
    if is_connected:
        print("Server is reachable")
except ConnectionError as e:
    print(f"Connection error: {e}")
```

## Atom Types

The SDK returns `Atom` objects that contain structured document content:

### Atom Properties

- `content: str` - Atom content (text)
- `atom_type: Optional[str]` - Atom type (Text, Image, Binary, Table, List)
- `position: Optional[Dict[str, Any]]` - Position information
- `metadata: Optional[Dict[str, Any]]` - Additional metadata

### Atom Types

Atoms can be of various types depending on the content:

- **Text**: Plain text content
- **Table**: Tabular data with rows and columns
- **Image**: Image data with OCR text
- **Binary**: Binary data
- **List**: Ordered or unordered lists

## Error Handling

The SDK handles errors gracefully and provides custom exceptions:

```python
from sdk_python import (
    DocumentAtomClient,
    APIError,
    ConnectionError,
    ValidationError,
    FileNotFoundError
)

client = DocumentAtomClient()

try:
    result = client.atom_extraction.extract_pdf("document.pdf")

    if result is None or not result.atoms:
        print("Failed to process document or no atoms extracted")
    else:
        print(f"Successfully extracted {len(result.atoms)} atoms")

except FileNotFoundError:
    print("File not found")
except ValidationError as e:
    print(f"Validation error: {e}")
except ConnectionError as e:
    print(f"Connection error: {e}")
except APIError as e:
    print(f"API error: {e}")
except Exception as e:
    print(f"Error: {e}")
```

## Configuration

### Environment Variables

You can configure the SDK using environment variables:

```bash
export DOCUMENTATOM_PROTOCOL=http
export DOCUMENTATOM_HOSTNAME=localhost
export DOCUMENTATOM_PORT=8000
```

### Programmatic Configuration

```python
from sdk_python import DocumentAtomClient, Configuration

# Using Configuration object
config = Configuration(
    hostname="api.example.com",
    port=443,
    protocol="https"
)
client = DocumentAtomClient(configuration=config)

# Or pass parameters directly
client = DocumentAtomClient(
    hostname="api.example.com",
    port=443,
    protocol="https"
)

# Or use base_url
client = DocumentAtomClient(base_url="https://api.example.com:443")
```

### Timeout Configuration

```python
from sdk_python import DocumentAtomClient, Configuration

config = Configuration(
    hostname="localhost",
    port=8000,
    protocol="http",
    timeout=600  # 10 minutes in seconds
)
client = DocumentAtomClient(configuration=config)
```

## Server Requirements

The SDK requires a running DocumentAtom server instance. You can:

1. **Run locally**: Start the DocumentAtom.Server project
2. **Use Docker**: Pull and run the `jchristn/documentatom` Docker image
3. **Deploy**: Deploy to your preferred hosting environment

### Server Setup

```bash
# Using Docker
docker run -p 8000:8000 jchristn/documentatom

# Or run the DocumentAtom.Server project locally
# Default endpoint: http://localhost:8000
```

## Examples

### Complete Example: Document Processing Pipeline

```python
from sdk_python import DocumentAtomClient, ConnectionError, ValidationError
from pathlib import Path

class DocumentProcessor:
    """Example document processor using DocumentAtom SDK."""

    def __init__(self, endpoint: str = "http://localhost:8000"):
        self.client = DocumentAtomClient(base_url=endpoint)

    def process_document(self, file_path: str):
        """Process a document and extract atoms."""
        try:
            # Check server connectivity
            if not self.client.validate_connectivity():
                raise ConnectionError("DocumentAtom server is not reachable")

            # Read document
            path = Path(file_path)
            if not path.exists():
                raise FileNotFoundError(f"File not found: {file_path}")

            # Detect document type
            type_result = self.client.type_detection.detect(file_path)
            print(f"Detected type: {type_result.file_type}")

            # Process based on type
            file_type = type_result.file_type.lower()

            if file_type == "pdf":
                result = self.client.atom_extraction.extract_pdf(file_path, ocr=True)
            elif file_type in ["docx", "doc"]:
                result = self.client.atom_extraction.extract_word(file_path)
            elif file_type in ["xlsx", "xls"]:
                result = self.client.atom_extraction.extract_excel(file_path)
            elif file_type in ["pptx", "ppt"]:
                result = self.client.atom_extraction.extract_powerpoint(file_path, ocr=True)
            elif file_type == "html":
                result = self.client.atom_extraction.extract_html(file_path)
            elif file_type == "md":
                result = self.client.atom_extraction.extract_markdown(file_path)
            elif file_type == "json":
                result = self.client.atom_extraction.extract_json(file_path)
            elif file_type == "xml":
                result = self.client.atom_extraction.extract_xml(file_path)
            elif file_type == "csv":
                result = self.client.atom_extraction.extract_csv(file_path)
            elif file_type == "txt":
                result = self.client.atom_extraction.extract_text(file_path)
            elif file_type == "rtf":
                result = self.client.atom_extraction.extract_rtf(file_path, ocr=True)
            elif file_type == "png":
                result = self.client.atom_extraction.extract_png(file_path)
            else:
                raise ValueError(f"Unsupported document type: {file_type}")

            if result and result.atoms:
                print(f"Extracted {len(result.atoms)} atoms from {file_path}")

                # Process atoms
                for atom in result.atoms:
                    self._process_atom(atom)
            else:
                print(f"No atoms extracted from {file_path}")

        except Exception as e:
            print(f"Error processing {file_path}: {e}")

    def _process_atom(self, atom):
        """Process a single atom."""
        content_preview = atom.content[:100] if atom.content else ""
        print(f"Atom: {atom.atom_type} - {content_preview}...")

        # Process based on atom type
        if atom.atom_type == "Table":
            print(f"  Table detected")
        elif atom.atom_type == "Image":
            print(f"  Image detected")
        elif atom.atom_type == "Text":
            print(f"  Text: {len(atom.content)} characters")

        if atom.metadata:
            print(f"  Metadata: {atom.metadata}")

# Usage
if __name__ == "__main__":
    processor = DocumentProcessor("http://localhost:8000")
    processor.process_document("document.pdf")
```

## Models

### TypeDetectionResult

- `file_type: str` - Detected file type
- `confidence: Optional[float]` - Confidence score
- `metadata: Optional[Dict[str, Any]]` - Additional metadata

### AtomExtractionResult

- `atoms: List[Atom]` - List of extracted atoms
- `metadata: Optional[Dict[str, Any]]` - Additional metadata
- `file_type: Optional[str]` - File type

### Atom

- `content: str` - Atom content (text)
- `atom_type: Optional[str]` - Type of atom (Text, Image, Binary, Table, List)
- `position: Optional[Dict[str, Any]]` - Position information
- `metadata: Optional[Dict[str, Any]]` - Additional metadata

## Dependencies

- **httpx**: HTTP client library
- **pathlib**: Path handling (standard library)

## Version History

Please refer to [CHANGELOG.md](CHANGELOG.md) for version history.

## Contributing

Contributions are welcome! Please feel free to submit issues, enhancement requests, or pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please:

1. Check the [DocumentAtom documentation](https://github.com/jchristn/DocumentAtom)
2. Review the test projects in the repository
3. File an issue in the repository
