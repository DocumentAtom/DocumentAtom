namespace DocumentAtom.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Cryptographic hash helper.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// Generate an MD5 hash.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>MD5 hash.</returns>
        public static byte[] MD5Hash(byte[] data)
        {
            if (data == null || data.Length < 1) data = Array.Empty<byte>();
            using (MD5 hash = System.Security.Cryptography.MD5.Create())
            {
                return hash.ComputeHash(data);
            }
        }

        /// <summary>
        /// Generate an MD5 hash.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>MD5 hash.</returns>
        public static byte[] MD5Hash(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Unable to read from supplied stream.");
            if (!stream.CanSeek) throw new ArgumentException("Unable to seek in supplied stream.");

            stream.Seek(0, SeekOrigin.Begin);

            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                return md5.ComputeHash(stream);
            }
        }

        /// <summary>
        /// Generate a SHA1 hash of a byte array.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>SHA1 hash.</returns>
        public static byte[] SHA1Hash(byte[] data)
        {
            if (data == null || data.Length < 1) data = Array.Empty<byte>();
            using (SHA1 hash = SHA1.Create())
            {
                return hash.ComputeHash(data);
            }
        }

        /// <summary>
        /// Generate a SHA1 hash.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>MD5 hash.</returns>
        public static byte[] SHA1Hash(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Unable to read from supplied stream.");
            if (!stream.CanSeek) throw new ArgumentException("Unable to seek in supplied stream.");

            stream.Seek(0, SeekOrigin.Begin);

            using (SHA1 sha1 = System.Security.Cryptography.SHA1.Create())
            {
                return sha1.ComputeHash(stream);
            }
        }

        /// <summary>
        /// Generate a SHA256 hash of a byte array.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>SHA256 hash.</returns>
        public static byte[] SHA256Hash(byte[] data)
        {
            if (data == null || data.Length < 1) data = Array.Empty<byte>();
            using (SHA256 hash = System.Security.Cryptography.SHA256.Create())
            {
                return hash.ComputeHash(data);
            }
        }

        /// <summary>
        /// Generate a SHA256 hash.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>MD5 hash.</returns>
        public static byte[] SHA256Hash(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Unable to read from supplied stream.");
            if (!stream.CanSeek) throw new ArgumentException("Unable to seek in supplied stream.");

            stream.Seek(0, SeekOrigin.Begin);

            using (SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return sha256.ComputeHash(stream);
            }
        }
    }
}
