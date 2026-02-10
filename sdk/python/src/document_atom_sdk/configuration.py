from .base import BaseClient

# Global client instance
_client = None


def configure(
    endpoint: str,
    timeout: int = 10,
    retries: int = 3,
):
    """Configure the SDK with endpoint."""
    global _client
    if endpoint is None:
        raise ValueError("Endpoint is required")
    _client = BaseClient(
        base_url=endpoint,
        timeout=timeout,
        retries=retries,
    )


# Utility function to get the shared client
def get_client():
    """Get the shared client instance."""
    if _client is None:
        raise ValueError("SDK is not configured. Call 'configure' first.")
    return _client
