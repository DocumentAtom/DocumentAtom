from urllib.parse import urlencode


def _get_url_base(cls, *args, **query_params) -> str:
    """
    Common URL construction logic for DocumentAtom SDK resources.

    Args:
        *args: Variable-length argument list for path segments.
        **query_params: Optional query parameters to include in the URL.

    Returns:
        str: The constructed URL without version prefix.
    """
    parts = []
    remaining_args = [arg for arg in args if arg is not None]

    # Add resource name
    parts.append(cls.RESOURCE_NAME)

    # Add remaining path components
    parts.extend(str(arg) for arg in remaining_args)

    # Build URL path
    path = "/".join(str(part) for part in parts if part)

    # Handle query parameters
    formatted_params = {k: v for k, v in query_params.items() if v is not None}
    flags = [k for k, v in query_params.items() if v is None]
    query_string = urlencode(formatted_params)

    # Append flags directly if they exist
    if flags:
        query_string += ("&" if query_string else "") + "&".join(flags)

    return f"{path}?{query_string}" if query_string else path
