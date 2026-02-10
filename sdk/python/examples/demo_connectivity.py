import document_atom_sdk

document_atom_sdk.configure(
    endpoint="http://YOUR_SERVER_URL_HERE:PORT",
)


def validate_connectivity():
    """Validate connectivity to the DocumentAtom API."""
    try:
        result = document_atom_sdk.Connectivity.validate_connectivity()
        print(f"Connectivity check successful: {result}")
        return True
    except Exception as e:
        print(f"Connectivity check failed: {e}")
        return False


# validate_connectivity()
