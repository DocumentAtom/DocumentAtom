namespace DocumentAtom.DataIngestion.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using DocumentAtom.Core.Atoms;

    /// <summary>
    /// Utilities for serializing and deserializing metadata.
    /// </summary>
    public static class MetadataSerializer
    {
        #region Public-Members

        /// <summary>
        /// JSON serializer options for metadata.
        /// </summary>
        public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        #endregion

        #region Public-Methods

        /// <summary>
        /// Serializes an Atom's properties to a metadata dictionary.
        /// </summary>
        /// <param name="atom">The atom to serialize.</param>
        /// <returns>Metadata dictionary.</returns>
        public static Dictionary<string, object> SerializeAtomMetadata(Atom atom)
        {
            if (atom == null) throw new ArgumentNullException(nameof(atom));

            Dictionary<string, object> metadata = new Dictionary<string, object>();

            // Required properties
            metadata[AtomMetadataKeys.AtomGuid] = atom.GUID.ToString();
            metadata[AtomMetadataKeys.AtomType] = atom.Type.ToString();
            metadata[AtomMetadataKeys.AtomLength] = atom.Length;

            // Optional properties
            if (atom.ParentGUID.HasValue)
            {
                metadata[AtomMetadataKeys.AtomParentGuid] = atom.ParentGUID.Value.ToString();
            }

            if (atom.PageNumber.HasValue)
            {
                metadata[AtomMetadataKeys.AtomPageNumber] = atom.PageNumber.Value;
            }

            if (atom.Position.HasValue)
            {
                metadata[AtomMetadataKeys.AtomPosition] = atom.Position.Value;
            }

            if (atom.MD5Hash != null)
            {
                metadata[AtomMetadataKeys.AtomMd5] = Convert.ToBase64String(atom.MD5Hash);
            }

            if (atom.SHA1Hash != null)
            {
                metadata[AtomMetadataKeys.AtomSha1] = Convert.ToBase64String(atom.SHA1Hash);
            }

            if (atom.SHA256Hash != null)
            {
                metadata[AtomMetadataKeys.AtomSha256] = Convert.ToBase64String(atom.SHA256Hash);
            }

            if (atom.HeaderLevel.HasValue)
            {
                metadata[AtomMetadataKeys.AtomHeaderLevel] = atom.HeaderLevel.Value;
            }

            if (!string.IsNullOrEmpty(atom.Title))
            {
                metadata[AtomMetadataKeys.AtomTitle] = atom.Title;
            }

            if (!string.IsNullOrEmpty(atom.Subtitle))
            {
                metadata[AtomMetadataKeys.AtomSubtitle] = atom.Subtitle;
            }

            if (atom.Formatting.HasValue)
            {
                metadata[AtomMetadataKeys.AtomFormatting] = atom.Formatting.Value.ToString();
            }

            if (atom.BoundingBox != null)
            {
                metadata[AtomMetadataKeys.AtomBoundingBox] = JsonSerializer.Serialize(atom.BoundingBox, JsonOptions);
            }

            if (!string.IsNullOrEmpty(atom.SheetName))
            {
                metadata[AtomMetadataKeys.AtomSheetName] = atom.SheetName;
            }

            if (!string.IsNullOrEmpty(atom.CellIdentifier))
            {
                metadata[AtomMetadataKeys.AtomCellId] = atom.CellIdentifier;
            }

            if (atom.Rows.HasValue)
            {
                metadata[AtomMetadataKeys.AtomRows] = atom.Rows.Value;
            }

            if (atom.Columns.HasValue)
            {
                metadata[AtomMetadataKeys.AtomColumns] = atom.Columns.Value;
            }

            // Quark information
            bool hasQuarks = atom.Quarks != null && atom.Quarks.Count > 0;
            metadata[AtomMetadataKeys.AtomHasQuarks] = hasQuarks;

            if (hasQuarks)
            {
                metadata[AtomMetadataKeys.AtomQuarkCount] = atom.Quarks!.Count;
            }

            return metadata;
        }

        /// <summary>
        /// Serializes the full Atom object to JSON.
        /// </summary>
        /// <param name="atom">The atom to serialize.</param>
        /// <returns>JSON string.</returns>
        public static string SerializeAtom(Atom atom)
        {
            if (atom == null) throw new ArgumentNullException(nameof(atom));
            return JsonSerializer.Serialize(atom, JsonOptions);
        }

        /// <summary>
        /// Deserializes an Atom from JSON.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <returns>The deserialized Atom, or null if deserialization fails.</returns>
        public static Atom? DeserializeAtom(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonSerializer.Deserialize<Atom>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts an Atom from metadata if it was serialized.
        /// </summary>
        /// <param name="metadata">The metadata dictionary.</param>
        /// <returns>The deserialized Atom, or null if not found or deserialization fails.</returns>
        public static Atom? ExtractAtomFromMetadata(IDictionary<string, object> metadata)
        {
            if (metadata == null) return null;

            if (metadata.TryGetValue(AtomMetadataKeys.AtomSerialized, out object? value) && value is string json)
            {
                return DeserializeAtom(json);
            }

            return null;
        }

        /// <summary>
        /// Converts a byte array hash to a hex string.
        /// </summary>
        /// <param name="hash">Hash bytes.</param>
        /// <returns>Hex string.</returns>
        public static string HashToHexString(byte[] hash)
        {
            if (hash == null) return string.Empty;
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="hex">Hex string.</param>
        /// <returns>Byte array.</returns>
        public static byte[] HexStringToHash(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();
            return Convert.FromHexString(hex);
        }

        #endregion
    }
}
