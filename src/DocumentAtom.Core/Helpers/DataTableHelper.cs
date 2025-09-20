namespace DocumentAtom.Core.Helpers
{
    using System;
    using System.Data;

    /// <summary>
    /// DataTable helpers.
    /// </summary>
    public static class DataTableHelper
    {
        #region Public-Methods

        /// <summary>
        /// Calculate DataTable length.
        /// This calculation adds the length of column names and row values.
        /// </summary>
        /// <param name="dt">DataTable.</param>
        /// <returns>Length.</returns>
        public static int GetLength(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0 || dt.Rows.Count == 0) return 0;

            int len = 0;

            // Include column names if desired
            foreach (DataColumn col in dt.Columns)
            {
                len += col.ColumnName.Length;
            }

            // Sum lengths of all cell values
            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    if (item != null)
                    {
                        len += item.ToString().Length;
                    }
                }
            }

            return len;
        }

        #endregion
    }
}
