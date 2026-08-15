namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Helpers;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering StringHelper, DataTableHelper, ProcessorSettingsBase, and the API request models.
    /// </summary>
    internal static class CoreHelperSuites
    {
        internal static TestSuiteDescriptor StringHelperSuite()
        {
            return new SuiteBuilder("Core.StringHelper")
                .Case("RemoveBinaryData.Null", "RemoveBinaryData returns null unchanged", () =>
                {
                    Check.Null(StringHelper.RemoveBinaryData(null!));
                })
                .Case("RemoveBinaryData.KeepsPrintable", "RemoveBinaryData keeps printable text", () =>
                {
                    Check.Equal("abc", StringHelper.RemoveBinaryData("abc"));
                })
                .Case("RemoveBinaryData.KeepsWhitespace", "RemoveBinaryData preserves whitespace between words", () =>
                {
                    Check.Equal("a b", StringHelper.RemoveBinaryData("a b"));
                })
                .Case("GetSubstrings.ShortText", "A string within the maximum length is returned once", () =>
                {
                    List<string> parts = StringHelper.GetSubstringsFromString("short text", 100, 10, 50).ToList();
                    Check.Single(parts);
                })
                .Case("GetSubstrings.InvalidMaxLength", "A non-positive maximum length throws ArgumentException", () =>
                {
                    Check.Throws<ArgumentException>(() => StringHelper.GetSubstringsFromString("text", 0, 10, 50).ToList());
                })
                .Case("GetSubstrings.InvalidShift", "A non-positive shift size throws ArgumentException", () =>
                {
                    Check.Throws<ArgumentException>(() => StringHelper.GetSubstringsFromString("a much longer piece of text here", 5, 0, 50).ToList());
                })
                .Case("GetFullWordsFromRange.Null", "GetFullWordsFromRange returns null for null input", () =>
                {
                    Check.Null(StringHelper.GetFullWordsFromRange(null!, 0, 5, 10));
                })
                .Build("Core: StringHelper");
        }

        internal static TestSuiteDescriptor DataTableHelperSuite()
        {
            return new SuiteBuilder("Core.DataTableHelper")
                .Case("GetLength.Null", "GetLength returns zero for a null table", () =>
                {
                    Check.Equal(0, DataTableHelper.GetLength(null!));
                })
                .Case("GetLength.Populated", "GetLength returns a positive value for a populated table", () =>
                {
                    using DataTable dt = new DataTable();
                    dt.Columns.Add("Name", typeof(string));
                    dt.Rows.Add("Alice");
                    Check.True(DataTableHelper.GetLength(dt) > 0);
                })
                .Case("GetLength.EmptyTable", "GetLength returns zero for a table with no rows", () =>
                {
                    using DataTable dt = new DataTable();
                    Check.Equal(0, DataTableHelper.GetLength(dt));
                })
                .Build("Core: DataTableHelper");
        }

        internal static TestSuiteDescriptor SettingsBaseSuite()
        {
            return new SuiteBuilder("Core.ProcessorSettingsBase")
                .Case("Defaults", "Default settings values are correct", () =>
                {
                    ProcessorSettingsBase s = new ProcessorSettingsBase();
                    Check.True(s.TrimText);
                    Check.True(s.RemoveBinaryFromText);
                    Check.True(s.ExtractAtomsFromImages);
                    Check.Equal(8192, s.StreamBufferSize);
                    Check.NotNull(s.Chunking);
                    Check.NotNull(s.TempDirectory);
                })
                .Case("StreamBufferSize.Invalid", "StreamBufferSize rejects values below one", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ProcessorSettingsBase().StreamBufferSize = 0);
                })
                .Case("TempDirectory.Null", "TempDirectory rejects null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new ProcessorSettingsBase().TempDirectory = null!);
                })
                .Case("Chunking.Null", "Chunking rejects null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new ProcessorSettingsBase().Chunking = null!);
                })
                .Build("Core: ProcessorSettingsBase");
        }

        internal static TestSuiteDescriptor ApiSuite()
        {
            return new SuiteBuilder("Core.Api")
                .Case("ApiProcessorSettings.Defaults", "ApiProcessorSettings starts with all null fields", () =>
                {
                    ApiProcessorSettings s = new ApiProcessorSettings();
                    Check.Null(s.TrimText);
                    Check.Null(s.Chunking);
                    Check.Null(s.Delimiters);
                })
                .Case("AtomRequest.Data.Null", "AtomRequest.Data rejects null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new AtomRequest().Data = null!);
                })
                .Case("AtomRequest.Data.Empty", "AtomRequest.Data rejects empty", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new AtomRequest().Data = string.Empty);
                })
                .Case("AtomRequest.GetDataBytes.Valid", "GetDataBytes decodes valid base64", () =>
                {
                    AtomRequest r = new AtomRequest();
                    r.Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("hello"));
                    byte[] bytes = r.GetDataBytes();
                    Check.Equal("hello", System.Text.Encoding.UTF8.GetString(bytes));
                })
                .Case("AtomRequest.GetDataBytes.Invalid", "GetDataBytes throws FormatException for non-base64 data", () =>
                {
                    AtomRequest r = new AtomRequest();
                    r.Data = "!!!not base64!!!";
                    Check.Throws<FormatException>(() => r.GetDataBytes());
                })
                .Build("Core: API models");
        }
    }
}
