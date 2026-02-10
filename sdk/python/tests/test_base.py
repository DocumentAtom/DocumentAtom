from unittest.mock import Mock, patch

import httpx
import pytest

from document_atom_sdk.base import BaseClient
from document_atom_sdk.enums.api_error_enum import ApiError_Enum
from document_atom_sdk.exceptions import SdkException, ServerError


@pytest.fixture
def base_url():
    return "http://test-api.com"


@pytest.fixture
def base_client(base_url):
    """Create a base client for testing."""
    with patch("httpx.Client"):
        return BaseClient(
            base_url=base_url,
        )


@pytest.fixture
def success_response():
    """Create a success response dictionary."""
    return {"data": "test"}


@pytest.fixture
def mock_httpx_client(monkeypatch):
    """Create a mock httpx client that properly handles requests."""

    def mock_request(*args, **kwargs):
        mock_response = Mock(spec=httpx.Response)
        mock_response.status_code = 200
        mock_response.content = b'{"data": "test"}'
        mock_response.json.return_value = {"data": "test"}
        mock_response.raise_for_status.return_value = None
        return mock_response

    mock_client = Mock(spec=httpx.Client)
    mock_client.request = mock_request
    mock_client.close.return_value = None

    def mock_client_constructor(*args, **kwargs):
        return mock_client

    monkeypatch.setattr(httpx, "Client", mock_client_constructor)
    return mock_client


@pytest.fixture
def mock_error_response():
    """Create a mock error response."""

    def create_error(error_type: str, description: str, status_code: int):
        mock_response = Mock(spec=httpx.Response)
        mock_response.status_code = status_code
        mock_response.json.return_value = {
            "Error": error_type,
            "Description": description,
        }
        return mock_response

    return create_error


def test_client_initialization(base_url):
    """Test client initialization with different parameters."""
    with patch("httpx.Client"):
        # Test default values
        client = BaseClient(base_url=base_url)
        assert client.base_url == base_url
        assert client.timeout == 10
        assert client.retries == 3
        # Test custom values
        custom_client = BaseClient(base_url=base_url, timeout=20, retries=5)
        assert custom_client.base_url == base_url
        assert custom_client.timeout == 20
        assert custom_client.retries == 5


def test_successful_request(base_client, monkeypatch):
    """Test successful request with JSON response."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "test"}'
    mock_response.json.return_value = {"data": "test"}
    mock_response.raise_for_status.return_value = None

    with patch.object(base_client.client, "request", return_value=mock_response):
        response = base_client.request("GET", "/test")
        assert response == {"data": "test"}


def test_empty_response(base_client, monkeypatch):
    """Test successful request with empty response."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b""
    mock_response.raise_for_status.return_value = None

    with patch.object(base_client.client, "request", return_value=mock_response):
        response = base_client.request("GET", "/test")
        assert response is None


def test_request_with_headers_and_params(base_client, monkeypatch):
    """Test request with both headers and query parameters."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "test"}'
    mock_response.json.return_value = {"data": "test"}
    mock_response.raise_for_status.return_value = None

    mock_request = Mock(return_value=mock_response)
    with patch.object(base_client.client, "request", mock_request):
        headers = {"Content-Type": "application/json", "Authorization": "Bearer token"}
        params = {"filter": "value"}
        base_client.request("GET", "/test", headers=headers, params=params)

        # The base client merges headers and transforms the URL
        call_args = mock_request.call_args
        assert call_args[0][0] == "GET"
        assert call_args[0][1] == f"{base_client.base_url}/test"
        assert call_args[1]["params"] == params
        # Headers should be merged with default headers
        assert "Authorization" in call_args[1]["headers"]
        assert call_args[1]["headers"]["Authorization"] == "Bearer token"


def test_conflict_error(base_client, monkeypatch):
    """Test handling of conflict error."""
    error_response = {
        "Error": ApiError_Enum.conflict,
        "Description": "Operation failed as it would create a conflict with an existing resource.",
    }
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 409
    mock_response.json.return_value = error_response
    mock_response.raise_for_status.side_effect = httpx.HTTPStatusError(
        "409 Conflict", request=Mock(spec=httpx.Request), response=mock_response
    )
    mock_response.headers = {"Content-Type": "application/json"}

    with patch.object(base_client.client, "request") as mock_request:
        mock_request.side_effect = httpx.HTTPStatusError(
            "409 Conflict", request=Mock(spec=httpx.Request), response=mock_response
        )
        with pytest.raises(SdkException) as exc_info:
            base_client.request("POST", "/test")
        assert error_response["Description"] in str(exc_info.value)


def test_server_error(base_client, monkeypatch):
    """Test handling of server error."""
    error_response = {
        "Error": "InternalError",
        "Description": "Internal server error occurred",
    }
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 500
    mock_response.headers = {"Content-Type": "application/json"}
    mock_response.json.return_value = error_response
    mock_response.raise_for_status.side_effect = httpx.HTTPStatusError(
        "500 Internal Server Error",
        request=Mock(spec=httpx.Request),
        response=mock_response,
    )

    with patch.object(
        base_client.client,
        "request",
        side_effect=httpx.HTTPStatusError(
            "500 Internal Server Error",
            request=Mock(spec=httpx.Request),
            response=mock_response,
        ),
    ):
        with pytest.raises(ServerError):
            base_client.request("GET", "/test")


def test_request_with_malformed_error_response(base_client, monkeypatch):
    """Test handling of malformed error response."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 400
    mock_response.headers = {"Content-Type": "application/json"}
    mock_response.json.return_value = {"InvalidFormat": "Missing Error field"}
    mock_response.raise_for_status.side_effect = httpx.HTTPStatusError(
        "400 Bad Request", request=Mock(spec=httpx.Request), response=mock_response
    )

    with patch.object(base_client.client, "request") as mock_request:
        mock_request.side_effect = httpx.HTTPStatusError(
            "400 Bad Request", request=Mock(spec=httpx.Request), response=mock_response
        )
        with pytest.raises(SdkException) as exc_info:
            base_client.request("GET", "/test")
        assert "Unexpected error" in str(exc_info.value)


