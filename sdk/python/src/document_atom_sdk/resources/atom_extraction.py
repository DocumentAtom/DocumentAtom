import io
from typing import BinaryIO, Optional, Union

from ..mixins import AtomExtractableAPIResource
from ..models.atom_extraction_result import AtomExtractionResultModel


class AtomExtraction(AtomExtractableAPIResource):
    """
    Atom extraction resource class.
    """

    RESOURCE_NAME: str = "atom"
    MODEL = AtomExtractionResultModel

    @classmethod
    def extract_atoms(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        format_type: str,
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ):
        """
        Extract atoms from a file.

        Args:
            file_input: File path (str), bytes, or file-like object (BinaryIO/BytesIO)
            format_type: Format type (csv, excel, html, json, markdown, ocr, pdf, png, powerpoint, rtf, text, word, xml)
            ocr: Whether to use OCR (only for pdf, powerpoint, rtf). If None, uses default behavior.
            filename: Optional filename (required if file_input is bytes or file-like object without a name)

        Returns:
            AtomExtractionResultModel containing extracted atoms
        """
        result = super().extract_atoms(file_input, format_type, ocr, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_csv(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a CSV file."""
        result = super().extract_atoms_csv(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_excel(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from an Excel file."""
        result = super().extract_atoms_excel(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_html(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from an HTML file."""
        result = super().extract_atoms_html(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_json(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a JSON file."""
        result = super().extract_atoms_json(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_markdown(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a Markdown file."""
        result = super().extract_atoms_markdown(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_ocr(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms using OCR."""
        result = super().extract_atoms_ocr(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_pdf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ):
        """Extract atoms from a PDF file."""
        result = super().extract_atoms_pdf(file_input, ocr, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_png(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a PNG file."""
        result = super().extract_atoms_png(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_powerpoint(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ):
        """Extract atoms from a PowerPoint file."""
        result = super().extract_atoms_powerpoint(file_input, ocr, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_rtf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        ocr: Optional[bool] = None,
        filename: Optional[str] = None,
    ):
        """Extract atoms from an RTF file."""
        result = super().extract_atoms_rtf(file_input, ocr, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_text(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a text file."""
        result = super().extract_atoms_text(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_word(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from a Word file."""
        result = super().extract_atoms_word(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result

    @classmethod
    def extract_atoms_xml(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        filename: Optional[str] = None,
    ):
        """Extract atoms from an XML file."""
        result = super().extract_atoms_xml(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result
