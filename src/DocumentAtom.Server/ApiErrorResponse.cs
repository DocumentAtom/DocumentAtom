namespace DocumentAtom.Server
{
    using System;

    /// <summary>
    /// API error response.
    /// </summary>
    public class ApiErrorResponse
    {
        #region Public-Members

        /// <summary>
        /// Error.
        /// </summary>
        public ApiErrorEnum Error { get; set; } = ApiErrorEnum.AuthenticationFailed;

        /// <summary>
        /// Human-readable message.
        /// </summary>
        public string Message
        {
            get
            {
                switch (Error)
                {
                    case ApiErrorEnum.AuthenticationFailed:
                        return "Your authentication material was not accepted.";
                    case ApiErrorEnum.AuthorizationFailed:
                        return "Your authentication material was accepted, but you are not authorized to perform this request.";
                    case ApiErrorEnum.BadGateway:
                        return "Your request is unable to be serviced as there are no origin servers available.";
                    case ApiErrorEnum.BadRequest:
                        return "We were unable to discern your request.  Please check your URL, query, and request body.";
                    case ApiErrorEnum.Conflict:
                        return "Operation failed as it would create a conflict with an existing resource.";
                    case ApiErrorEnum.DeserializationError:
                        return "Your request body was invalid and could not be deserialized.";
                    case ApiErrorEnum.InternalError:
                        return "An internal error has been encountered.";
                    case ApiErrorEnum.NotFound:
                        return "The requested resource was not found.";
                    case ApiErrorEnum.Timeout:
                        return "The request was not completed within the specified timeout interval.";
                    case ApiErrorEnum.UnknownTypeDetected:
                        return "An unrecognizable data type was supplied.";
                    case ApiErrorEnum.RequestBodyMissing:
                        return "A request body is required for this operation.";
                    case ApiErrorEnum.RequiredPropertiesMissing:
                        return "A required property was missing from the request.";

                    default:
                        return "An unknown error code '" + Error.ToString() + "' was encountered.";
                }
            }
        }

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode
        {
            get
            {
                switch (Error)
                {
                    case ApiErrorEnum.AuthenticationFailed:
                        return 401;
                    case ApiErrorEnum.AuthorizationFailed:
                        return 401;
                    case ApiErrorEnum.BadGateway:
                        return 502;
                    case ApiErrorEnum.BadRequest:
                        return 400;
                    case ApiErrorEnum.Conflict:
                        return 409;
                    case ApiErrorEnum.DeserializationError:
                        return 400;
                    case ApiErrorEnum.InternalError:
                        return 500;
                    case ApiErrorEnum.NotFound:
                        return 404;
                    case ApiErrorEnum.Timeout:
                        return 408;
                    case ApiErrorEnum.UnknownTypeDetected:
                        return 400;
                    case ApiErrorEnum.RequestBodyMissing:
                        return 400;
                    case ApiErrorEnum.RequiredPropertiesMissing:
                        return 400;

                    default:
                        return 500;
                }
            }
        }

        /// <summary>
        /// Additional contextual information.
        /// </summary>
        public object Context { get; set; } = null;

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public ApiErrorResponse()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="error">Error code.</param>
        /// <param name="context">Context.</param>
        /// <param name="description">Description.</param>
        /// 
        public ApiErrorResponse(ApiErrorEnum error, object context = null, string description = null)
        {
            Error = error;
            Context = context;
            Description = description;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
