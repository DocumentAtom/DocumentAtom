namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Core.TextTools;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the text-processing utilities: Lemmatizer, StringSequenceReplacer,
    /// StringSplitter, WordRemover, and TokenExtractor.
    /// </summary>
    internal static class TextToolsSuites
    {
        internal static TestSuiteDescriptor Lemmatizer()
        {
            return new SuiteBuilder("Core.Lemmatizer")
                .Case("IrregularNoun", "Irregular nouns are lemmatized via the dictionary", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Equal("child", lem.Process("children"));
                    Check.Equal("mouse", lem.Process("mice"));
                })
                .Case("IrregularVerb", "Irregular verbs are lemmatized via the dictionary", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Equal("go", lem.Process("went"));
                })
                .Case("Morphological", "Comparative and superlative forms map to their base", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Equal("good", lem.Process("better"));
                    Check.Equal("good", lem.Process("best"));
                })
                .Case("RegularPlural", "Regular plurals lose their trailing s", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Equal("cat", lem.Process("cats"));
                })
                .Case("Preserved", "Preserved terms are returned unchanged", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Equal("computing", lem.Process("computing"));
                })
                .Case("NullOrEmpty", "Null and whitespace input is returned unchanged", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Null(lem.Process((string)null!));
                    Check.Equal("   ", lem.Process("   "));
                })
                .Case("Array.Null", "Processing a null array yields an empty array", () =>
                {
                    using Lemmatizer lem = new Lemmatizer();
                    Check.Empty(lem.Process((string[])null!));
                })
                .Build("Core: Lemmatizer");
        }

        internal static TestSuiteDescriptor Replacer()
        {
            return new SuiteBuilder("Core.StringSequenceReplacer")
                .Case("Default.PunctuationToSpace", "Default replacements lowercase and strip punctuation", () =>
                {
                    using StringSequenceReplacer r = new StringSequenceReplacer();
                    string result = r.Process("Hello, World!");
                    Check.Contains("hello", result);
                    Check.Contains("world", result);
                    Check.DoesNotContain(",", result);
                })
                .Case("Empty.ReturnsEmpty", "Null or empty input returns an empty string", () =>
                {
                    using StringSequenceReplacer r = new StringSequenceReplacer();
                    Check.Equal(string.Empty, r.Process((string)null!));
                    Check.Equal(string.Empty, r.Process(string.Empty));
                })
                .Case("Custom.Replacements", "A custom replacement dictionary is honored", () =>
                {
                    Dictionary<string, string> map = new Dictionary<string, string> { { "foo", "bar" } };
                    using StringSequenceReplacer r = new StringSequenceReplacer(map);
                    Check.Contains("bar", r.Process("foo"));
                })
                .Build("Core: StringSequenceReplacer");
        }

        internal static TestSuiteDescriptor Splitter()
        {
            return new SuiteBuilder("Core.StringSplitter")
                .Case("Split.OnDefaultChars", "Text is split on the default separator characters", () =>
                {
                    using StringSplitter s = new StringSplitter();
                    List<string> tokens = s.Process("alpha beta,gamma;delta").ToList();
                    Check.Equal(4, tokens.Count);
                    Check.Contains("gamma", string.Join(",", tokens));
                })
                .Case("Empty.YieldsNothing", "Null or empty input yields nothing", () =>
                {
                    using StringSplitter s = new StringSplitter();
                    Check.Empty(s.Process(null!).ToList());
                    Check.Empty(s.Process("").ToList());
                })
                .Build("Core: StringSplitter");
        }

        internal static TestSuiteDescriptor Remover()
        {
            return new SuiteBuilder("Core.WordRemover")
                .Case("Remove.StopWords", "Stop words are removed while other words are preserved", () =>
                {
                    using WordRemover w = new WordRemover();
                    string result = w.Process("the quick brown fox");
                    Check.DoesNotContain("the", result);
                    Check.Contains("quick", result);
                    Check.Contains("fox", result);
                })
                .Case("Empty.ReturnsEmpty", "Null or whitespace input returns an empty string", () =>
                {
                    using WordRemover w = new WordRemover();
                    Check.Equal(string.Empty, w.Process((string)null!));
                })
                .Case("Array.Null", "Processing a null token array yields an empty array", () =>
                {
                    using WordRemover w = new WordRemover();
                    Check.Empty(w.Process((string[])null!));
                })
                .Build("Core: WordRemover");
        }

        internal static TestSuiteDescriptor Extractor()
        {
            return new SuiteBuilder("Core.TokenExtractor")
                .Case("Defaults", "Default minimum and maximum token lengths", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Equal(3, t.MinimumTokenLength);
                    Check.Equal(64, t.MaximumTokenLength);
                })
                .Case("Process.RespectsLength", "Extracted tokens honor the configured length window", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    List<string> tokens = t.Process("The quick brown foxes jumped over the lazy dogs").ToList();
                    Check.NotEmpty(tokens);
                    foreach (string token in tokens)
                        Check.True(token.Length >= t.MinimumTokenLength && token.Length <= t.MaximumTokenLength);
                })
                .Case("Process.EmptyYieldsNothing", "Null input yields no tokens", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Empty(t.Process(null!).ToList());
                })
                .Case("Validation.MinimumTokenLength", "MinimumTokenLength rejects values below one", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Throws<ArgumentOutOfRangeException>(() => t.MinimumTokenLength = 0);
                })
                .Case("Validation.MaximumTokenLength", "MaximumTokenLength rejects values below one", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Throws<ArgumentOutOfRangeException>(() => t.MaximumTokenLength = 0);
                })
                .Case("Validation.NullSplitter", "StringSplitter rejects null", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Throws<ArgumentNullException>(() => t.StringSplitter = null!);
                })
                .Case("Validation.MaxLessThanMin", "Process throws when maximum is less than minimum", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    t.MinimumTokenLength = 10;
                    t.MaximumTokenLength = 5;
                    Check.Throws<ArgumentException>(() => t.Process("some sample text here").ToList());
                })
                .Case("Chunk.Validation.MaxTokenCount", "Chunk throws for a maximum token count below one", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Throws<ArgumentOutOfRangeException>(() => t.Chunk("sample text", 0, 64).ToList());
                })
                .Case("Chunk.Validation.MaxChunkLength", "Chunk throws for a maximum chunk length below 32", () =>
                {
                    using TokenExtractor t = new TokenExtractor();
                    Check.Throws<ArgumentOutOfRangeException>(() => t.Chunk("sample text", 10, 16).ToList());
                })
                .Build("Core: TokenExtractor");
        }
    }
}
