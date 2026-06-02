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

    internal sealed class DocumentAtomRouteHandlers
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private readonly ServerRuntimeContext _Context;
        private readonly AtomRequestProcessor _Processor;

        public DocumentAtomRouteHandlers(ServerRuntimeContext context, AtomRequestProcessor processor)
        {
            _Context = context;
            _Processor = processor;
        }
        public async Task ExcelAtomRoute(HttpContextBase ctx)
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

            XlsxProcessorSettings settings = _Processor.ApplyApiSettings(new XlsxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = _Processor.BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (XlsxProcessor processor = new XlsxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task PdfAtomRoute(HttpContextBase ctx)
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

            PdfProcessorSettings settings = _Processor.ApplyApiSettings(new PdfProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = _Processor.BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (PdfProcessor processor = new PdfProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task PowerPointAtomRoute(HttpContextBase ctx)
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

            PptxProcessorSettings settings = _Processor.ApplyApiSettings(new PptxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = _Processor.BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (PptxProcessor processor = new PptxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task RtfAtomRoute(HttpContextBase ctx)
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

            RtfProcessorSettings settings = _Processor.ApplyApiSettings(new RtfProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = _Processor.BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (RtfProcessor processor = new RtfProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task WordAtomRoute(HttpContextBase ctx)
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

            DocxProcessorSettings settings = _Processor.ApplyApiSettings(new DocxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = _Processor.BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (DocxProcessor processor = new DocxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}

