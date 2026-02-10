import io
from pathlib import Path
from typing import Any, BinaryIO, Dict, Optional, Union

from .configuration import get_client
from .enums.severity_enum import Severity_Enum
from .exceptions import FileNotFoundError, SdkException, ValidationError
from .sdk_logging import log_error
from .utils.url_helper import _get_url_base

JSON_CONTENT_TYPE = {"Content-Type": "application/json"}


class ConnectivityCheckableAPIResource:
    """
    Mixin class for connectivity validation.

    This mixin provides a method to validate connectivity to the API.
    """

    @classmethod
    def validate_connectivity(cls) -> bool:
        """
        Validate connectivity to the DocumentAtom API.

        Returns:
            True if connection is successful

        Raises:
            SdkException: If connection fails
        """
        client = get_client()

        try:
            # For connectivity check, we just use the base URL (root path)
            url = ""
            client.request("HEAD", url)
            return True
        except Exception as e:
            log_error(
                Severity_Enum.Error.value, f"Failed to validate connectivity: {str(e)}"
            )
            raise SdkException(f"Failed to validate connectivity: {str(e)}") from e


class TypeDetectableAPIResource:
    """
    Mixin class for type detection operations.

    This mixin provides a method to detect the type of a file.
    """

    RESOURCE_NAME: str = "typedetect"

    @classmethod
    def _prepare_file_input(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> tuple[BinaryIO, str]:
        """
        Prepare file input for upload. Accepts file path, bytes, or file-like object.

        Args:
            file_input: File path (str), bytes, or file-like object
            filename: Optional filename (required if file_input is bytes or BytesIO)

        Returns:
            Tuple of (file_object, filename)
        """
        if isinstance(file_input, str):
            # File path
            path = Path(file_input)
            if not path.exists():
                raise FileNotFoundError(f"File does not exist: {file_input}")
            if not path.is_file():
                raise ValidationError(f"Path is not a file: {file_input}")
            return open(path, "rb"), path.name
        elif isinstance(file_input, bytes):
            # Raw bytes
            if not filename:
                raise ValidationError("filename is required when file_input is bytes")
            return io.BytesIO(file_input), filename
        elif isinstance(file_input, (io.BytesIO, BinaryIO)):
            # File-like object
            if not filename:
                # Try to get filename from file object if it has a name attribute
                filename = getattr(file_input, "name", "file")
                if filename == "file":
                    raise ValidationError(
                        "filename is required when file_input is a file-like object without a name"
                    )
            return file_input, filename
        else:
            raise ValidationError(
                f"file_input must be str (file path), bytes, or file-like object, got {type(file_input)}"
            )

    @classmethod
    def detect_type(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """
        Detect the type of a file.

        Args:
            file_input: File path (str), bytes, or file-like object (BinaryIO/BytesIO)
            filename: Optional filename (required if file_input is bytes or file-like object without a name)

        Returns:
            Dictionary containing detected file type and metadata

        Raises:
            ValidationError: If file path is invalid
            SdkException: If API request fails
        """
        client = get_client()

        # Prepare file input (handles path, bytes, or file-like object)
        file_obj, file_name = cls._prepare_file_input(file_input, filename)

        url = _get_url_base(cls)

        # Get MIME type from filename
        ext = Path(file_name).suffix.lower()
        mime_map = {
            ".pdf": "application/pdf",
            ".png": "image/png",
            ".jpg": "image/jpeg",
            ".jpeg": "image/jpeg",
            ".docx": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc": "application/msword",
            ".xlsx": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls": "application/vnd.ms-excel",
            ".pptx": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt": "application/vnd.ms-powerpoint",
            ".txt": "text/plain",
            ".html": "text/html",
            ".htm": "text/html",
            ".csv": "text/csv",
            ".json": "application/json",
            ".xml": "application/xml",
            ".md": "text/markdown",
            ".rtf": "application/rtf",
        }
        content_type = mime_map.get(ext, "application/octet-stream")

        try:
            # Reset file pointer if it's a file-like object
            if hasattr(file_obj, "seek"):
                file_obj.seek(0)

            # Read file content as bytes
            file_content = file_obj.read()

            # Send raw binary data with Content-Type header
            result = client.request(
                "POST",
                url,
                content=file_content,
                headers={"Content-Type": content_type},
            )
        finally:
            # Close file if we opened it (file path case)
            if isinstance(file_input, str):
                file_obj.close()

        return result


class AtomExtractableAPIResource:
    """
    Mixin class for atom extraction operations.

    This mixin provides methods to extract atoms from files in various formats.
    """

    RESOURCE_NAME: str = "atom"
    SUPPORTED_FORMATS = {
        "csv",
        "excel",
        "html",
        "json",
        "markdown",
        "ocr",
        "pdf",
        "png",
        "powerpoint",
        "rtf",
        "text",
        "word",
        "xml",
    }
    FORMATS_WITH_OCR = {"pdf", "powerpoint", "rtf"}

    # MIME type mapping based on format
    MIME_TYPES = {
        "csv": "text/csv",
        "excel": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "html": "text/html",
        "json": "application/json",
        "markdown": "text/markdown",
        "ocr": "application/octet-stream",
        "pdf": "application/pdf",
        "png": "image/png",
        "powerpoint": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "rtf": "application/rtf",
        "text": "text/plain",
        "word": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xml": "application/xml",
    }

    @classmethod
    def _prepare_file_input(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> tuple[BinaryIO, str]:
        """
        Prepare file input for upload. Accepts file path, bytes, or file-like object.

        Args:
            file_input: File path (str), bytes, or file-like object
            filename: Optional filename (required if file_input is bytes or BytesIO)

        Returns:
            Tuple of (file_object, filename)
        """
        if isinstance(file_input, str):
            # File path
            path = Path(file_input)
            if not path.exists():
                raise FileNotFoundError(f"File does not exist: {file_input}")
            if not path.is_file():
                raise ValidationError(f"Path is not a file: {file_input}")
            return open(path, "rb"), path.name
        elif isinstance(file_input, bytes):
            # Raw bytes
            if not filename:
                raise ValidationError("filename is required when file_input is bytes")
            return io.BytesIO(file_input), filename
        elif isinstance(file_input, (io.BytesIO, BinaryIO)):
            # File-like object
            if not filename:
                # Try to get filename from file object if it has a name attribute
                filename = getattr(file_input, "name", "file")
                if filename == "file":
                    raise ValidationError(
                        "filename is required when file_input is a file-like object without a name"
                    )
            return file_input, filename
        else:
            raise ValidationError(
                f"file_input must be str (file path), bytes, or file-like object, got {type(file_input)}"
            )

    @classmethod
    def extract_atoms(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        format_type: str,
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """
        Extract atoms from a file.

        Args:
            file_input: File path (str), bytes, or file-like object (BinaryIO/BytesIO)
            format_type: Format type (csv, excel, html, json, markdown, ocr, pdf, png, powerpoint, rtf, text, word, xml)
            ocr: Whether to use OCR (only for pdf, powerpoint, rtf). If None, uses default behavior.
            filename: Optional filename (required if file_input is bytes or file-like object without a name)

        Returns:
            Dictionary containing extracted atoms

        Raises:
            ValidationError: If file path or format is invalid
            SdkException: If API request fails
        """
        client = get_client()

        format_type = format_type.lower()
        if format_type not in cls.SUPPORTED_FORMATS:
            raise ValidationError(
                f"Unsupported format: {format_type}. "
                f"Supported formats: {', '.join(sorted(cls.SUPPORTED_FORMATS))}"
            )

        # Prepare file input (handles path, bytes, or file-like object)
        file_obj, file_name = cls._prepare_file_input(file_input, filename)

        # Build query parameters
        params = {}
        if format_type in cls.FORMATS_WITH_OCR and ocr is not None:
            if ocr:
                params["ocr"] = None  # Query param without value

        # Build URL with format type as path segment
        url = _get_url_base(cls, format_type, **params)

        # Get MIME type for the format
        content_type = cls.MIME_TYPES.get(format_type, "application/octet-stream")

        try:
            # Reset file pointer if it's a file-like object
            if hasattr(file_obj, "seek"):
                file_obj.seek(0)

            # Read file content as bytes
            file_content = file_obj.read()

            # Send raw binary data with Content-Type header
            result = client.request(
                "POST",
                url,
                content=file_content,
                headers={"Content-Type": content_type},
            )
        finally:
            # Close file if we opened it (file path case)
            if isinstance(file_input, str):
                file_obj.close()

        return result

    @classmethod
    def extract_atoms_csv(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a CSV file."""
        return cls.extract_atoms(file_input, "csv", filename=filename)

    @classmethod
    def extract_atoms_excel(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from an Excel file."""
        return cls.extract_atoms(file_input, "excel", filename=filename)

    @classmethod
    def extract_atoms_html(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from an HTML file."""
        return cls.extract_atoms(file_input, "html", filename=filename)

    @classmethod
    def extract_atoms_json(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a JSON file."""
        return cls.extract_atoms(file_input, "json", filename=filename)

    @classmethod
    def extract_atoms_markdown(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a Markdown file."""
        return cls.extract_atoms(file_input, "markdown", filename=filename)

    @classmethod
    def extract_atoms_ocr(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms using OCR."""
        return cls.extract_atoms(file_input, "ocr", filename=filename)

    @classmethod
    def extract_atoms_pdf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a PDF file."""
        return cls.extract_atoms(file_input, "pdf", ocr=ocr, filename=filename)

    @classmethod
    def extract_atoms_png(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a PNG file."""
        return cls.extract_atoms(file_input, "png", filename=filename)

    @classmethod
    def extract_atoms_powerpoint(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a PowerPoint file."""
        return cls.extract_atoms(file_input, "powerpoint", ocr=ocr, filename=filename)

    @classmethod
    def extract_atoms_rtf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from an RTF file."""
        return cls.extract_atoms(file_input, "rtf", ocr=ocr, filename=filename)

    @classmethod
    def extract_atoms_text(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a text file."""
        return cls.extract_atoms(file_input, "text", filename=filename)

    @classmethod
    def extract_atoms_word(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from a Word file."""
        return cls.extract_atoms(file_input, "word", filename=filename)

    @classmethod
    def extract_atoms_xml(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ) -> Dict[str, Any]:
        """Extract atoms from an XML file."""
        return cls.extract_atoms(file_input, "xml", filename=filename)
