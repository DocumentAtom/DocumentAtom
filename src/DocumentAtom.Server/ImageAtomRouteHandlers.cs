namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.Text;
    using DocumentAtom.Text.Csv;
    using DocumentAtom.Text.Html;
    using DocumentAtom.Text.Json;
    using DocumentAtom.Text.Markdown;
    using DocumentAtom.Text.Xml;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Ocr;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Word;
    using WatsonWebserver.Core;

    internal sealed class ImageAtomRouteHandlers
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private readonly ServerRuntimeContext _Context;
        private readonly AtomRequestProcessor _Processor;

        public ImageAtomRouteHandlers(ServerRuntimeContext context, AtomRequestProcessor processor)
        {
            _Context = context;
            _Processor = processor;
        }
        public async Task OcrAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                AtomRouteRequest request = _Processor.ParseRequest(ctx);
                data = request.Data;
                apiSettings = request.Settings;
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            using (ImageContentExtractor ice = new ImageContentExtractor(_Context.Settings.Tesseract.DataDirectory, _Context.Settings.Tesseract.Language))
            {
                ExtractionResult er = ice.ExtractContent(data);
                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(er, true));
                return;
            }
        }

        public async Task PngAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                AtomRouteRequest request = _Processor.ParseRequest(ctx);
                data = request.Data;
                apiSettings = request.Settings;
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            ImageProcessorSettings settings = _Processor.BuildImageSettings(apiSettings);

            List<Atom> ret = new List<Atom>();

            using (ImageProcessor processor = new ImageProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task TypeDetectionRoute(HttpContextBase ctx)
        {
            if (ctx.Request.DataAsBytes == null || ctx.Request.ContentLength < 1)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            byte[] data = ctx.Request.DataAsBytes;

            string contentType = ctx.Request.ContentType;
            if (String.IsNullOrEmpty(contentType))
                contentType = "application/octet-stream";

            string dir = "./" + Guid.NewGuid() + "/";
            TypeResult tr = new TypeResult();

            try
            {
                using (TypeDetector td = new TypeDetector(dir))
                {
                    tr = td.Process(data, contentType);
                }
            }
            finally
            {
                FileHelper.RecursiveDelete(new DirectoryInfo(dir), true);
                Directory.Delete(dir);
            }

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Context.Serializer.SerializeJson(tr, true));
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}

