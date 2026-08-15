namespace DocumentAtom.Testing.Shared.Suites
{
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.DataIngestion;
    using Touchstone.Core;

    /// <summary>
    /// Suite verifying the stable integer values of the public enums (guards against reordering
    /// that would break serialized data and wire compatibility).
    /// </summary>
    internal static class EnumSuites
    {
        internal static TestSuiteDescriptor Build()
        {
            return new SuiteBuilder("Core.Enums")
                .Case("ChunkStrategyEnum", "ChunkStrategyEnum values are stable", () =>
                {
                    Check.Equal(0, (int)ChunkStrategyEnum.FixedTokenCount);
                    Check.Equal(1, (int)ChunkStrategyEnum.SentenceBased);
                    Check.Equal(2, (int)ChunkStrategyEnum.ParagraphBased);
                    Check.Equal(3, (int)ChunkStrategyEnum.RegexBased);
                    Check.Equal(4, (int)ChunkStrategyEnum.WholeList);
                    Check.Equal(5, (int)ChunkStrategyEnum.ListEntry);
                    Check.Equal(6, (int)ChunkStrategyEnum.Row);
                    Check.Equal(7, (int)ChunkStrategyEnum.RowWithHeaders);
                    Check.Equal(8, (int)ChunkStrategyEnum.RowGroupWithHeaders);
                    Check.Equal(9, (int)ChunkStrategyEnum.KeyValuePairs);
                    Check.Equal(10, (int)ChunkStrategyEnum.WholeTable);
                })
                .Case("OverlapStrategyEnum", "OverlapStrategyEnum values are stable", () =>
                {
                    Check.Equal(0, (int)OverlapStrategyEnum.SlidingWindow);
                    Check.Equal(1, (int)OverlapStrategyEnum.SentenceBoundaryAware);
                    Check.Equal(2, (int)OverlapStrategyEnum.SemanticBoundaryAware);
                })
                .Case("AtomTypeEnum", "AtomTypeEnum values are stable", () =>
                {
                    Check.Equal(0, (int)AtomTypeEnum.Text);
                    Check.Equal(1, (int)AtomTypeEnum.List);
                    Check.Equal(2, (int)AtomTypeEnum.Binary);
                    Check.Equal(3, (int)AtomTypeEnum.Table);
                    Check.Equal(4, (int)AtomTypeEnum.Unknown);
                    Check.Equal(5, (int)AtomTypeEnum.Image);
                    Check.Equal(6, (int)AtomTypeEnum.Hyperlink);
                    Check.Equal(7, (int)AtomTypeEnum.Code);
                    Check.Equal(8, (int)AtomTypeEnum.Meta);
                })
                .Case("SeverityEnum", "SeverityEnum values are stable", () =>
                {
                    Check.Equal(0, (int)SeverityEnum.Debug);
                    Check.Equal(1, (int)SeverityEnum.Info);
                    Check.Equal(2, (int)SeverityEnum.Warn);
                    Check.Equal(3, (int)SeverityEnum.Error);
                    Check.Equal(4, (int)SeverityEnum.Alert);
                    Check.Equal(5, (int)SeverityEnum.Critical);
                    Check.Equal(6, (int)SeverityEnum.Emergency);
                })
                .Case("MarkdownFormattingEnum", "MarkdownFormattingEnum values are stable", () =>
                {
                    Check.Equal(0, (int)MarkdownFormattingEnum.Text);
                    Check.Equal(1, (int)MarkdownFormattingEnum.Header);
                    Check.Equal(2, (int)MarkdownFormattingEnum.Code);
                    Check.Equal(3, (int)MarkdownFormattingEnum.UnorderedList);
                    Check.Equal(4, (int)MarkdownFormattingEnum.OrderedList);
                    Check.Equal(5, (int)MarkdownFormattingEnum.Link);
                    Check.Equal(6, (int)MarkdownFormattingEnum.Image);
                    Check.Equal(7, (int)MarkdownFormattingEnum.Url);
                    Check.Equal(8, (int)MarkdownFormattingEnum.Table);
                })
                .Case("DocumentTypeEnum.Endpoints", "DocumentTypeEnum boundary values are stable", () =>
                {
                    Check.Equal(0, (int)DocumentTypeEnum.Unknown);
                    Check.Equal(1, (int)DocumentTypeEnum.Bmp);
                    Check.Equal(25, (int)DocumentTypeEnum.Pdf);
                    Check.Equal(36, (int)DocumentTypeEnum.Text);
                    Check.Equal(42, (int)DocumentTypeEnum.Xml);
                })
                .Case("IngestionElementType", "IngestionElementType values are stable", () =>
                {
                    Check.Equal(0, (int)IngestionElementType.Paragraph);
                    Check.Equal(1, (int)IngestionElementType.Header);
                    Check.Equal(2, (int)IngestionElementType.Table);
                    Check.Equal(3, (int)IngestionElementType.Image);
                    Check.Equal(4, (int)IngestionElementType.List);
                    Check.Equal(5, (int)IngestionElementType.Code);
                    Check.Equal(6, (int)IngestionElementType.Binary);
                    Check.Equal(7, (int)IngestionElementType.Unknown);
                })
                .Build("Core: Enum values");
        }
    }
}
