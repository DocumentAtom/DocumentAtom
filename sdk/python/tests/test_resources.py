import io
from unittest.mock import Mock

import pytest

from document_atom_sdk.configuration import configure, get_client
from document_atom_sdk.exceptions import FileNotFoundError, ValidationError
from document_atom_sdk.models.atom import AtomModel
from document_atom_sdk.models.processor_settings import (
    ApiProcessorSettingsModel,
    ChunkingConfigurationModel,
)
from document_atom_sdk.resources.atom_extraction import AtomExtraction
from document_atom_sdk.resources.connectivity import Connectivity
from document_atom_sdk.resources.type_detection import TypeDetection


# ---------------------------------------------------------------------------
# Mock response data matching the server's actual JSON shapes
# ---------------------------------------------------------------------------

MOCK_TYPE_DETECTION_RESPONSE = {
    "MimeType": "application/pdf",
    "Extension": ".pdf",
    "Type": "Pdf",
}

MOCK_ATOM_LIST_RESPONSE = [
    {
        "GUID": "00000000-0000-0000-0000-000000000001",
        "Type": "Text",
        "Position": 0,
        "Length": 12,
        "MD5Hash": "abc123",
        "SHA1Hash": "def456",
        "SHA256Hash": "ghi789",
        "Text": "Test content",
    }
]


@pytest.fixture(autouse=True)
def setup_client():
    """Setup client for each test."""
    configure(endpoint="http://test-api.com")
    yield


@pytest.fixture
def mock_client_type_detect(monkeypatch):
    """Mock the client request method for type detection responses."""

    def mock_request(method, url, **kwargs):
        return MOCK_TYPE_DETECTION_RESPONSE

    client = get_client()
    monkeypatch.setattr(client, "request", mock_request)
    return client


@pytest.fixture
def mock_client_atom(monkeypatch):
    """Mock the client request method for atom extraction responses."""

    def mock_request(method, url, **kwargs):
        return MOCK_ATOM_LIST_RESPONSE

    client = get_client()
    monkeypatch.setattr(client, "request", mock_request)
    return client


@pytest.fixture
def temp_file(tmp_path):
    """Create a temporary file for testing."""
    test_file = tmp_path / "test.txt"
    test_file.write_text("Test content")
    return str(test_file)


@pytest.fixture
def ocr_settings():
    """Processor settings with OCR enabled."""
    return ApiProcessorSettingsModel(extract_atoms_from_images=True)


@pytest.fixture
def chunking_settings():
    """Processor settings with chunking enabled."""
    return ApiProcessorSettingsModel(
        chunking=ChunkingConfigurationModel(
            enable=True,
            strategy="FixedTokenCount",
            fixed_token_count=128,
            overlap_count=16,
            overlap_strategy="SlidingWindow",
        )
    )


