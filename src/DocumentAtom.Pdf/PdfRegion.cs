namespace DocumentAtom.Pdf
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// PDF region.
    /// </summary>
    public class PdfRegion
    {
        /// <summary>
        /// Bounding box.
        /// </summary>
        public PdfRectangle BoundingBox { get; set; } 

        /// <summary>
        /// Type.
        /// </summary>
        public PdfRegionTypeEnum Type { get; set; } = PdfRegionTypeEnum.Text;

        /// <summary>
        /// Boolean indicating if the region was processed.
        /// </summary>
        public bool Processed { get; set; } = false;

        /// <summary>
        /// PDF region.
        /// </summary>
        public PdfRegion()
        {

        }
    }

    /// <summary>
    /// PDF table region.
    /// </summary>
    public class PdfTableRegion : PdfRegion
    {
        /// <summary>
        /// Table..
        /// </summary>
        public DataTable Table { get; set; } = null;

        /// <summary>
        /// PDF table region.
        /// </summary>
        public PdfTableRegion()
        {
            Type = PdfRegionTypeEnum.Table;
        }
    }

    /// <summary>
    /// PDF list region.
    /// </summary>
    public class PdfListRegion : PdfRegion
    {
        /// <summary>
        /// Items.
        /// </summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// Boolean indicating if the list is ordered.
        /// </summary>
        public bool IsOrdered { get; set; } = false;

        /// <summary>
        /// PDF list region.
        /// </summary>
        public PdfListRegion()
        {
            Type = PdfRegionTypeEnum.List;
        }
    }

    /// <summary>
    /// Section header.
    /// </summary>
    public class SectionHeader : PdfRegion
    {
        /// <summary>
        /// Text.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// Section header.
        /// </summary>
        public SectionHeader()
        {
            Type = PdfRegionTypeEnum.Header;
        }
    }

    /// <summary>
    /// PDF region type.
    /// </summary>
    public enum PdfRegionTypeEnum
    {
        /// <summary>
        /// HEader.
        /// </summary>
        Header,
        /// <summary>
        /// Table.
        /// </summary>
        Table,
        /// <summary>
        /// List.
        /// </summary>
        List,
        /// <summary>
        /// Text.
        /// </summary>
        Text
    }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
}
