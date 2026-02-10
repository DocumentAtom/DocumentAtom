import io
from unittest.mock import Mock

import pytest

from document_atom_sdk.configuration import configure, get_client
from document_atom_sdk.exceptions import FileNotFoundError, ValidationError
from document_atom_sdk.models.atom_extraction_result import AtomExtractionResultModel
from document_atom_sdk.resources.atom_extraction import AtomExtraction
from document_atom_sdk.resources.connectivity import Connectivity
from document_atom_sdk.resources.type_detection import TypeDetection


@pytest.fixture(autouse=True)
def setup_client():
    """Setup client for each test."""
    configure(endpoint="http://test-api.com")
    yield
    # Cleanup if needed


@pytest.fixture
def mock_client_request(monkeypatch):
    """Mock the client request method."""
    mock_response = {"FileType": "pdf", "Confidence": 0.95}

    def mock_request(method, url, **kwargs):
        return mock_response

    client = get_client()
    monkeypatch.setattr(client, "request", mock_request)
    return client


@pytest.fixture
def temp_file(tmp_path):
    """Create a temporary file for testing."""
    test_file = tmp_path / "test.txt"
    test_file.write_text("Test content")
    return str(test_file)


class TestConnectivity:
    """Test connectivity resource."""

    def test_validate_connectivity_success(self, monkeypatch):
        """Test successful connectivity validation."""
        client = get_client()

        def mock_request(method, url, **kwargs):
            mock_response = Mock()
            mock_response.status_code = 200
            return mock_response

        monkeypatch.setattr(client, "request", mock_request)

        result = Connectivity.validate_connectivity()
        assert result is True

    def test_validate_connectivity_failure(self, monkeypatch):
        """Test connectivity validation failure."""
        client = get_client()

        def mock_request(method, url, **kwargs):
            raise Exception("Connection failed")

        monkeypatch.setattr(client, "request", mock_request)

        with pytest.raises(Exception):
            Connectivity.validate_connectivity()


class TestTypeDetection:
    """Test type detection resource."""

    def test_detect_type_with_file_path(self, temp_file, mock_client_request):
        """Test type detection with file path."""
        result = TypeDetection.detect_type(temp_file)
        assert result is not None

    def test_detect_type_with_bytes(self, mock_client_request):
        """Test type detection with bytes."""
        file_content = b"test content"
        result = TypeDetection.detect_type(file_content, filename="test.txt")
        assert result is not None

    def test_detect_type_with_file_like_object(self, mock_client_request):
        """Test type detection with file-like object."""
        file_obj = io.BytesIO(b"test content")
        file_obj.name = "test.txt"
        result = TypeDetection.detect_type(file_obj)
        assert result is not None

    def test_detect_type_file_not_found(self):
        """Test type detection with non-existent file."""
        with pytest.raises(FileNotFoundError):
            TypeDetection.detect_type("/nonexistent/file.txt")

    def test_detect_type_invalid_input(self):
        """Test type detection with invalid input."""
        with pytest.raises(ValidationError):
            TypeDetection.detect_type(123)  # Invalid type


class TestAtomExtraction:
    """Test atom extraction resource."""

    def test_extract_atoms_csv(self, temp_file, mock_client_request):
        """Test CSV atom extraction."""
        result = AtomExtraction.extract_atoms_csv(temp_file)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_pdf(self, temp_file, mock_client_request):
        """Test PDF atom extraction."""
        result = AtomExtraction.extract_atoms_pdf(temp_file)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_pdf_with_ocr(self, temp_file, mock_client_request):
        """Test PDF atom extraction with OCR."""
        result = AtomExtraction.extract_atoms_pdf(temp_file, ocr=True)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_pdf_with_ocr_false(self, temp_file, mock_client_request):
        """Test PDF atom extraction with OCR=False."""
        result = AtomExtraction.extract_atoms_pdf(temp_file, ocr=False)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_unsupported_format(self, temp_file):
        """Test atom extraction with unsupported format."""
        with pytest.raises(ValidationError):
            AtomExtraction.extract_atoms(temp_file, "unsupported")

    def test_extract_atoms_file_not_found(self):
        """Test atom extraction with non-existent file."""
        with pytest.raises(FileNotFoundError):
            AtomExtraction.extract_atoms("/nonexistent/file.txt", "pdf")

    def test_extract_atoms_with_bytes(self, mock_client_request):
        """Test atom extraction with bytes."""
        file_content = b"test content"
        result = AtomExtraction.extract_atoms(file_content, "text", filename="test.txt")
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_with_file_like_object(self, mock_client_request):
        """Test atom extraction with file-like object."""
        file_obj = io.BytesIO(b"test content")
        file_obj.name = "test.txt"
        result = AtomExtraction.extract_atoms(file_obj, "text")
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    @pytest.mark.parametrize(
        "format_type",
        [
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
        ],
    )
    def test_extract_atoms_all_formats(
        self, temp_file, mock_client_request, format_type
    ):
        """Test atom extraction for all supported formats."""
        result = AtomExtraction.extract_atoms(temp_file, format_type)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_powerpoint_with_ocr(self, temp_file, mock_client_request):
        """Test PowerPoint atom extraction with OCR."""
        result = AtomExtraction.extract_atoms_powerpoint(temp_file, ocr=True)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_rtf_with_ocr(self, temp_file, mock_client_request):
        """Test RTF atom extraction with OCR."""
        result = AtomExtraction.extract_atoms_rtf(temp_file, ocr=True)
        assert result is not None
        assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_convenience_methods(self, temp_file, mock_client_request):
        """Test all convenience methods for atom extraction."""
        methods = [
            ("extract_atoms_csv", "csv"),
            ("extract_atoms_excel", "excel"),
            ("extract_atoms_html", "html"),
            ("extract_atoms_json", "json"),
            ("extract_atoms_markdown", "markdown"),
            ("extract_atoms_ocr", "ocr"),
            ("extract_atoms_png", "png"),
            ("extract_atoms_text", "text"),
            ("extract_atoms_word", "word"),
            ("extract_atoms_xml", "xml"),
        ]

        for method_name, format_type in methods:
            method = getattr(AtomExtraction, method_name)
            result = method(temp_file)
            assert result is not None
            assert isinstance(result, AtomExtraction.MODEL)

    def test_extract_atoms_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms when MODEL is None to cover return result path."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms(temp_file, "csv")
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_csv_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_csv when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_csv(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_excel_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_excel when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_excel(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_html_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_html when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_html(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_json_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_json when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_json(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_markdown_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_markdown when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_markdown(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_ocr_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_ocr when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_ocr(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_pdf_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_pdf when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_pdf(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_png_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_png when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_png(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_powerpoint_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_powerpoint when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_powerpoint(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_rtf_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_rtf when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_rtf(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_text_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_text when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_text(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_word_when_model_is_none(
        self, temp_file, mock_client_request
    ):
        """Test extract_atoms_word when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_word(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model

    def test_extract_atoms_xml_when_model_is_none(self, temp_file, mock_client_request):
        """Test extract_atoms_xml when MODEL is None."""
        original_model = AtomExtraction.MODEL
        try:
            AtomExtraction.MODEL = None
            result = AtomExtraction.extract_atoms_xml(temp_file)
            assert result is not None
            assert not isinstance(result, AtomExtractionResultModel)
        finally:
            AtomExtraction.MODEL = original_model
