namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using DocumentAtom.Core.Helpers;
    using Touchstone.Core;

    /// <summary>
    /// Suite covering <see cref="HashHelper"/> byte-array hashing over the various input overloads.
    /// </summary>
    internal static class HashHelperSuites
    {
        internal static TestSuiteDescriptor Build()
        {
            return new SuiteBuilder("Core.HashHelper")
                .Case("Bytes.Lengths", "MD5, SHA1, and SHA256 over bytes produce 16/20/32-byte digests", () =>
                {
                    byte[] data = new byte[] { 1, 2, 3, 4, 5 };
                    Check.Equal(16, HashHelper.MD5Hash(data).Length);
                    Check.Equal(20, HashHelper.SHA1Hash(data).Length);
                    Check.Equal(32, HashHelper.SHA256Hash(data).Length);
                })
                .Case("String.Lengths", "Hashing a string produces digests of the expected length", () =>
                {
                    Check.Equal(16, HashHelper.MD5Hash("test").Length);
                    Check.Equal(20, HashHelper.SHA1Hash("test").Length);
                    Check.Equal(32, HashHelper.SHA256Hash("test").Length);
                })
                .Case("String.Deterministic", "Hashing the same string twice yields identical bytes", () =>
                {
                    byte[] a = HashHelper.SHA256Hash("repeatable");
                    byte[] b = HashHelper.SHA256Hash("repeatable");
                    Check.True(Equal(a, b));
                })
                .Case("List.Empty", "Hashing a null string list yields an empty byte array", () =>
                {
                    Check.Equal(0, HashHelper.MD5Hash((List<string>)null!).Length);
                })
                .Case("List.NonEmpty", "Hashing a populated string list yields a 32-byte SHA256 digest", () =>
                {
                    Check.Equal(32, HashHelper.SHA256Hash(new List<string> { "a", "b", "c" }).Length);
                })
                .Case("Stream.Valid", "Hashing a readable, seekable stream yields a 16-byte MD5 digest", () =>
                {
                    using MemoryStream ms = new MemoryStream(new byte[] { 9, 8, 7, 6 });
                    Check.Equal(16, HashHelper.MD5Hash(ms).Length);
                })
                .Case("Stream.Null", "Hashing a null stream throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => HashHelper.MD5Hash((Stream)null!));
                })
                .Case("DataTable.Valid", "Hashing a DataTable yields a 32-byte SHA256 digest", () =>
                {
                    using DataTable dt = new DataTable();
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("Age", typeof(int));
                    dt.Rows.Add("Alice", 30);
                    Check.Equal(32, HashHelper.SHA256Hash(dt).Length);
                })
                .Build("Core: HashHelper");
        }

        private static bool Equal(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
