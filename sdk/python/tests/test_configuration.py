import sys

import pytest

from document_atom_sdk.base import BaseClient
from document_atom_sdk.configuration import configure, get_client


@pytest.fixture(autouse=True)
def reset_client():
    """Fixture to reset the global client before and after each test."""
    # Store the original client
    original_client = sys.modules["document_atom_sdk.configuration"]._client

    # Reset client to None
    sys.modules["document_atom_sdk.configuration"]._client = None

    yield

    # Restore the original state after the test
    sys.modules["document_atom_sdk.configuration"]._client = original_client


def test_configure_with_endpoint_only():
    """Test configuration with only endpoint provided."""
    endpoint = "http://test-api.com"
    configure(endpoint=endpoint)

    client = get_client()
    assert isinstance(client, BaseClient)
    assert client.base_url == endpoint


def test_reconfigure_client():
    """Test that reconfiguring client creates new instance."""
    configure(endpoint="http://test-api-1.com")
    first_client = get_client()

    configure(endpoint="http://test-api-2.com")
    second_client = get_client()

    assert first_client is not second_client
    assert first_client.base_url != second_client.base_url


def test_client_singleton():
    """Test that get_client returns the same instance."""
    configure(endpoint="http://test-api.com")

    client1 = get_client()
    client2 = get_client()

    assert client1 is client2


@pytest.mark.parametrize(
    "endpoint",
    [
        "http://test-api.com",
        "http://test-api.com/",
        "https://test-api.com",
        "http://localhost:8080",
    ],
)
def test_valid_configuration_combinations(endpoint):
    """Test various valid combinations of configuration parameters."""
    configure(endpoint=endpoint)
    client = get_client()

    assert client.base_url == endpoint


def test_configure_maintains_default_client_settings():
    """Test that configured client maintains default BaseClient settings."""
    configure(endpoint="http://test-api.com")
    client = get_client()

    assert client.timeout == 10
    assert client.retries == 3


def test_multiple_rapid_configurations():
    """Test multiple rapid configurations."""
    endpoints = [
        "http://test-api-1.com",
        "http://test-api-2.com",
        "http://test-api-3.com",
    ]

    for endpoint in endpoints:
        configure(endpoint=endpoint)
        client = get_client()
        assert client.base_url == endpoint


def test_client_attribute_access():
    """Test client attribute access."""
    configure(endpoint="http://test-api.com")
    client = get_client()

    assert client.base_url == "http://test-api.com"
    assert client.timeout == 10
    assert client.retries == 3


def test_get_client_without_configuration():
    """Test that get_client raises ValueError when client is not configured."""
    # Import the module directly to access _client
    import document_atom_sdk.configuration as config

    # Explicitly set _client to None
    config._client = None

    # Assert that get_client raises ValueError
    with pytest.raises(ValueError) as exc_info:
        get_client()
    assert str(exc_info.value) == "SDK is not configured. Call 'configure' first."


def test_configure_with_special_characters():
    """Test configuration with special characters in endpoint."""
    endpoint = "http://test-api.com/path-with-special/chars"

    configure(endpoint=endpoint)
    client = get_client()

    assert client.base_url == endpoint


def test_configure_with_custom_timeout_and_retries():
    """Test configuration with custom timeout and retries."""
    endpoint = "http://test-api.com"

    configure(endpoint=endpoint, timeout=30, retries=5)
    client = get_client()

    assert client.base_url == endpoint
    assert client.timeout == 30
    assert client.retries == 5


def test_configure_with_none_endpoint():
    """Test configuration fails when endpoint is None."""
    with pytest.raises(ValueError) as exc_info:
        configure(endpoint=None)
    assert str(exc_info.value) == "Endpoint is required"
