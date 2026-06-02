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

    internal sealed class TextAtomRouteHandlers
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private readonly ServerRuntimeContext _Context;
        private readonly AtomRequestProcessor _Processor;

        public TextAtomRouteHandlers(ServerRuntimeContext context, AtomRequestProcessor processor)
        {
            _Context = context;
            _Processor = processor;
        }
        public async Task CsvAtomRoute(HttpContextBase ctx)
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

            CsvProcessorSettings settings = _Processor.ApplyApiSettings(new CsvProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (CsvProcessor processor = new CsvProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task HtmlAtomRoute(HttpContextBase ctx)
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

            HtmlProcessorSettings settings = _Processor.ApplyApiSettings(new HtmlProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (HtmlProcessor processor = new HtmlProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, apiSettings?.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task JsonAtomRoute(HttpContextBase ctx)
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

            JsonProcessorSettings settings = _Processor.ApplyApiSettings(new JsonProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (JsonProcessor processor = new JsonProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task MarkdownAtomRoute(HttpContextBase ctx)
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

            MarkdownProcessorSettings settings = _Processor.ApplyApiSettings(new MarkdownProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (MarkdownProcessor processor = new MarkdownProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task TextAtomRoute(HttpContextBase ctx)
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

            TextProcessorSettings settings = _Processor.ApplyApiSettings(new TextProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (TextProcessor processor = new TextProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                _Processor.ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Context.Serializer.SerializeJson(ret, true));
                return;
            }
        }

        public async Task XmlAtomRoute(HttpContextBase ctx)
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

            XmlProcessorSettings settings = _Processor.ApplyApiSettings(new XmlProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (XmlProcessor processor = new XmlProcessor(settings))
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

