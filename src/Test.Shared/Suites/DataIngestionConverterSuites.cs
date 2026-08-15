namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.DataIngestion;
    using DocumentAtom.DataIngestion.Converters;
    using DocumentAtom.DataIngestion.Metadata;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the atom-to-element converter, the metadata serializer, and the metadata key constants.
    /// </summary>
    internal static class DataIngestionConverterSuites
    {
        internal static TestSuiteDescriptor Converter()
        {
            return new SuiteBuilder("DataIngestion.Converter")
                .Case("Convert.Null", "Converting a null atom returns null", () =>
                {
                    AtomToIngestionElementConverter c = new AtomToIngestionElementConverter();
                    Check.Null(c.Convert(null!));
                })
                .Case("Convert.Meta", "A Meta atom converts to null", () =>
                {
                    AtomToIngestionElementConverter c = new AtomToIngestionElementConverter();
                    Atom atom = new Atom();
                    atom.Type = AtomTypeEnum.Meta;
                    atom.Text = "ignored";
                    Check.Null(c.Convert(atom));
                })
                .Case("Convert.EmptyContent", "An atom with no content or binary converts to null", () =>
                {
                    AtomToIngestionElementConverter c = new AtomToIngestionElementConverter();
                    Atom atom = new Atom();
                    atom.Type = AtomTypeEnum.Text;
                    atom.Text = null;
                    Check.Null(c.Convert(atom));
                })
                .Case("Convert.TextParagraph", "A text atom converts to a paragraph element", () =>
                {
                    AtomToIngestionElementConverter c = new AtomToIngestionElementConverter();
                    Atom atom = Atom.FromTextContent("some paragraph text", 0, new ChunkingConfiguration());
                    IngestionDocumentElement? element = c.Convert(atom);
                    Check.NotNull(element);
                    Check.Equal(IngestionElementType.Paragraph, element!.ElementType);
                    Check.Equal(atom.GUID.ToString(), element.Id);
                    Check.Contains("some paragraph text", element.Content!);
                })
                .Build("DataIngestion: Converter");
        }

        internal static TestSuiteDescriptor Serializer()
        {
            return new SuiteBuilder("DataIngestion.MetadataSerializer")
                .Case("SerializeAtomMetadata.CoreKeys", "Serializing atom metadata always emits the core keys", () =>
                {
                    Atom atom = Atom.FromTextContent("hello", 0, new ChunkingConfiguration());
                    Dictionary<string, object> meta = MetadataSerializer.SerializeAtomMetadata(atom);
                    Check.True(meta.ContainsKey(AtomMetadataKeys.AtomGuid));
                    Check.True(meta.ContainsKey(AtomMetadataKeys.AtomType));
                    Check.True(meta.ContainsKey(AtomMetadataKeys.AtomLength));
                })
                .Case("SerializeAtomMetadata.Null", "Serializing null atom metadata throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => MetadataSerializer.SerializeAtomMetadata(null!));
                })
                .Case("SerializeAtom.Roundtrip", "An atom survives a serialize/deserialize round trip", () =>
                {
                    Atom atom = Atom.FromTextContent("roundtrip", 2, new ChunkingConfiguration());
                    string json = MetadataSerializer.SerializeAtom(atom);
                    Atom? back = MetadataSerializer.DeserializeAtom(json);
                    Check.NotNull(back);
                    Check.Equal(atom.GUID, back!.GUID);
                    Check.Equal("roundtrip", back.Text);
                })
                .Case("SerializeAtom.Null", "Serializing a null atom throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => MetadataSerializer.SerializeAtom(null!));
                })
                .Case("DeserializeAtom.NullOrInvalid", "Deserializing null or garbage returns null", () =>
                {
                    Check.Null(MetadataSerializer.DeserializeAtom(null!));
                    Check.Null(MetadataSerializer.DeserializeAtom("not json at all"));
                })
                .Case("HashToHex.Roundtrip", "Hash-to-hex and back is a faithful round trip", () =>
                {
                    byte[] hash = new byte[] { 0xAB, 0xCD, 0xEF };
                    string hex = MetadataSerializer.HashToHexString(hash);
                    Check.Equal("abcdef", hex);
                    byte[] back = MetadataSerializer.HexStringToHash(hex);
                    Check.Equal(hash.Length, back.Length);
                    Check.Equal(hash[0], back[0]);
                })
                .Case("HashToHex.Null", "Hashing null yields an empty string", () =>
                {
                    Check.Equal(string.Empty, MetadataSerializer.HashToHexString(null!));
                })
                .Case("ExtractAtomFromMetadata.Missing", "Extracting from metadata without the serialized key returns null", () =>
                {
                    Check.Null(MetadataSerializer.ExtractAtomFromMetadata(new Dictionary<string, object>()));
                })
                .Build("DataIngestion: MetadataSerializer");
        }

        internal static TestSuiteDescriptor Keys()
        {
            return new SuiteBuilder("DataIngestion.AtomMetadataKeys")
                .Case("Values", "Metadata key constants have their expected namespaced values", () =>
                {
                    Check.Equal("atom:guid", AtomMetadataKeys.AtomGuid);
                    Check.Equal("atom:type", AtomMetadataKeys.AtomType);
                    Check.Equal("atom:header_level", AtomMetadataKeys.AtomHeaderLevel);
                    Check.Equal("chunk:source", AtomMetadataKeys.ChunkSource);
                    Check.Equal("chunk:index", AtomMetadataKeys.ChunkIndex);
                    Check.Equal("source:filename", AtomMetadataKeys.SourceFilename);
                    Check.Equal("hierarchy:level", AtomMetadataKeys.HierarchyLevel);
                    Check.Equal("section:title", AtomMetadataKeys.SectionTitle);
                })
                .Build("DataIngestion: AtomMetadataKeys");
        }
    }
}
