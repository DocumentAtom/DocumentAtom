namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Threading.Tasks;
    using WatsonWebserver.Core;

    internal sealed class ServerRouteHandlers
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private readonly ServerRuntimeContext _Context;

        public ServerRouteHandlers(ServerRuntimeContext context)
        {
            _Context = context;
        }

        public async Task PreflightRoute(HttpContextBase ctx)
        {
            NameValueCollection responseHeaders = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            string[] requestedHeaders = null;
            string headers = "";

            if (ctx.Request.Headers != null)
            {
                for (int i = 0; i < ctx.Request.Headers.Count; i++)
                {
                    string key = ctx.Request.Headers.GetKey(i);
                    string value = ctx.Request.Headers.Get(i);
                    if (String.IsNullOrEmpty(key)) continue;
                    if (String.IsNullOrEmpty(value)) continue;
                    if (String.Compare(key.ToLower(), "access-control-request-headers") == 0)
                    {
                        requestedHeaders = value.Split(',');
                        break;
                    }
                }
            }

            if (requestedHeaders != null)
            {
                foreach (string curr in requestedHeaders)
                {
                    headers += ", " + curr;
                }
            }

            ApplyCorsHeaders(responseHeaders, headers);
            responseHeaders.Add("Accept", "*/*");
            responseHeaders.Add("Accept-Language", "en-US, en");
            responseHeaders.Add("Accept-Charset", "ISO-8859-1, utf-8");
            responseHeaders.Add("Connection", "keep-alive");

            ctx.Response.StatusCode = 200;
            ctx.Response.Headers = responseHeaders;
            await ctx.Response.Send();
            return;
        }

        public async Task PreRoutingRoute(HttpContextBase ctx)
        {
            ctx.Response.ContentType = Constants.JsonContentType;
            if (ctx.Response.Headers == null)
                ctx.Response.Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            ApplyCorsHeaders(ctx.Response.Headers, null);
        }

        private void ApplyCorsHeaders(NameValueCollection headers, string requestedHeaders)
        {
            if (headers == null) return;
            if (_Context.Settings?.Cors == null || !_Context.Settings.Cors.Enable) return;

            string allowedHeaders = String.Join(", ", _Context.Settings.Cors.AllowHeaders);
            if (!String.IsNullOrEmpty(requestedHeaders))
                allowedHeaders += ", " + requestedHeaders.Trim().TrimStart(',');

            headers.Set("Access-Control-Allow-Origin", _Context.Settings.Cors.AllowOrigin);
            headers.Set("Access-Control-Allow-Methods", String.Join(", ", _Context.Settings.Cors.AllowMethods));
            headers.Set("Access-Control-Allow-Headers", allowedHeaders);
            headers.Set("Access-Control-Expose-Headers", String.Join(", ", _Context.Settings.Cors.ExposeHeaders));
        }

        public void ApplyCorsSettingsToWebserverDefaults()
        {
            if (_Context.Settings?.Webserver?.Headers?.DefaultHeaders == null) return;
            if (_Context.Settings.Cors == null || !_Context.Settings.Cors.Enable) return;

            _Context.Settings.Webserver.Headers.DefaultHeaders[WebserverConstants.HeaderAccessControlAllowOrigin] = _Context.Settings.Cors.AllowOrigin;
            _Context.Settings.Webserver.Headers.DefaultHeaders[WebserverConstants.HeaderAccessControlAllowMethods] = String.Join(", ", _Context.Settings.Cors.AllowMethods);
            _Context.Settings.Webserver.Headers.DefaultHeaders[WebserverConstants.HeaderAccessControlAllowHeaders] = String.Join(", ", _Context.Settings.Cors.AllowHeaders);
            _Context.Settings.Webserver.Headers.DefaultHeaders[WebserverConstants.HeaderAccessControlExposeHeaders] = String.Join(", ", _Context.Settings.Cors.ExposeHeaders);
        }

        public async Task PostRoutingRoute(HttpContextBase ctx)
        {
            _Context.Logging.Debug(
                _Context.Header + 
                ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " " +
                "from " + ctx.Request.Source.IpAddress + ": " + 
                ctx.Response.StatusCode + " " +
                "(" + ctx.Timestamp.TotalMs!.Value.ToString("F2") + "ms)");
        }

        public async Task RootRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.HtmlContentType;
            await ctx.Response.Send(Constants.HtmlHomepage);
        }

        public async Task GetFavicon(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.FaviconContentType;
            await ctx.Response.Send(File.ReadAllBytes(Constants.FaviconFilename));
        }

        public async Task HeadFavicon(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.FaviconContentType;
            await ctx.Response.Send();
        }

        public async Task ExceptionRoute(HttpContextBase ctx, Exception e)
        {
            if (e is System.IO.FileFormatException)
            {
                _Context.Logging.Warn(_Context.Header + "file format exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is DocumentFormat.OpenXml.Packaging.OpenXmlPackageException)
            {
                _Context.Logging.Warn(_Context.Header + "OpenXML package exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is System.IO.InvalidDataException)
            {
                _Context.Logging.Warn(_Context.Header + "invalid data exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is UglyToad.PdfPig.Core.PdfDocumentFormatException)
            {
                _Context.Logging.Warn(_Context.Header + "PDF document format exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else
            {
                _Context.Logging.Warn(
                    _Context.Header +
                    "handling exception for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " " +
                    "from " + ctx.Request.Source.IpAddress +
                    Environment.NewLine +
                    e.ToString());

                ctx.Response.StatusCode = 500;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.InternalError, null, e.Message), true));
                return;
            }
        }

        public async Task DefaultRoute(HttpContextBase ctx)
        {
            _Context.Logging.Warn(_Context.Header + "unknown verb or endpoint " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " from " + ctx.Request.Source.IpAddress);
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Unknown verb or endpoint."), true));
            return;
        }

        public async Task LoopbackRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.TextContentType;
            await ctx.Response.Send();
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}

