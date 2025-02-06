namespace DocumentAtom.Server
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// API error codes.
    /// </summary>
    public enum ApiErrorEnum
    {
        /// <summary>
        /// Request body missing.
        /// </summary>
        [EnumMember(Value = "RequestBodyMissing")]
        RequestBodyMissing,
        /// <summary>
        /// Required properties were missing.
        /// </summary>
        [EnumMember(Value = "RequiredPropertiesMissing")]
        RequiredPropertiesMissing,
        /// <summary>
        /// Unable to discern type of supplied data.
        /// </summary>
        [EnumMember(Value = "UnknownTypeDetected")]
        UnknownTypeDetected,
        /// <summary>
        /// Authentication failed.
        /// </summary>
        [EnumMember(Value = "AuthenticationFailed")]
        AuthenticationFailed,
        /// <summary>
        /// Authorization failed.
        /// </summary>
        [EnumMember(Value = "AuthorizationFailed")]
        AuthorizationFailed,
        /// <summary>
        /// Bad gateway.
        /// </summary>
        [EnumMember(Value = "BadGateway")]
        BadGateway,
        /// <summary>
        /// Bad request.
        /// </summary>
        [EnumMember(Value = "BadRequest")]
        BadRequest,
        /// <summary>
        /// Conflict.
        /// </summary>
        [EnumMember(Value = "Conflict")]
        Conflict,
        /// <summary>
        /// DeserializationError.
        /// </summary>
        [EnumMember(Value = "DeserializationError")]
        DeserializationError,
        /// <summary>
        /// Internal error.
        /// </summary>
        [EnumMember(Value = "InternalError")]
        InternalError,
        /// <summary>
        /// Not found.
        /// </summary>
        [EnumMember(Value = "NotFound")]
        NotFound,
        /// <summary>
        /// Timeout.
        /// </summary>
        [EnumMember(Value = "Timeout")]
        Timeout
    }
}
