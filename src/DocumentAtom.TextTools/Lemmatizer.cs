namespace DocumentAtom.TextTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Reduces words to their base or dictionary form by removing inflectional endings and returning the base or dictionary form of a word.
    /// </summary>
    public class Lemmatizer : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Irregular verbs.
        /// </summary>
        public Dictionary<string, string> IrregularVerbs
        {
            get => _IrregularVerbs;
            set => _IrregularVerbs = value ?? new Dictionary<string, string>(_DefaultIrregularVerbs);
        }

        /// <summary>
        /// Irregular nouns.
        /// </summary>
        public Dictionary<string, string> IrregularNouns
        {
            get => _IrregularNouns;
            set => _IrregularNouns = value ?? new Dictionary<string, string>(_DefaultIrregularNouns);
        }

        /// <summary>
        /// Dictionary for morphological transformations (comparative/superlative adjectives, adverbs).
        /// </summary>
        public Dictionary<string, string> MorphologicalForms
        {
            get => _MorphologicalForms;
            set => _MorphologicalForms = value ?? new Dictionary<string, string>(_DefaultMorphologicalForms);
        }

        /// <summary>
        /// Terms that should be preserved in their original form.
        /// </summary>
        public string[] PreservedTerms
        {
            get => _PreservedTerms;
            set => _PreservedTerms = value ?? _DefaultPreservedTerms;
        }

        /// <summary>
        /// Common prefixes used in word formation.
        /// </summary>
        public string[] CommonPrefixes
        {
            get => _CommonPrefixes;
            set => _CommonPrefixes = value ?? _DefaultCommonPrefixes;
        }

        /// <summary>
        /// Common suffixes used in word formation.
        /// </summary>
        public string[] CommonSuffixes
        {
            get => _CommonSuffixes;
            set => _CommonSuffixes = value ?? _DefaultCommonSuffixes;
        }

        /// <summary>
        /// Words with preserved endings (e.g., -ment, -ant, -ent).
        /// </summary>
        public Dictionary<string, string> PreservedEndingForms
        {
            get => _PreservedEndingForms;
            set => _PreservedEndingForms = value ?? new Dictionary<string, string>(_DefaultPreservedEndingForms);
        }

        #endregion

        #region Private-Members

        private readonly string[] _DefaultPreservedTerms = new[]
        {
            "computing", "computer", "computation", "compute",
            "engineering", "processing", "scheduling", "training",
            "programming", "networking", "operating", "processing",
            "sophisticated", "accelerating"
        };

        private readonly string[] _DefaultCommonPrefixes = new[]
        {
            "anti", "auto", "dis", "inter", "intra", "macro", "mega", "micro",
            "mini", "multi", "non", "over", "post", "pre", "re", "semi",
            "ultra", "un", "under"
        };

        private readonly string[] _DefaultCommonSuffixes = new[]
        {
            "ability", "able", "age", "al", "ance", "ant", "ary", "ation",
            "dom", "ed", "ence", "ent", "ful", "ibility", "ible", "ic",
            "ical", "ify", "ing", "ise", "ity", "ive", "ization", "ize",
            "less", "ly", "ment", "ness", "ogy", "ous", "ship", "ty"
        };

        private readonly Dictionary<string, string> _DefaultPreservedEndingForms = new Dictionary<string, string>
        {
            {"accelerating", "accelerate"},
            {"analyzer", "analyze"},
            {"architecture", "architecture"},
            {"archive", "archive"},
            {"archives", "archive"},
            {"archived", "archive"},
            {"archiving", "archive"},
            {"basic", "basic"},
            {"capability", "capable"},
            {"complexity", "complex"},
            {"computer", "compute"},
            {"computing", "computing"},
            {"contemporary", "contemporary"},
            {"continuous", "continuous"},
            {"controller", "control"},
            {"coverage", "coverage"},
            {"crucial", "crucial"},
            {"cryptographic", "cryptographic"},
            {"density", "density"},
            {"dependent", "dependent"},
            {"development", "develop"},
            {"different", "different"},
            {"directory", "directory"},
            {"duplicate", "duplicate"},
            {"dynamic", "dynamic"},
            {"efficiency", "efficient"},
            {"efficient", "efficient"},
            {"electronic", "electronic"},
            {"environment", "environ"},
            {"facilitate", "facilitate"},
            {"facility", "facility"},
            {"facilities", "facility"},
            {"factory", "factory"},
            {"flexible", "flexible"},
            {"functional", "functional"},
            {"generate", "generate"},
            {"heterogeneous", "heterogeneous"},
            {"implement", "implement"},
            {"infrastructure", "infrastructure"},
            {"integrate", "integrate"},
            {"investment", "invest"},
            {"latency", "latency"},
            {"latent", "latent"},
            {"management", "manage"},
            {"manufacture", "manufacture"},
            {"maximize", "maximize"},
            {"memory", "memory"},
            {"minimize", "minimize"},
            {"operate", "operate"},
            {"operational", "operational"},
            {"optimize", "optimize"},
            {"processing", "processing"},
            {"processor", "process"},
            {"public", "public"},
            {"redundant", "redundant"},
            {"register", "register"},
            {"reliability", "reliable"},
            {"repository", "repository"},
            {"scalable", "scalable"},
            {"security", "secure"},
            {"significant", "significant"},
            {"simultaneous", "simultaneous"},
            {"sophisticated", "sophisticated"},
            {"specific", "specific"},
            {"specialize", "specialize"},
            {"speed", "speed"},
            {"stability", "stable"},
            {"storage", "storage"},
            {"technical", "technical"},
            {"technique", "technique"},
            {"transfer", "transfer"},
            {"unique", "unique"},
            {"usage", "usage"},
            {"utilize", "utilize"},
            {"various", "various"},
            {"voltage", "voltage"}
        };

        private readonly Dictionary<string, string> _DefaultIrregularVerbs = new Dictionary<string, string>
        {
            {"am", "be"}, {"are", "be"}, {"became", "become"}, {"becomes", "become"},
            {"began", "begin"}, {"begins", "begin"}, {"begun", "begin"}, {"brings", "bring"},
            {"brought", "bring"}, {"came", "come"}, {"comes", "come"}, {"did", "do"},
            {"does", "do"}, {"done", "do"}, {"draws", "draw"}, {"drew", "draw"},
            {"drawn", "draw"}, {"feels", "feel"}, {"felt", "feel"}, {"finds", "find"},
            {"found", "find"}, {"gave", "give"}, {"gets", "get"}, {"gives", "give"},
            {"given", "give"}, {"goes", "go"}, {"gone", "go"}, {"got", "get"},
            {"gotten", "get"}, {"had", "have"}, {"has", "have"}, {"have", "have"},
            {"is", "be"}, {"keeps", "keep"}, {"kept", "keep"}, {"knew", "know"},
            {"known", "know"}, {"knows", "know"}, {"leaves", "leave"}, {"left", "leave"},
            {"made", "make"}, {"makes", "make"}, {"saw", "see"}, {"seen", "see"},
            {"sees", "see"}, {"shows", "show"}, {"showed", "show"}, {"shown", "show"},
            {"speaks", "speak"}, {"spoke", "speak"}, {"spoken", "speak"}, {"takes", "take"},
            {"taken", "take"}, {"tells", "tell"}, {"thinks", "think"}, {"thought", "think"},
            {"told", "tell"}, {"took", "take"}, {"was", "be"}, {"went", "go"},
            {"were", "be"}, {"writes", "write"}, {"wrote", "write"}, {"written", "write"}
        };

        private readonly Dictionary<string, string> _DefaultIrregularNouns = new Dictionary<string, string>
        {
            {"alumni", "alumnus"}, {"analyses", "analysis"}, {"cacti", "cactus"},
            {"children", "child"}, {"criteria", "criterion"}, {"crises", "crisis"},
            {"data", "datum"}, {"deer", "deer"}, {"diagnoses", "diagnosis"},
            {"echoes", "echo"}, {"elves", "elf"}, {"feet", "foot"},
            {"fish", "fish"}, {"fungi", "fungus"}, {"geese", "goose"},
            {"halves", "half"}, {"heroes", "hero"}, {"lives", "life"},
            {"loaves", "loaf"}, {"men", "man"}, {"mice", "mouse"},
            {"nuclei", "nucleus"}, {"oases", "oasis"}, {"people", "person"},
            {"phenomena", "phenomenon"}, {"potatoes", "potato"}, {"series", "series"},
            {"sheep", "sheep"}, {"species", "species"}, {"syllabi", "syllabus"},
            {"teeth", "tooth"}, {"theses", "thesis"}, {"thieves", "thief"},
            {"tomatoes", "tomato"}, {"wives", "wife"}, {"wolves", "wolf"},
            {"women", "woman"}
        };

        private readonly Dictionary<string, string> _DefaultMorphologicalForms = new Dictionary<string, string>
        {
            {"actually", "actual"}, {"architectures", "architecture"}, {"best", "good"},
            {"better", "good"}, {"completely", "complete"}, {"components", "component"},
            {"computational", "computation"}, {"computers", "computer"}, {"computing", "computing"},
            {"computations", "computation"}, {"fewer", "few"}, {"fewest", "few"},
            {"finally", "final"}, {"fully", "full"}, {"further", "far"},
            {"furthest", "far"}, {"generally", "general"}, {"historically", "historical"},
            {"includes", "include"}, {"included", "include"}, {"including", "include"},
            {"later", "late"}, {"latest", "late"}, {"likely", "like"},
            {"more", "many"}, {"most", "many"}, {"naturally", "natural"},
            {"possibly", "possible"}, {"probably", "probable"}, {"really", "real"},
            {"usually", "usual"}, {"worse", "bad"}, {"worst", "bad"}
        };

        private Dictionary<string, string> _IrregularVerbs;
        private Dictionary<string, string> _IrregularNouns;
        private Dictionary<string, string> _MorphologicalForms;
        private string[] _PreservedTerms;
        private string[] _CommonPrefixes;
        private string[] _CommonSuffixes;
        private Dictionary<string, string> _PreservedEndingForms;

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates a new lemmatizer.
        /// </summary>
        public Lemmatizer()
        {
            // Initialize with default values
            IrregularVerbs = null;
            IrregularNouns = null;
            MorphologicalForms = null;
            PreservedTerms = null;
            CommonPrefixes = null;
            CommonSuffixes = null;
            PreservedEndingForms = null;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Process a single word.
        /// </summary>
        /// <param name="word">Word to process.</param>
        /// <returns>Lemmatized word.</returns>
        public string Process(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;

            word = word.ToLower().Trim();

            // Check preserved terms
            if (_PreservedTerms.Contains(word))
                return word;

            // Check preserved endings
            if (_PreservedEndingForms.ContainsKey(word))
                return _PreservedEndingForms[word];

            // Check irregular forms
            if (_IrregularVerbs.ContainsKey(word))
                return _IrregularVerbs[word];
            if (_IrregularNouns.ContainsKey(word))
                return _IrregularNouns[word];
            if (_MorphologicalForms.ContainsKey(word))
                return _MorphologicalForms[word];

            // Handle derived forms with prefixes/suffixes
            string derivedForm = ProcessDerivedForm(word);
            if (derivedForm != word)
                return derivedForm;

            // Apply rules in order of specificity
            string lemma = word;

            // Plural nouns
            if (lemma.EndsWith("ies"))
                lemma = Regex.Replace(lemma, "ies$", "y");
            else if (lemma.EndsWith("es"))
                lemma = Regex.Replace(lemma, "es$", "");
            else if (lemma.EndsWith("s") && !lemma.EndsWith("ss") && !lemma.EndsWith("us"))
                lemma = Regex.Replace(lemma, "s$", "");
            else if (lemma.EndsWith("ated"))
                lemma = Regex.Replace(lemma, "d$", "");

            // Verb forms
            if (lemma.EndsWith("ing"))
            {
                // Double consonant + ing
                if (Regex.IsMatch(lemma, "[aeiou][^aeiou]ing$"))
                {
                    string stem = Regex.Replace(lemma, "ing$", "");
                    if (stem.Length > 2) // Prevent stems that are too short
                        lemma = stem;
                }
                else
                    lemma = Regex.Replace(lemma, "ing$", "e");
            }
            else if (lemma.EndsWith("ed"))
            {
                // Double consonant + ed
                if (Regex.IsMatch(lemma, "[aeiou][^aeiou]ed$"))
                {
                    string stem = Regex.Replace(lemma, "ed$", "");
                    if (stem.Length > 2) // Prevent stems that are too short
                        lemma = stem;
                }
                else
                    lemma = Regex.Replace(lemma, "ed$", "e");
            }

            return lemma;
        }

        /// <summary>
        /// Process multiple words.
        /// </summary>
        /// <param name="words">Words to process.</param>
        /// <returns>Lemmatized words.</returns>
        public string[] Process(string[] words)
        {
            if (words == null)
                return Array.Empty<string>();

            string[] lemmas = new string[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                lemmas[i] = Process(words[i]);
            }
            return lemmas;
        }

        #endregion

        #region Private-Methods

        private string ProcessDerivedForm(string word)
        {
            if (_PreservedTerms.Contains(word))
                return word;

            // Check for common prefixes
            foreach (string prefix in _CommonPrefixes)
            {
                if (word.StartsWith(prefix))
                {
                    string withoutPrefix = word.Substring(prefix.Length);
                    string lemmatized = Process(withoutPrefix);
                    if (lemmatized != withoutPrefix)
                        return prefix + lemmatized;
                }
            }

            // Check for common suffixes
            foreach (string suffix in _CommonSuffixes)
            {
                if (word.EndsWith(suffix))
                {
                    string withoutSuffix = word.Substring(0, word.Length - suffix.Length);
                    if (withoutSuffix.Length > 2) // Prevent over-stemming
                        return withoutSuffix;
                }
            }

            return word;
        }

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}