def test_request_with_retry_success(base_client, monkeypatch):
    """Test request that succeeds after retries."""
    success_response = Mock(spec=httpx.Response)
    success_response.status_code = 200
    success_response.content = b'{"data": "success"}'
    success_response.json.return_value = {"data": "success"}
    success_response.raise_for_status.return_value = None

    mock_request = Mock(
        side_effect=[httpx.RequestError("First attempt failed"), success_response]
    )

    with patch.object(base_client.client, "request", mock_request):
        response = base_client.request("GET", "/test")
        assert response == {"data": "success"}
        assert mock_request.call_count == 2


def test_client_close(base_client):
    """Test client close method."""
    mock_close = Mock()
    base_client.client.close = mock_close
    base_client.close()
    mock_close.assert_called_once()


def test_request_with_network_error(base_client, monkeypatch):
    """Test handling of network error."""
    with patch.object(
        base_client.client, "request", side_effect=httpx.NetworkError("Network error")
    ):
        with pytest.raises(SdkException) as exc_info:
            base_client.request("GET", "/test")
        assert "Request failed after 3 attempts" in str(exc_info.value)


def test_request_with_timeout_error(base_client, monkeypatch):
    """Test handling of timeout error."""
    with patch.object(
        base_client.client,
        "request",
        side_effect=httpx.TimeoutException("Request timed out"),
    ):
        with pytest.raises(SdkException) as exc_info:
            base_client.request("GET", "/test")
        assert "Request failed after 3 attempts" in str(exc_info.value)


def test_request_with_different_http_methods(base_client, monkeypatch):
    """Test requests with different HTTP methods."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "test"}'
    mock_response.json.return_value = {"data": "test"}
    mock_response.raise_for_status.return_value = None
    mock_response.headers = {"Content-Type": "application/json"}

    mock_request = Mock(return_value=mock_response)
    with patch.object(base_client.client, "request", mock_request):
        methods = ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD"]
        for method in methods:
            base_client.request(method, "/test")
            # Verify the call was made with correct method and transformed URL
            call_args = mock_request.call_args
            assert call_args[0][0] == method
            assert call_args[0][1] == f"{base_client.base_url}/test"
            assert "Content-Type" in call_args[1]["headers"]
            assert call_args[1]["headers"]["Content-Type"] == "application/json"


def test_request_with_complex_json(base_client, monkeypatch):
    """Test request with complex JSON payload."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "test"}'
    mock_response.json.return_value = {"data": "test"}
    mock_response.raise_for_status.return_value = None
    mock_response.headers = {"Content-Type": "application/json"}
    mock_request = Mock(return_value=mock_response)
    with patch.object(base_client.client, "request", mock_request):
        complex_payload = {
            "nested": {"array": [1, 2, 3], "object": {"key": "value"}},
            "list": ["item1", "item2"],
            "null": None,
        }

        base_client.request("POST", "/test", json=complex_payload)
        # Verify the call was made with correct parameters
        call_args = mock_request.call_args
        assert call_args[0][0] == "POST"
        assert call_args[0][1] == f"{base_client.base_url}/test"
        assert call_args[1]["json"] == complex_payload
        assert "Content-Type" in call_args[1]["headers"]
        assert call_args[1]["headers"]["Content-Type"] == "application/json"


def test_request_with_file_upload(base_client, monkeypatch):
    """Test request with file upload."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "uploaded"}'
    mock_response.json.return_value = {"data": "uploaded"}
    mock_response.raise_for_status.return_value = None

    mock_request = Mock(return_value=mock_response)
    with patch.object(base_client.client, "request", mock_request):
        files = {"file": ("test.txt", b"content", "text/plain")}
        base_client.request("POST", "/upload", files=files)

        # Verify Content-Type header was removed for file uploads
        call_kwargs = mock_request.call_args[1]
        assert "Content-Type" not in call_kwargs.get("headers", {})


def test_request_with_content_parameter(base_client, monkeypatch):
    """Test request with content parameter (for binary data)."""
    mock_response = Mock(spec=httpx.Response)
    mock_response.status_code = 200
    mock_response.content = b'{"data": "received"}'
    mock_response.json.return_value = {"data": "received"}
    mock_response.raise_for_status.return_value = None

    mock_request = Mock(return_value=mock_response)
    with patch.object(base_client.client, "request", mock_request):
        content = b"binary content"
        headers = {"Content-Type": "application/pdf"}
        base_client.request("POST", "/upload", content=content, headers=headers)

        call_kwargs = mock_request.call_args[1]
        assert call_kwargs["content"] == content
        assert call_kwargs["headers"]["Content-Type"] == "application/pdf"


def test_request_max_retries_exhausted(base_client, monkeypatch):
    """Test request fails after max retries with RequestError."""
    with patch.object(
        base_client.client,
        "request",
        side_effect=httpx.RequestError("Connection failed"),
    ):
        with pytest.raises(SdkException) as exc_info:
            base_client.request("GET", "/test")
        assert f"Request failed after {base_client.retries} attempts" in str(
            exc_info.value
        )
