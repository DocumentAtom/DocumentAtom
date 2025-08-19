namespace DocumentAtom.Html
{
#pragma warning disable CS8625

    using System;
    using System.Collections.Generic;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using SerializableDataTables;

    /// <summary>
    /// HTML atom.
    /// </summary>
    public class HtmlAtom : Atom
    {
        #region Public-Members

        /// <summary>
        /// HTML tag name.
        /// </summary>
        public string Tag { get; set; } = null;

        /// <summary>
        /// HTML element ID attribute.
        /// </summary>
        public string Id { get; set; } = null;

        /// <summary>
        /// HTML element class attribute.
        /// </summary>
        public string Class { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new HTML atom.
        /// </summary>
        public HtmlAtom() : base()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a string representation of the HTML atom.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string ret = base.ToString();

            if (!String.IsNullOrEmpty(Tag))
            {
                ret += " Tag: " + Tag;
            }

            if (!String.IsNullOrEmpty(Id))
            {
                ret += " Id: " + Id;
            }

            if (!String.IsNullOrEmpty(Class))
            {
                ret += " Class: " + Class;
            }

            if (HeaderLevel != null)
            {
                ret += " H" + HeaderLevel.Value;
            }

            if (!String.IsNullOrEmpty(Text))
            {
                string truncated = Text.Length > 50 ? Text.Substring(0, 50) + "..." : Text;
                ret += " Text: " + truncated;
            }

            if (UnorderedList != null && UnorderedList.Count > 0)
            {
                ret += " UnorderedList: " + UnorderedList.Count + " items";
            }

            if (OrderedList != null && OrderedList.Count > 0)
            {
                ret += " OrderedList: " + OrderedList.Count + " items";
            }

            if (Table != null)
            {
                ret += " Table: " + Rows + "x" + Columns;
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Image atom.
    /// </summary>
    public class ImageAtom : Atom
    {
        #region Public-Members

        /// <summary>
        /// Image source URL or path.
        /// </summary>
        public string Src { get; set; } = null;

        /// <summary>
        /// Alternative text for the image.
        /// </summary>
        public string Alt { get; set; } = null;

        /// <summary>
        /// Width attribute of the image.
        /// </summary>
        public string Width { get; set; } = null;

        /// <summary>
        /// Height attribute of the image.
        /// </summary>
        public string Height { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new image atom.
        /// </summary>
        public ImageAtom() : base()
        {
            Type = AtomTypeEnum.Image;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a string representation of the image atom.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string ret = base.ToString();

            if (!String.IsNullOrEmpty(Src))
            {
                ret += " Src: " + Src;
            }

            if (!String.IsNullOrEmpty(Alt))
            {
                ret += " Alt: " + Alt;
            }

            if (!String.IsNullOrEmpty(Width) && !String.IsNullOrEmpty(Height))
            {
                ret += " Size: " + Width + "x" + Height;
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Hyperlink atom.
    /// </summary>
    public class HyperlinkAtom : Atom
    {
        #region Public-Members

        /// <summary>
        /// Hyperlink URL.
        /// </summary>
        public string Href { get; set; } = null;

        /// <summary>
        /// Target attribute of the link.
        /// </summary>
        public string Target { get; set; } = null;

        /// <summary>
        /// Rel attribute of the link.
        /// </summary>
        public string Rel { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new hyperlink atom.
        /// </summary>
        public HyperlinkAtom() : base()
        {
            Type = AtomTypeEnum.Hyperlink;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a string representation of the hyperlink atom.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string ret = base.ToString();

            if (!String.IsNullOrEmpty(Text))
            {
                ret += " Text: " + Text;
            }

            if (!String.IsNullOrEmpty(Href))
            {
                ret += " Href: " + Href;
            }

            if (!String.IsNullOrEmpty(Target))
            {
                ret += " Target: " + Target;
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Code atom for code blocks and inline code.
    /// </summary>
    public class CodeAtom : Atom
    {
        #region Public-Members

        /// <summary>
        /// Code content.
        /// </summary>
        public string Code { get; set; } = null;

        /// <summary>
        /// Programming language if specified.
        /// </summary>
        public string Language { get; set; } = null;

        /// <summary>
        /// Indicates if this is inline code vs a code block.
        /// </summary>
        public bool IsInline { get; set; } = false;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new code atom.
        /// </summary>
        public CodeAtom() : base()
        {
            Type = AtomTypeEnum.Code;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a string representation of the code atom.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string ret = base.ToString();

            if (!String.IsNullOrEmpty(Language))
            {
                ret += " Language: " + Language;
            }

            if (IsInline)
            {
                ret += " (inline)";
            }

            if (!String.IsNullOrEmpty(Code))
            {
                string truncated = Code.Length > 50 ? Code.Substring(0, 50) + "..." : Code;
                ret += " Code: " + truncated;
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }

    /// <summary>
    /// Meta atom for meta tags and document metadata.
    /// </summary>
    public class MetaAtom : Atom
    {
        #region Public-Members

        /// <summary>
        /// Meta tag name attribute.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Meta tag content attribute.
        /// </summary>
        public string Content { get; set; } = null;

        /// <summary>
        /// Meta tag property attribute (for Open Graph tags).
        /// </summary>
        public string Property { get; set; } = null;

        /// <summary>
        /// Meta tag http-equiv attribute.
        /// </summary>
        public string HttpEquiv { get; set; } = null;

        /// <summary>
        /// Meta tag charset attribute.
        /// </summary>
        public string Charset { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new meta atom.
        /// </summary>
        public MetaAtom() : base()
        {
            Type = AtomTypeEnum.Meta;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a string representation of the meta atom.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string ret = base.ToString();

            if (!String.IsNullOrEmpty(Name))
            {
                ret += " Name: " + Name;
            }

            if (!String.IsNullOrEmpty(Property))
            {
                ret += " Property: " + Property;
            }

            if (!String.IsNullOrEmpty(Content))
            {
                string truncated = Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content;
                ret += " Content: " + truncated;
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }

#pragma warning restore CS8625
}