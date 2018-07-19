using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoBlock
{
    namespace TableDisplay
    {
        public class Table
        {
            public class Column
            {
                public class WidthOutOfRangeException : ArgumentOutOfRangeException
                {
                    public WidthOutOfRangeException(int givenWidth)
                        : base("width", givenWidth, formatExceptionMessage())
                    {

                    }

                    private static string formatExceptionMessage()
                    {
                        return "Column width must be greater than or equal to column header length";
                    }
                }

                public class WidhtsAndHeadersCountMismatchException : MismatchException
                {
                    public WidhtsAndHeadersCountMismatchException()
                        : base("widths.Length", "headers.Length")
                    {

                    }
                }

                private string header;
                private int width;

                public Column(string header, int width)
                {
                    assertValidWidth(width, header);

                    this.header = header;
                    this.width = width;
                }

                public Column(Column column) 
                    : this(column.header, column.width)
                {
                    
                }

                public string Header
                {
                    get { return header; }
                }

                public int Width
                {
                    get { return width; }
                }

                public static Column[] ParseArray(IList<string> headers, IList<int> widths)
                {
                    // assert headers.Length and widths.Length match
                    assertMatchingWidthsAndHeadersCount(headers, widths);

                    Column[] columns = new Column[headers.Count];

                    for (int i = 0; i < headers.Count; i++)
                    {
                        string currentHeader = headers[i];
                        int currentWidth = widths[i];

                        columns[i] = new Column(currentHeader, currentWidth);
                    }

                    return columns;
                }

                public override bool Equals(object obj)
                {
                    if(obj == null)
                    {
                        return false;
                    }

                    Column other = obj as Column;

                    return other != null 
                        && this.header == other.header 
                        && this.width == other.width;
                }

                public static bool operator ==(Column column1, Column column2)
                {
                    return column1.Equals(column2);
                }

                public static bool operator !=(Column column1, Column column2)
                {
                    return !(column1 == column2);
                }

                public override int GetHashCode()
                {
                    return CollectionUtils.GetHashCode(this);
                }

                public override string ToString()
                {
                    return header.PadRight(width);
                }

                private static void assertValidWidth(int width, string header)
                {
                    if (width < 0)
                    {
                        throw new ArgumentOutOfRangeException("width", width, "Width must be greater than zero.");
                    }
                }

                private static void assertMatchingWidthsAndHeadersCount(IList<string> headers, IList<int> widths)
                {
                    if(headers.Count != widths.Count)
                    {
                        throw new WidhtsAndHeadersCountMismatchException();
                    }
                }
            }

            public class Row
            {
                private readonly string[] columnValues;

                public Row(IEnumerable<string> columnValues)
                {
                    this.columnValues = CollectionUtils.ConvertToArray(columnValues);
                }

                public int ColumnCount
                {
                    get { return columnValues.Length; }
                }

                public string[] ColumnValues
                {
                    get { return columnValues; }
                }

                public string GetColumnValue(int columnIndex)
                {
                    return columnValues[columnIndex];
                }

                public override bool Equals(object obj)
                {
                    if(obj == null)
                    {
                        return false;
                    }

                    Row other = obj as Row;

                    return other != null
                        && Enumerable.SequenceEqual(this.columnValues, other.columnValues);
                }

                public static bool operator ==(Row row1, Row row2)
                {
                    return row1.Equals(row2);
                }

                public static bool operator !=(Row row1, Row row2)
                {
                    return !(row1 == row2);
                }

                public override int GetHashCode()
                {
                    return CollectionUtils.GetHashCode(this);
                }

                // if columnWidth is zero (less than actual width) padRight retuns the original string.
                public string ToString(IList<Column> columns)
                {
                    StringBuilder rowStringBuilder = new StringBuilder();

                    // append all column values to create the row string
                    for(int i = 0; i < columnValues.Length; i++)
                    {
                        // append the current column value padded according to column width
                        string columnValuePadded = columnValues[i].PadRight(columns[i].Width);
                        rowStringBuilder.Append(columnValuePadded);
                    }

                    return rowStringBuilder.ToString();
                }
            }

            public class TableException : Exception
            {
                public TableException(string exceptionMessage)
                    : base(exceptionMessage)
                {

                }
            }

            public class RowColumnCountMismatchException : Exception
            {
                public RowColumnCountMismatchException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Row must have the same column count as Table.";
                }
            }

            public class OperationRequiresEmptyTableException : Exception
            {
                private string operationName;

                public OperationRequiresEmptyTableException(string operationName)
                    : base(formatExceptionMessage(operationName))
                {
                    this.operationName = operationName;
                }

                public string OperationName
                {
                    get { return operationName; }
                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Table must be empty of rows before performing the following operation: {0}.",
                        operationName);
                }
            }

            private List<Column> columns = new List<Column>();
            private List<Row> rows = new List<Row>();

            public Table(IList<Column> tableColumns = null, IList<Row> tableRows = null)
            {
                if(tableColumns != null)
                {
                    AddColumnRange(tableColumns);
                }

                if(tableRows != null)
                {
                    AddRowRange(tableRows);
                }               
            }

            public int ColumnCount
            {
                get { return columns.Count; }
            }

            public int RowCount
            {
                get { return rows.Count; }
            }

            public bool EmptyOfRows
            {
                get { return rows.Count == 0; }
            }

            public Row GetRow(int rowIndex)
            {
                return rows[rowIndex];
            }

            public void AddRow(Row tableRow)
            {
                assertColumnCountMatchesTable(tableRow);

                rows.Add(tableRow);
            }

            public void AddRowRange<T>(IList<T> rows) where T : Row
            {
                foreach(Row row in rows)
                {
                    assertColumnCountMatchesTable(row);
                }

                this.rows.AddRange(rows);
            }

            // false if remove was unsuccessful / item not found in row list
            public bool RemoveRow(Row row)
            {
                return rows.Remove(row);
            }

            public bool[] RemoveRowRange<T>(IList<T> rows) where T : Row
            {
                bool[] rowsRemoved = new bool[rows.Count];

                // try removing each row in rows, and store whether removal was successful in rowsRemoved 
                for(int i = 0; i < rows.Count; i++)
                {
                    rowsRemoved[i] = RemoveRow(rows[i]);
                }

                return rowsRemoved;
            }

            public void ClearRows()
            {
                rows.Clear();
            }
            
            public void AddColumn(Column column)
            {
                assertTableEmptyOfRows("AddColumn");

                columns.Add(column);
            }

            public void AddColumnRange<T>(IList<T> columns) where T : Column
            {
                assertTableEmptyOfRows("AddColumnRange");

                this.columns.AddRange(columns);
            }

            // false if remove was unsuccessful / item not found in row list
            public bool RemoveColumn(Column column)
            {
                assertTableEmptyOfRows("RemoveColumn");

                return columns.Remove(column);
            }

            public bool[] RemoveColumnRange<T>(IList<T> columns) where T : Column
            {
                bool[] columnsRemoved = new bool[columns.Count];

                // try removing each column in columns, and store whether removal was successful in rowsRemoved 
                for (int i = 0; i < columns.Count; i++)
                {
                    columnsRemoved[i] = RemoveColumn(columns[i]);
                }

                return columnsRemoved;
            }

            public void ClearColumns()
            {
                assertTableEmptyOfRows("RemoveColumn");

                columns.Clear();
            }

            public string GetColumnHeaderString()
            {
                StringBuilder columnHeaderStringBuilder = new StringBuilder();

                foreach(Column column in columns)
                {
                    columnHeaderStringBuilder.Append(column.ToString());
                }

                return columnHeaderStringBuilder.ToString();
            }
            
            public string GetRowString(int rowIndex)
            {
                return rows[rowIndex].ToString(columns);
            }

            public string GetTableDisplayString()
            {
                StringBuilder tableStringBuilder = new StringBuilder();

                // append column header string
                string columnHeaderString = GetColumnHeaderString();
                tableStringBuilder.Append(columnHeaderString);
                tableStringBuilder.Append(Environment.NewLine);

                // append table rows
                for (int i = 0; i < rows.Count; i++)
                {
                    string rowString = GetRowString(i);
                    tableStringBuilder.Append(rowString);
                    tableStringBuilder.Append(Environment.NewLine);
                }

                return tableStringBuilder.ToString();
            }

            private void assertTableEmptyOfRows(string operationName)
            {
                if (!EmptyOfRows)
                {
                    throw new OperationRequiresEmptyTableException(operationName);
                }
            }

            private void assertColumnCountMatchesTable(Row tableRow)
            {
                if (tableRow.ColumnCount != ColumnCount)
                {
                    throw new RowColumnCountMismatchException();
                }
            }
        }
    }
}

