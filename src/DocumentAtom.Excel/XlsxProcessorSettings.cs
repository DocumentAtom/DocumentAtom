namespace DocumentAtom.Excel
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for Microsoft Excel .xlsx processor.
    /// </summary>
    public class XlsxProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Terms to search for when identifying header rows.
        /// </summary>
        public List<string> CommonHeaderRowTerms
        {
            get
            {
                return _CommonHeaderRowTerms;
            }
            set
            {
                if (value == null) value = new List<string>();
                _CommonHeaderRowTerms = value;
            }
        }

        /// <summary>
        /// Pattern weights for header row detection.
        /// </summary>
        public HeaderRowPatternWeights HeaderRowWeights
        {
            get
            {
                return _HeaderRowWeights;
            }
            set
            {
                if (value == null) value = new HeaderRowPatternWeights();
                _HeaderRowWeights = value;
            }
        }

        /// <summary>
        /// Header row score threshold.
        /// </summary>
        public int HeaderRowScoreThreshold
        {
            get
            {
                return _HeaderRowScoreThreshold;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(HeaderRowScoreThreshold));
                _HeaderRowScoreThreshold = value;
            }
        }

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on sheet (page) grouping.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        #endregion

        #region Private-Members

        private List<string> _CommonHeaderRowTerms = new List<string>
        { 
            // Common identifiers
            "id", "identifier", "key", "code", "ref", "reference", "number", "num", "no",
                
            // Name-related
            "name", "first", "last", "middle", "full", "prefix", "suffix", "title", "username",
            "firstname", "lastname", "fullname", "surname", "nickname",
                
            // Personal info
            "age", "gender", "sex", "dob", "birth", "birthday", "birthdate", "born",
                
            // Temporal terms
            "date", "time", "day", "month", "year", "quarter", "period", "duration", "term",
            "created", "updated", "modified", "start", "end", "begin", "expiry", "timestamp",
                
            // Location/address
            "address", "street", "city", "state", "country", "province", "zip", "zipcode",
            "postal", "location", "place", "region", "area", "territory", "continent",
                
            // Contact information
            "email", "phone", "fax", "mobile", "cell", "contact", "website", "url", "web",
            "homepage", "domain", "twitter", "facebook", "social",
                
            // Categorical
            "type", "category", "class", "classification", "group", "department", "division",
            "status", "state", "condition", "level", "tier", "grade", "rank", "rating",
                
            // Descriptions
            "description", "desc", "details", "info", "information", "note", "notes", "comment",
            "comments", "summary", "overview", "remark", "remarks", "about", "text", "content",
                
            // Numerical values
            "amount", "quantity", "count", "total", "sum", "value", "score", "size", "height",
            "width", "length", "weight", "volume", "area", "depth", "balance", "percent",
            "percentage", "ratio", "rate", "frequency",
                
            // Financial
            "price", "cost", "fee", "charge", "tax", "discount", "currency", "payment", "paid",
            "balance", "debit", "credit", "invoice", "salary", "wage", "budget", "expense",
            "revenue", "income", "profit", "loss", "margin", "interest", "commission",
                
            // Inventory/products
            "sku", "upc", "product", "item", "inventory", "stock", "model", "brand", "make",
            "unit", "part", "component", "material", "color", "version", "edition",
                
            // Organizational
            "company", "organization", "org", "business", "corp", "corporation", "enterprise",
            "agency", "institution", "department", "division", "branch", "team", "position",
            "role", "job", "title", "occupation", "employer", "employee", "customer", "client",
            "vendor", "supplier", "partner",
                
            // File metadata
            "file", "filename", "path", "extension", "size", "format", "type", "author",
            "creator", "owner", "publisher", "source",
                
            // Technical
            "ip", "mac", "port", "host", "domain", "server", "user", "login", "password",
            "hash", "encrypt", "ssl", "http", "url", "uri", "api", "token", "session", 
                
            // Status indicators
            "status", "state", "condition", "active", "inactive", "enabled", "disabled",
            "complete", "incomplete", "approved", "rejected", "pending", "processed",
                
            // Measurements
            "measurement", "measure", "unit", "metric", "dimension", "length", "width",
            "height", "weight", "volume", "distance", "speed", "velocity", "acceleration",
            "force", "energy", "power", "voltage", "current", "resistance", "temperature",
            "degree", "pressure", "flow", "capacity",
                
            // Statistical
            "average", "avg", "mean", "median", "mode", "max", "min", "maximum", "minimum",
            "sum", "total", "count", "frequency", "variance", "deviation", "std", "quartile",
            "percentile"
        };

        private HeaderRowPatternWeights _HeaderRowWeights = new HeaderRowPatternWeights();

        private int _HeaderRowScoreThreshold = 3;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for Microsoft Excel .xlsx processor.
        /// </summary>
        public XlsxProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
