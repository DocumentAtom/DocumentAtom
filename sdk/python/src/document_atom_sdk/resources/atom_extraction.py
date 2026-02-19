import io
from typing import Any, BinaryIO, List, Optional, Union

from ..mixins import AtomExtractableAPIResource
from ..models.atom import AtomModel
from ..models.atom_extraction_result import AtomListAdapter


class AtomExtraction(AtomExtractableAPIResource):
    """
    Atom extraction resource class.
    """

    RESOURCE_NAME: str = "atom"

    @classmethod
    def _parse_atoms(cls, result) -> List[AtomModel]:
        """Parse the server response into a list of AtomModel.

        Most endpoints return a JSON array of Atom objects.  The OCR endpoint
        returns an ExtractionResult dict with TextElements/Tables/Lists which
        must be converted client-side (mirroring the C# SDK behaviour).
        """
        if result is None:
            return []
        if isinstance(result, list):
            return AtomListAdapter.validate_python(result)

        # OCR-style ExtractionResult: {TextElements, Tables, Lists}
        if isinstance(result, dict) and "TextElements" in result:
            return cls._convert_extraction_result(result)

        return AtomListAdapter.validate_python(result)

    @classmethod
    def _convert_extraction_result(cls, data: dict) -> List[AtomModel]:
        """Convert an OCR ExtractionResult dict into a list of AtomModel."""
        atoms: List[AtomModel] = []

        for te in data.get("TextElements") or []:
            text = te.get("Text")
            if text:
                atoms.append(AtomModel(
                    Type="Text",
                    Text=text,
                    Length=len(text),
                ))

        for table in data.get("Tables") or []:
            atoms.append(AtomModel(
                Type="Table",
                Rows=table.get("Rows"),
                Columns=table.get("Columns"),
            ))

        for lst in data.get("Lists") or []:
            items = lst.get("Items") or []
            if items:
                is_ordered = lst.get("IsOrdered", False)
                atoms.append(AtomModel(
                    Type="List",
                    Length=sum(len(s) for s in items if s),
                    **{"UnorderedList" if not is_ordered else "OrderedList": items},
                ))

        return atoms

    @classmethod
    def extract_atoms(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        format_type: str,
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """
        Extract atoms from a file.

        Args:
            file_input: File path (str), bytes, or file-like object (BinaryIO/BytesIO)
            format_type: Format type (csv, excel, html, json, markdown, ocr, pdf, png, powerpoint, rtf, text, word, xml)
            settings: Optional processor settings (dict or ApiProcessorSettingsModel)
            filename: Optional filename (required if file_input is bytes or file-like object without a name)

        Returns:
            List of AtomModel containing extracted atoms
        """
        result = super().extract_atoms(file_input, format_type, settings, filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_csv(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a CSV file."""
        result = super().extract_atoms_csv(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_excel(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from an Excel file."""
        result = super().extract_atoms_excel(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_html(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from an HTML file."""
        result = super().extract_atoms_html(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_json(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a JSON file."""
        result = super().extract_atoms_json(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_markdown(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a Markdown file."""
        result = super().extract_atoms_markdown(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_ocr(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms using OCR."""
        result = super().extract_atoms_ocr(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_pdf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a PDF file."""
        result = super().extract_atoms_pdf(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_png(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a PNG file."""
        result = super().extract_atoms_png(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_powerpoint(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a PowerPoint file."""
        result = super().extract_atoms_powerpoint(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_rtf(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from an RTF file."""
        result = super().extract_atoms_rtf(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_text(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a text file."""
        result = super().extract_atoms_text(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_word(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from a Word file."""
        result = super().extract_atoms_word(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)

    @classmethod
    def extract_atoms_xml(
        cls,
        file_input: Union[str, bytes, BinaryIO, io.BytesIO],
        settings: Optional[Any] = None,
        filename: Optional[str] = None,
    ) -> List[AtomModel]:
        """Extract atoms from an XML file."""
        result = super().extract_atoms_xml(file_input, settings=settings, filename=filename)
        return cls._parse_atoms(result)