@pytest.fixture
def full_settings():
    """Processor settings with OCR and chunking enabled."""
    return ApiProcessorSettingsModel(
        extract_atoms_from_images=True,
        chunking=ChunkingConfigurationModel(
            enable=True,
            strategy="SentenceBased",
            fixed_token_count=256,
            overlap_count=32,
            overlap_strategy="SlidingWindow",
        ),
    )


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

    def test_detect_type_with_file_path(self, temp_file, mock_client_type_detect):
        """Test type detection with file path."""
        result = TypeDetection.detect_type(temp_file)
        assert result is not None
        assert result.mime_type == "application/pdf"
        assert result.extension == ".pdf"
        assert result.type == "Pdf"

    def test_detect_type_with_bytes(self, mock_client_type_detect):
        """Test type detection with bytes."""
        file_content = b"test content"
        result = TypeDetection.detect_type(file_content, filename="test.txt")
        assert result is not None

    def test_detect_type_with_file_like_object(self, mock_client_type_detect):
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

    def test_extract_atoms_csv(self, temp_file, mock_client_atom):
        """Test CSV atom extraction."""
        result = AtomExtraction.extract_atoms_csv(temp_file)
        assert isinstance(result, list)
        assert len(result) == 1
        assert isinstance(result[0], AtomModel)

    def test_extract_atoms_pdf(self, temp_file, mock_client_atom):
        """Test PDF atom extraction."""
        result = AtomExtraction.extract_atoms_pdf(temp_file)
        assert isinstance(result, list)
        assert len(result) == 1
        assert isinstance(result[0], AtomModel)

    def test_extract_atoms_pdf_with_ocr(self, temp_file, mock_client_atom, ocr_settings):
        """Test PDF atom extraction with OCR settings."""
        result = AtomExtraction.extract_atoms_pdf(temp_file, settings=ocr_settings)
        assert isinstance(result, list)
        assert len(result) >= 1

    def test_extract_atoms_pdf_with_chunking(self, temp_file, mock_client_atom, chunking_settings):
        """Test PDF atom extraction with chunking settings."""
        result = AtomExtraction.extract_atoms_pdf(temp_file, settings=chunking_settings)
        assert isinstance(result, list)
        assert len(result) >= 1

    def test_extract_atoms_pdf_with_full_settings(self, temp_file, mock_client_atom, full_settings):
        """Test PDF atom extraction with OCR and chunking settings."""
        result = AtomExtraction.extract_atoms_pdf(temp_file, settings=full_settings)
        assert isinstance(result, list)
        assert len(result) >= 1

    def test_extract_atoms_unsupported_format(self, temp_file):
        """Test atom extraction with unsupported format."""
        with pytest.raises(ValidationError):
            AtomExtraction.extract_atoms(temp_file, "unsupported")

    def test_extract_atoms_file_not_found(self):
        """Test atom extraction with non-existent file."""
        with pytest.raises(FileNotFoundError):
            AtomExtraction.extract_atoms("/nonexistent/file.txt", "pdf")

    def test_extract_atoms_with_bytes(self, mock_client_atom):
        """Test atom extraction with bytes."""
        file_content = b"test content"
        result = AtomExtraction.extract_atoms(file_content, "text", filename="test.txt")
        assert isinstance(result, list)
        assert len(result) == 1

    def test_extract_atoms_with_file_like_object(self, mock_client_atom):
        """Test atom extraction with file-like object."""
        file_obj = io.BytesIO(b"test content")
        file_obj.name = "test.txt"
        result = AtomExtraction.extract_atoms(file_obj, "text")
        assert isinstance(result, list)
        assert len(result) == 1

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
        self, temp_file, mock_client_atom, format_type
    ):
        """Test atom extraction for all supported formats."""
        result = AtomExtraction.extract_atoms(temp_file, format_type)
        assert isinstance(result, list)
        assert len(result) >= 1

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
    def test_extract_atoms_all_formats_with_chunking(
        self, temp_file, mock_client_atom, chunking_settings, format_type
    ):
        """Test atom extraction with chunking for all supported formats."""
        result = AtomExtraction.extract_atoms(temp_file, format_type, settings=chunking_settings)
        assert isinstance(result, list)
        assert len(result) >= 1

    def test_extract_atoms_powerpoint_with_ocr(self, temp_file, mock_client_atom, ocr_settings):
        """Test PowerPoint atom extraction with OCR settings."""
        result = AtomExtraction.extract_atoms_powerpoint(temp_file, settings=ocr_settings)
        assert isinstance(result, list)

    def test_extract_atoms_rtf_with_ocr(self, temp_file, mock_client_atom, ocr_settings):
        """Test RTF atom extraction with OCR settings."""
        result = AtomExtraction.extract_atoms_rtf(temp_file, settings=ocr_settings)
        assert isinstance(result, list)

    def test_extract_atoms_word_with_chunking(self, temp_file, mock_client_atom, chunking_settings):
        """Test Word atom extraction with chunking settings."""
        result = AtomExtraction.extract_atoms_word(temp_file, settings=chunking_settings)
        assert isinstance(result, list)

    def test_extract_atoms_excel_with_chunking(self, temp_file, mock_client_atom, chunking_settings):
        """Test Excel atom extraction with chunking settings."""
        result = AtomExtraction.extract_atoms_excel(temp_file, settings=chunking_settings)
        assert isinstance(result, list)

    def test_extract_atoms_text_with_chunking(self, temp_file, mock_client_atom, chunking_settings):
        """Test text atom extraction with chunking settings."""
        result = AtomExtraction.extract_atoms_text(temp_file, settings=chunking_settings)
        assert isinstance(result, list)

    def test_extract_atoms_convenience_methods(self, temp_file, mock_client_atom):
        """Test all convenience methods for atom extraction."""
        methods = [
            "extract_atoms_csv",
            "extract_atoms_excel",
            "extract_atoms_html",
            "extract_atoms_json",
            "extract_atoms_markdown",
            "extract_atoms_ocr",
            "extract_atoms_png",
            "extract_atoms_text",
            "extract_atoms_word",
            "extract_atoms_xml",
        ]

        for method_name in methods:
            method = getattr(AtomExtraction, method_name)
            result = method(temp_file)
            assert isinstance(result, list), f"{method_name} should return a list"

    def test_extract_atoms_convenience_methods_with_chunking(self, temp_file, mock_client_atom, chunking_settings):
        """Test all convenience methods with chunking settings."""
        methods = [
            "extract_atoms_csv",
            "extract_atoms_excel",
            "extract_atoms_html",
            "extract_atoms_json",
            "extract_atoms_markdown",
            "extract_atoms_ocr",
            "extract_atoms_pdf",
            "extract_atoms_png",
            "extract_atoms_powerpoint",
            "extract_atoms_rtf",
            "extract_atoms_text",
            "extract_atoms_word",
            "extract_atoms_xml",
        ]

        for method_name in methods:
            method = getattr(AtomExtraction, method_name)
            result = method(temp_file, settings=chunking_settings)
            assert isinstance(result, list), f"{method_name} should return a list"

    def test_extract_atoms_generic_with_settings(self, temp_file, mock_client_atom, full_settings):
        """Test generic extract_atoms with full settings."""
        result = AtomExtraction.extract_atoms(temp_file, "pdf", settings=full_settings)
        assert isinstance(result, list)
        assert len(result) >= 1

    def test_extract_atoms_returns_empty_list_for_none(self, temp_file, monkeypatch):
        """Test extract_atoms returns empty list when server returns None."""

        def mock_request(method, url, **kwargs):
            return None

        client = get_client()
        monkeypatch.setattr(client, "request", mock_request)

        result = AtomExtraction.extract_atoms(temp_file, "csv")
        assert isinstance(result, list)
        assert len(result) == 0

    def test_atom_model_fields(self, temp_file, mock_client_atom):
        """Test that AtomModel fields are correctly populated."""
        result = AtomExtraction.extract_atoms(temp_file, "text")
        assert len(result) == 1
        atom = result[0]
        assert atom.guid == "00000000-0000-0000-0000-000000000001"
        assert atom.type == "Text"
        assert atom.position == 0
        assert atom.length == 12
        assert atom.text == "Test content"

    def test_chunking_configuration_model(self):
        """Test ChunkingConfigurationModel serialization."""
        config = ChunkingConfigurationModel(
            enable=True,
            strategy="FixedTokenCount",
            fixed_token_count=128,
            overlap_count=16,
            overlap_strategy="SlidingWindow",
        )
        data = config.model_dump(by_alias=True, exclude_none=True)
        assert data["Enable"] is True
        assert data["Strategy"] == "FixedTokenCount"
        assert data["FixedTokenCount"] == 128
        assert data["OverlapCount"] == 16
        assert data["OverlapStrategy"] == "SlidingWindow"

    def test_api_processor_settings_model_with_chunking(self):
        """Test ApiProcessorSettingsModel serialization with chunking."""
        settings = ApiProcessorSettingsModel(
            extract_atoms_from_images=True,
            chunking=ChunkingConfigurationModel(
                enable=True,
                strategy="SentenceBased",
                fixed_token_count=256,
            ),
        )
        data = settings.model_dump(by_alias=True, exclude_none=True)
        assert data["ExtractAtomsFromImages"] is True
        assert data["Chunking"]["Enable"] is True
        assert data["Chunking"]["Strategy"] == "SentenceBased"
        assert data["Chunking"]["FixedTokenCount"] == 256
