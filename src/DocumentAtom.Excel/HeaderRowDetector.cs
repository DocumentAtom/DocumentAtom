namespace DocumentAtom.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Excel;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Improved header row detector with a more modular pattern evaluation system.
    /// </summary>
    public class HeaderRowDetector : IDisposable
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        #endregion

        #region Private-Members

        private XlsxProcessorSettings _Settings = null;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Header row detector.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public HeaderRowDetector(XlsxProcessorSettings settings)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines if the first row of a sheet is a header row by analyzing patterns in the data.
        /// </summary>
        /// <param name="rows">List of rows from the sheet</param>
        /// <param name="sharedStringTable">The shared string table for the workbook</param>
        /// <param name="maxRowsToAnalyze">Maximum number of rows to analyze (default 10)</param>
        /// <returns>True if the first row is likely a header row, false otherwise</returns>
        public bool IsHeaderRow(List<Row> rows, SharedStringTable sharedStringTable, int maxRowsToAnalyze = 10)
        {
            // Early validation
            if (rows == null || rows.Count <= 1)
                return false; // Need at least two rows to compare

            // Extract and prepare data
            var rowData = PrepareRowData(rows, sharedStringTable, maxRowsToAnalyze);
            if (rowData == null || rowData.Count <= 1)
                return false;

            // Check if first column is just sequential numbers matching row position
            bool isSimpleRowNumbering = IsSimpleRowNumbering(rowData);

            // If we have strong evidence of simple row numbering, and enough rows to be confident
            if (isSimpleRowNumbering && rowData.Count >= 5)
            {
                LogPatternScores(new Dictionary<string, double>
                {
                    { "SimpleRowNumbering", -5.0 }
                }, -5.0);

                return false;
            }

            // Apply the pattern detectors and calculate the score
            var patternScores = EvaluatePatterns(rowData);

            // Calculate final score
            double totalScore = patternScores.Sum(s => s.Value);

            // Log the results if needed
            LogPatternScores(patternScores, totalScore);

            return totalScore >= _Settings.HeaderRowScoreThreshold;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    // No managed resources to dispose
                }

                _Disposed = true;
            }
        }

        private List<Dictionary<int, string>> PrepareRowData(List<Row> rows, SharedStringTable sharedStringTable, int maxRowsToAnalyze)
        {
            // Limit the number of rows to analyze
            var rowsToAnalyze = rows.Take(Math.Min(rows.Count, maxRowsToAnalyze + 1)).ToList();

            // Initialize result data structure
            var rowData = new List<Dictionary<int, string>>();
            int maxColumnIndex = 0;

            // Process each row
            foreach (var row in rowsToAnalyze)
            {
                var cellData = new Dictionary<int, string>();

                foreach (var cell in row.Elements<Cell>())
                {
                    string cellRef = cell.CellReference?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(cellRef)) continue;

                    string colRef = GetColumnReference(cellRef);
                    int colIdx = ColumnReferenceToIndex(colRef);

                    // Store cell value
                    cellData[colIdx] = GetCellValue(cell, sharedStringTable);

                    maxColumnIndex = Math.Max(maxColumnIndex, colIdx);
                }

                // Add row data to results
                rowData.Add(cellData);
            }

            // Ensure all rows have values for all column positions (empty string if needed)
            foreach (var row in rowData)
            {
                for (int i = 0; i <= maxColumnIndex; i++)
                {
                    if (!row.ContainsKey(i))
                    {
                        row[i] = string.Empty;
                    }
                }
            }

            return rowData;
        }

        private Dictionary<string, double> EvaluatePatterns(List<Dictionary<int, string>> rowData)
        {
            var scores = new Dictionary<string, double>();

            // Detect patterns and calculate individual scores
            scores["AllRowsSimilar"] = DetectAllRowsSimilar(rowData) ? _Settings.HeaderRowWeights.AllRowsSimilar : 0;
            scores["SequentialFirstColumn"] = DetectSequentialFirstColumn(rowData) ? _Settings.HeaderRowWeights.SequentialFirstColumn : 0;
            scores["HigherTextRatio"] = DetectHigherTextRatio(rowData) ? _Settings.HeaderRowWeights.HigherTextRatio : 0;
            scores["DistinctFormat"] = DetectDistinctFormat(rowData) ? _Settings.HeaderRowWeights.DistinctFormat : 0;
            scores["ConsistentDataColumns"] = DetectConsistentDataColumns(rowData) ? _Settings.HeaderRowWeights.ConsistentDataColumns : 0;
            scores["HeaderTerms"] = DetectHeaderTerms(rowData) ? _Settings.HeaderRowWeights.HeaderTerms : 0;

            // Column header patterns
            var columnHeadersResult = DetectColumnHeaderPatterns(rowData);
            if (columnHeadersResult.IsNumericSequence && !columnHeadersResult.IsColumnNumbers)
                scores["NumericSequence"] = _Settings.HeaderRowWeights.NumericSequence;
            if (columnHeadersResult.IsColumnNumbers)
                scores["ColumnNumbers"] = _Settings.HeaderRowWeights.ColumnNumbers;
            if (columnHeadersResult.HasColumnHeaders)
                scores["ColumnHeaders"] = _Settings.HeaderRowWeights.ColumnHeaders;

            // Add the new check for pure numeric headers
            if (IsPureNumericHeader(rowData))
            {
                scores["PureNumericHeader"] = 5.0; // Strong positive signal
                                                   // Remove any negative NumericSequence score if it exists
                scores.Remove("NumericSequence");
            }
            scores["Row1Differs"] = DetectRow1Differs(rowData) ? _Settings.HeaderRowWeights.Row1Differs : 0;

            return scores;
        }

        #region Pattern Detectors

        private bool DetectAllRowsSimilar(List<Dictionary<int, string>> rowData)
        {
            // Cell count pattern
            var nonEmptyCounts = rowData.Select(row =>
                row.Values.Count(val => !string.IsNullOrWhiteSpace(val))).ToList();

            double avgNonEmpty = nonEmptyCounts.Average();
            double stdDev = Math.Sqrt(nonEmptyCounts.Select(x => Math.Pow(x - avgNonEmpty, 2)).Average());
            double relativeStdDev = avgNonEmpty > 0 ? stdDev / avgNonEmpty : 0;

            bool allRowsHaveSimilarCellCount = relativeStdDev < 0.1; // Less than 10% variation

            // Length pattern similarity
            var lengthPatterns = rowData.Select(row =>
                row.Values.Select(value => value?.Length ?? 0).ToList()).ToList();

            bool allRowsHaveSimilarLengthPattern = true;
            var firstRowPattern = lengthPatterns[0];

            for (int i = 1; i < lengthPatterns.Count; i++)
            {
                double similarity = CalculatePatternSimilarity(firstRowPattern, lengthPatterns[i]);
                if (similarity < 0.8) // Less than 80% similarity
                {
                    allRowsHaveSimilarLengthPattern = false;
                    break;
                }
            }

            return allRowsHaveSimilarCellCount && allRowsHaveSimilarLengthPattern;
        }

        private bool DetectSequentialFirstColumn(List<Dictionary<int, string>> rowData)
        {
            if (rowData.Count <= 2) return false;

            // Check if second row starts with 1 (common pattern for header)
            if (rowData.Count > 1 &&
                int.TryParse(rowData[1][0], out int secondRowVal) &&
                secondRowVal == 1)
            {
                return true;
            }

            // Check for sequential values starting from 1 in data rows
            int expectedValue = 1;
            for (int rowIdx = 1; rowIdx < rowData.Count; rowIdx++)
            {
                if (!int.TryParse(rowData[rowIdx][0], out int actualValue) ||
                    actualValue != expectedValue)
                {
                    return false;
                }
                expectedValue++;
            }

            return true;
        }

        private bool DetectHigherTextRatio(List<Dictionary<int, string>> rowData)
        {
            int firstRowTextCount = 0;
            int firstRowNumericCount = 0;
            int dataRowsTextCount = 0;
            int dataRowsNumericCount = 0;

            // Analyze first row
            foreach (var value in rowData[0].Values)
            {
                if (IsNumeric(value))
                    firstRowNumericCount++;
                else if (!string.IsNullOrWhiteSpace(value))
                    firstRowTextCount++;
            }

            // Analyze data rows
            for (int rowIdx = 1; rowIdx < rowData.Count; rowIdx++)
            {
                foreach (var value in rowData[rowIdx].Values)
                {
                    if (IsNumeric(value))
                        dataRowsNumericCount++;
                    else if (!string.IsNullOrWhiteSpace(value))
                        dataRowsTextCount++;
                }
            }

            // Calculate proportions
            double firstRowTextRatio = firstRowTextCount + firstRowNumericCount > 0
                ? (double)firstRowTextCount / (firstRowTextCount + firstRowNumericCount)
                : 0;

            double dataRowsTextRatio = dataRowsTextCount + dataRowsNumericCount > 0
                ? (double)dataRowsTextCount / (dataRowsTextCount + dataRowsNumericCount)
                : 0;

            return firstRowTextRatio > dataRowsTextRatio && firstRowTextRatio > 0.5;
        }

        private bool DetectDistinctFormat(List<Dictionary<int, string>> rowData)
        {
            if (rowData.Count <= 1) return false;

            int firstRowSpecialFormatCount = 0;
            int secondRowSpecialFormatCount = 0;

            foreach (var colIdx in rowData[0].Keys)
            {
                if (rowData[1].ContainsKey(colIdx))
                {
                    string firstRowValue = rowData[0][colIdx];
                    string secondRowValue = rowData[1][colIdx];

                    if (IsTitleCase(firstRowValue) || IsAllCaps(firstRowValue))
                        firstRowSpecialFormatCount++;

                    if (IsTitleCase(secondRowValue) || IsAllCaps(secondRowValue))
                        secondRowSpecialFormatCount++;
                }
            }

            return firstRowSpecialFormatCount > secondRowSpecialFormatCount;
        }

        private bool DetectConsistentDataColumns(List<Dictionary<int, string>> rowData)
        {
            if (rowData.Count <= 2) return false;

            Dictionary<int, List<string>> columnDataTypes = new Dictionary<int, List<string>>();

            // Get max column index
            int maxColIdx = rowData.SelectMany(r => r.Keys).Max();

            // Build collection of data types by column (excluding first row)
            for (int colIdx = 0; colIdx <= maxColIdx; colIdx++)
            {
                columnDataTypes[colIdx] = new List<string>();

                // Start from row 1 (skipping potential header)
                for (int rowIdx = 1; rowIdx < rowData.Count; rowIdx++)
                {
                    if (rowData[rowIdx].TryGetValue(colIdx, out string value) &&
                        !string.IsNullOrWhiteSpace(value))
                    {
                        columnDataTypes[colIdx].Add(GetDataType(value));
                    }
                }
            }

            // Count columns with consistent data types
            int consistentColumns = 0;
            foreach (var column in columnDataTypes)
            {
                if (column.Value.Count > 0 && column.Value.Distinct().Count() == 1)
                {
                    consistentColumns++;
                }
            }

            // Check if first row has significantly different data types than the consistent columns
            int firstRowDifferentTypes = 0;
            foreach (var kvp in columnDataTypes)
            {
                int colIdx = kvp.Key;
                var types = kvp.Value;

                if (types.Count > 0 && types.Distinct().Count() == 1 &&
                    rowData[0].TryGetValue(colIdx, out string firstRowValue) &&
                    !string.IsNullOrWhiteSpace(firstRowValue))
                {
                    string firstRowType = GetDataType(firstRowValue);
                    string dataRowType = types[0];

                    if (firstRowType != dataRowType)
                    {
                        firstRowDifferentTypes++;
                    }
                }
            }

            // We need both consistency in data rows AND difference in the first row
            return consistentColumns > 0 &&
                (double)consistentColumns / Math.Max(1, columnDataTypes.Count) > 0.5 &&
                firstRowDifferentTypes > 0; // At least one column must have different types
        }

        private bool DetectHeaderTerms(List<Dictionary<int, string>> rowData)
        {
            int headerTermMatches = 0;
            int exactHeaderTerms = 0;

            foreach (string value in rowData[0].Values)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                string lowerValue = value.ToLower();

                // Check for exact term matches
                if (_Settings.CommonHeaderRowTerms.Any(term => lowerValue == term))
                {
                    exactHeaderTerms++;
                }
                // Check for partial term matches
                else if (_Settings.CommonHeaderRowTerms.Any(term => lowerValue.Contains(term)))
                {
                    headerTermMatches++;
                }
            }

            // Require stronger evidence - either multiple partial matches or at least one exact match
            return exactHeaderTerms > 0 || headerTermMatches >= 2;
        }

        private class ColumnHeaderResult
        {
            public bool IsNumericSequence { get; set; }
            public bool IsColumnNumbers { get; set; }
            public bool HasColumnHeaders { get; set; }
        }

        private ColumnHeaderResult DetectColumnHeaderPatterns(List<Dictionary<int, string>> rowData)
        {
            var result = new ColumnHeaderResult();

            if (rowData[0].Count < 2)
                return result;

            DetectNumericSequencePatterns(rowData, result);
            result.HasColumnHeaders = CheckForNamedColumnHeaders(rowData[0].Values);

            return result;
        }

        private void DetectNumericSequencePatterns(List<Dictionary<int, string>> rowData, ColumnHeaderResult result)
        {
            List<int> numericValues = new List<int>();
            foreach (string value in rowData[0].Values)
            {
                if (int.TryParse(value, out int numValue))
                {
                    numericValues.Add(numValue);
                }
            }

            if (numericValues.Count >= 2)
            {
                bool isSequential = true;
                for (int i = 1; i < numericValues.Count; i++)
                {
                    if (numericValues[i] != numericValues[i - 1] + 1)
                    {
                        isSequential = false;
                        break;
                    }
                }

                // In the DetectNumericSequencePatterns method, modify this section
                if (isSequential && numericValues.Count >= rowData[0].Count / 2)
                {
                    result.IsNumericSequence = true;

                    // Strengthen the column numbers detection
                    // If the sequence starts with 1 and is sequential, it's likely column headers
                    if (numericValues.Count > 0 && numericValues[0] == 1)
                    {
                        // Check if the numbers match their column position approximately
                        bool matchesColumnPosition = true;
                        for (int i = 0; i < Math.Min(numericValues.Count, 5); i++) // Check first few columns
                        {
                            // Allow for some columns to be skipped or merged
                            if (Math.Abs(numericValues[i] - (i + 1)) > 2)
                            {
                                matchesColumnPosition = false;
                                break;
                            }
                        }

                        // If it matches column positions OR covers most columns, consider it column numbers
                        if (matchesColumnPosition || numericValues.Count >= rowData[0].Count * 0.75)
                        {
                            result.IsColumnNumbers = true;
                            result.IsNumericSequence = false; // Override the negative pattern
                        }
                    }
                }
            }
        }

        private bool CheckForNamedColumnHeaders(IEnumerable<string> values)
        {
            Dictionary<string, List<int>> prefixGroups = new Dictionary<string, List<int>>();

            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                // Find where digits start in the string
                int digitIndex = -1;
                for (int i = 0; i < value.Length; i++)
                {
                    if (char.IsDigit(value[i]))
                    {
                        digitIndex = i;
                        break;
                    }
                }

                if (digitIndex > 0 && digitIndex < value.Length)
                {
                    string prefix = value.Substring(0, digitIndex);
                    string numberPart = value.Substring(digitIndex);

                    if (int.TryParse(numberPart, out int columnNumber))
                    {
                        if (!prefixGroups.ContainsKey(prefix))
                        {
                            prefixGroups[prefix] = new List<int>();
                        }
                        prefixGroups[prefix].Add(columnNumber);
                    }
                }
            }

            // Check if any prefix group has sequential numbers
            foreach (var group in prefixGroups)
            {
                if (group.Value.Count >= 2)
                {
                    var sortedNumbers = group.Value.OrderBy(n => n).ToList();

                    bool isSequential = true;
                    for (int i = 1; i < sortedNumbers.Count; i++)
                    {
                        if (sortedNumbers[i] != sortedNumbers[i - 1] + 1)
                        {
                            isSequential = false;
                            break;
                        }
                    }

                    if (isSequential)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool DetectRow1Differs(List<Dictionary<int, string>> rowData)
        {
            if (rowData.Count < 3) return false;

            Dictionary<int, HashSet<string>> valuesPerColumn = new Dictionary<int, HashSet<string>>();

            // Get max column index
            int maxColIdx = rowData.SelectMany(r => r.Keys).Max();

            // Initialize value sets per column
            for (int colIdx = 0; colIdx <= maxColIdx; colIdx++)
            {
                valuesPerColumn[colIdx] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // Collect unique values from rows 2+ (excluding nulls)
            for (int rowIdx = 1; rowIdx < rowData.Count; rowIdx++)
            {
                foreach (var kvp in rowData[rowIdx])
                {
                    int colIdx = kvp.Key;
                    string value = kvp.Value;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        valuesPerColumn[colIdx].Add(value);
                    }
                }
            }

            // Count columns where row 1 differs from consistent data rows
            int consistentDataColumnsCount = 0;
            int row1DifferentCount = 0;

            foreach (var colIdx in rowData[0].Keys)
            {
                string row1Value = rowData[0][colIdx];

                // If column has consistent values in rows 2+
                if (valuesPerColumn.ContainsKey(colIdx) && valuesPerColumn[colIdx].Count <= 1)
                {
                    consistentDataColumnsCount++;

                    // And row 1 differs from those values
                    if (!string.IsNullOrWhiteSpace(row1Value) &&
                        (valuesPerColumn[colIdx].Count == 0 ||
                         !valuesPerColumn[colIdx].Contains(row1Value)))
                    {
                        row1DifferentCount++;
                    }
                }
            }

            return consistentDataColumnsCount > 0 &&
                   row1DifferentCount > 0 &&
                   (double)row1DifferentCount / consistentDataColumnsCount >= 0.5;
        }

        private bool IsSimpleRowNumbering(List<Dictionary<int, string>> rowData)
        {
            // Need at least 3 rows to be confident
            if (rowData.Count < 3)
                return false;

            // Check if first column has sequential numbers exactly matching row position
            for (int i = 0; i < rowData.Count; i++)
            {
                // Check if the value exists and is numeric
                if (!rowData[i].TryGetValue(0, out string value) ||
                    !int.TryParse(value, out int numValue) ||
                    numValue != i + 1)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPureNumericHeader(List<Dictionary<int, string>> rowData)
        {
            // Get all values from first row that are numeric
            var firstRowValues = rowData[0].Values
                .Where(v => int.TryParse(v, out _))
                .Select(v => int.Parse(v))
                .OrderBy(v => v)
                .ToList();

            // If most of the first row is numeric and starts with a low number (1-3)
            if (firstRowValues.Count >= rowData[0].Count * 0.8 &&
                firstRowValues.Count > 0 &&
                firstRowValues[0] <= 3)
            {
                // Check if it's a relatively clean sequence
                bool isRelativelySequential = true;
                for (int i = 1; i < firstRowValues.Count; i++)
                {
                    // Allow for some non-sequential numbers but the difference shouldn't be too large
                    if (firstRowValues[i] - firstRowValues[i - 1] > 3)
                    {
                        isRelativelySequential = false;
                        break;
                    }
                }

                return isRelativelySequential;
            }

            return false;
        }

        #endregion

        #region Helper Methods

        private static string GetColumnReference(string cellReference)
        {
            return string.Concat(cellReference.TakeWhile(c => !char.IsDigit(c)));
        }

        private static int ColumnReferenceToIndex(string colRef)
        {
            int index = 0;
            foreach (char c in colRef)
            {
                index = index * 26 + (c - 'A' + 1);
            }
            return index - 1; // Zero-based
        }

        private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
        {
            if (cell.CellValue == null)
                return string.Empty;

            string value = cell.CellValue.Text;

            // If the cell represents a shared string
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString &&
                int.TryParse(value, out int index) &&
                index >= 0 && index < sharedStringTable.Count())
            {
                return sharedStringTable.ElementAt(index).InnerText;
            }

            return value;
        }

        private static double CalculatePatternSimilarity(List<int> pattern1, List<int> pattern2)
        {
            int minLength = Math.Min(pattern1.Count, pattern2.Count);
            if (minLength == 0) return 0;

            int matchCount = 0;

            for (int i = 0; i < minLength; i++)
            {
                // Consider similar if within 20% of each other
                if (pattern1[i] == 0 && pattern2[i] == 0)
                {
                    matchCount++;
                }
                else if (pattern1[i] > 0 && pattern2[i] > 0)
                {
                    double ratio = (double)Math.Max(pattern1[i], pattern2[i]) /
                                   Math.Max(1, Math.Min(pattern1[i], pattern2[i]));
                    if (ratio <= 1.2) // Within 20%
                    {
                        matchCount++;
                    }
                }
            }

            return (double)matchCount / minLength;
        }

        private static bool IsNumeric(string value)
        {
            return !string.IsNullOrEmpty(value) && double.TryParse(value, out _);
        }

        private static bool IsTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2) return false;

            string[] words = value.Split(' ');
            return words.Length > 0 && words.All(word =>
                !string.IsNullOrEmpty(word) &&
                char.IsUpper(word[0]) &&
                word.Length > 1 &&
                word.Skip(1).All(c => !char.IsUpper(c) || !char.IsLetter(c)));
        }

        private static bool IsAllCaps(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Length > 1 &&
                   value.Any(char.IsLetter) &&
                   value.All(c => !char.IsLetter(c) || char.IsUpper(c));
        }

        private static string GetDataType(string value)
        {
            if (string.IsNullOrEmpty(value)) return "empty";
            if (IsNumeric(value)) return "numeric";
            if (DateTime.TryParse(value, out _)) return "date";
            return "text";
        }

        private void LogPatternScores(Dictionary<string, double> scores, double totalScore)
        {
            Console.WriteLine($"Header detection scores:");
            foreach (var score in scores)
            {
                Console.WriteLine($"  {score.Key} = {score.Value}");
            }
            Console.WriteLine($"  Total Score = {totalScore}");
        }

        #endregion

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore IDE0044 // Add readonly modifier
    }
}