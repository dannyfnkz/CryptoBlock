using CryptoBlock.Utils;
using CryptoBlock.Utils.CollectionUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoBlock
{
    namespace TableDisplay
    {
        /// <summary>
        /// represents a table consisting a header (<see cref="GetTableHeaderString"/>, <see cref="Column"/>s,
        /// and <see cref="Row"/>s, where all <see cref="Row"/>s have the same number of <see cref="Column"/>s.
        /// </summary>
        /// <remarks>
        /// displayable as a string in the format:
        ///         col0 | col1 | col2 |...
        /// row0:   val0   val1   val2
        /// row1:   val0   val1   val2
        /// ...
        /// (<see cref="GetTableDisplayString"/>).
        ///  </remarks>
        ///  <seealso cref="Row"/>
        ///  <seealso cref="Column"/>
        public class Table
        {
            /// <summary>
            /// represents a table column.
            /// </summary>
            public class Column
            {
                /// <summary>
                /// thrown if column width provided as argument was negative.
                /// </summary>
                /// <seealso cref="System.ArgumentOutOfRangeException"/>
                public class WidthOutOfRangeException : ArgumentOutOfRangeException
                {
                    public WidthOutOfRangeException(int givenWidth)
                        : base("width", givenWidth, formatExceptionMessage())
                    {

                    }

                    private static string formatExceptionMessage()
                    {
                        return "Width must be greater than zero.";
                    }
                }

                /// <summary>
                /// thrown if column headers array and column widths array, provided as arguments,
                /// had different lengths.
                /// </summary>
                /// <seealso cref="Utils.MismatchException"/>
                public class WidhtsAndHeadersCountMismatchException : MismatchException
                {
                    public WidhtsAndHeadersCountMismatchException()
                        : base("widths.Length", "headers.Length")
                    {

                    }
                }

                protected const string DEFAULT_CUT_SUFFIX = "..";
                protected const int DEFAULT_PADDING = 2;

                private readonly string header;
                private readonly int width;
                private readonly int padding = DEFAULT_PADDING;
                private readonly string cutSuffix = DEFAULT_CUT_SUFFIX;

                /// <summary>
                /// initializes a new column with specified <paramref name="header"/> and <paramref name="width"/>.
                /// </summary>
                /// <param name="header"></param>
                /// <param name="width"></param>
                /// <param name="padding"></param>
                /// <param name="cutSuffix"></param>
                /// <exception cref="WidthOutOfRangeException">
                /// <seealso cref="assertValidWidth(int, string)"/>
                /// </exception>
                public Column(
                    string header,
                    int width,
                    int padding,
                    string cutSuffix)
                    : this(header, width)
                {
                    this.padding = padding;
                    this.cutSuffix = cutSuffix;
                }

                public Column(
                    string header,
                    int width)
                {
                    assertValidWidth(width, header);

                    this.header = header;
                    this.width = width;
                }

                public Column(
                    string header,
                    int width,
                    int padding)
                    : this(header, width)
                {
                    this.padding = padding;
                }

                public Column(
                    string header,
                    int width,
                    string cutSuffix)
                    : this(header, width)
                {
                    this.cutSuffix = cutSuffix;
                }

                /// <summary>
                /// initializes a table column with the same header and width as <paramref name="column"/>
                /// </summary>
                /// <param name="column"></param>
                /// <exception cref="WidthOutOfRangeException"><seealso cref="Column(int,int)"/></exception>
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

                public int Padding
                {
                    get { return padding; }
                }

                public int WidthWithPadding
                {
                    get { return Width + Padding; }
                }

                public string CutSuffix
                {
                    get { return cutSuffix; }
                }

                /// <summary>
                /// parses a column array using <paramref name="headers"/> and <paramref name="widths"/> lists,
                /// where (<paramref name="headers"/>[i], <paramref name="widths"/>) specify the i'th column.
                /// </summary>
                /// <param name="headers"></param>
                /// <param name="widths"></param>
                /// <returns>
                /// a column array where the i'th element has header of <paramref name="headers"/>[i]
                /// and width of <paramref name="widths"/>[i]
                /// </returns>
                /// <exception cref="WidhtsAndHeadersCountMismatchException">
                /// <seealso cref="assertMatchingWidthsAndHeadersCount(IList{string}, IList{int})"/>
                /// </exception>
                public static Column[] ParseArray(IList<string> headers, IList<int> widths)
                {
                    // assert headers.Length and widths.Length match
                    assertMatchingWidthsAndHeadersCount(headers, widths);

                    Column[] columns = new Column[headers.Count];

                    // i'th column in columns has (headers[i], widths[i]) as its values
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
                    // shorten header if it is longer than column width
                    string shortenedHeader = StringUtils.ShortenIfLongerThan(Header, Width, CutSuffix);
                     
                    return shortenedHeader.PadRight(WidthWithPadding);
                }

                /// <summary>
                /// asserts that width is greater than or equal to zero.
                /// </summary>
                /// <param name="width"></param>
                /// <param name="header"></param>
                /// <exception cref="WidthOutOfRangeException">
                /// thrown if <paramref name="width"/> is negative.
                /// </exception>
                private static void assertValidWidth(int width, string header)
                {
                    if (width < 0)
                    {
                        throw new WidthOutOfRangeException(width);
                    }
                }

                /// <summary>
                /// asserts that <paramref name="headers"/> and <paramref name="widths"/> have matching lengths.
                /// </summary>
                /// <param name="headers"></param>
                /// <param name="widths"></param>
                /// <exception cref="WidhtsAndHeadersCountMismatchException">
                /// thrown if <paramref name="headers"/> and <paramref name="widths"/> don't have matching lengths.
                /// </exception>
                private static void assertMatchingWidthsAndHeadersCount(IList<string> headers, IList<int> widths)
                {
                    if(headers.Count != widths.Count)
                    {
                        throw new WidhtsAndHeadersCountMismatchException();
                    }
                }
            }

            /// <summary>
            /// represents a table row.
            /// </summary>
            public class Row
            {
                private readonly string[] columnValues;

                /// <summary>
                /// initializes a table row with <paramref name="columnValues"/> as its column values. 
                /// </summary>
                /// <param name="columnValues"></param>
                /// <exception cref="ArgumentNullException">
                /// <seealso cref="CollectionUtils.ConvertToArray{T}(IEnumerable{T})"/>
                /// </exception>
                public Row(IEnumerable<string> columnValues)
                {
                    this.columnValues = CollectionUtils.ConvertToArray(columnValues);
                }

                /// <summary>
                /// number of column values row has.
                /// </summary>
                public int ColumnCount
                {
                    get { return columnValues.Length; }
                }

                /// <summary>
                /// array of column values row has.
                /// </summary>
                public string[] ColumnValues
                {
                    get { return columnValues; }
                }

                /// <summary>
                /// returns the value of the column at index <paramref name="columnIndex"/>. 
                /// </summary>
                /// <param name="columnIndex"></param>
                /// <returns>
                /// value of the column at index <paramref name="columnIndex"/>. 
                /// </returns>
                /// <exception cref="System.IndexOutOfRangeException"></exception>
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

                /// <summary>
                /// returns the string representation of row,
                /// consisting of row column values, where the i'th column value is aligned to the right
                /// according to <paramref name="columns"/>[i].Width.
                /// </summary>
                /// <remarks>
                /// if <paramref name="columns"/>[i].Width is less than actual i'th column value width,
                /// <see cref="String.PadRight(int)"/> returns the original string.
                /// </remarks>
                /// <param name="columns"></param>
                /// <returns>
                /// string representation of row, consisting of the column values,
                /// aligned according to <paramref name="columns"/> widths
                /// </returns>
                public string ToString(IList<Column> columns)
                {
                    StringBuilder rowStringBuilder = new StringBuilder();

                    // append all column values to create the row string
                    for(int i = 0; i < columnValues.Length; i++)
                    {
                        string columnValue = columnValues[i];

                        // shorten current column value if it is longer than current column width
                        string columnValueDisplayString = StringUtils.ShortenIfLongerThan(
                            columnValue,
                            columns[i].Width,
                            columns[i].CutSuffix);

                        // pad columnValueDisplayString according to column width
                        columnValueDisplayString = columnValueDisplayString.PadRight(columns[i].WidthWithPadding);

                        // append columnValueDisplayString
                        rowStringBuilder.Append(columnValueDisplayString);
                    }

                    return rowStringBuilder.ToString();
                }
            }

            /// <summary>
            /// represents an exception related to the <see cref="Table"/> class.
            /// </summary>
            public class TableException : Exception
            {
                public TableException(string exceptionMessage)
                    : base(exceptionMessage)
                {

                }
            }

            /// <summary>
            /// thrown if a <see cref="Row"/> was attempted to be added,
            /// which has a different column count than the <see cref="Table"/> it was being added to.
            /// </summary>
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

            /// <summary>
            /// thrown if an operation which requires table to be empty of rows was attempted to be performed.
            /// </summary>
            public class OperationRequiresEmptyTableException : Exception
            {
                private string operationName;

                public OperationRequiresEmptyTableException(string operationName)
                    : base(formatExceptionMessage(operationName))
                {
                    this.operationName = operationName;
                }

                /// <summary>
                /// name of the operation which was attempted to be performed.
                /// </summary>
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

            // table columns
            private List<Column> columns = new List<Column>();

            // table rows
            private List<Row> rows = new List<Row>();

            /// <summary>
            /// initializes a table with specified <paramref name="tableColumns"/> and <paramref name="tableRows"/>.
            /// </summary>
            /// <param name="tableColumns"></param>
            /// <param name="tableRows"></param>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="AddColumnRange{T}(IList{T})"/>
            /// </exception>
            /// <exception cref="RowColumnCountMismatchException">
            /// <seealso cref="AddRowRange{T}(IList{T})"/>
            /// </exception>
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

            /// <summary>
            /// number of columns in table.
            /// </summary>
            public int ColumnCount
            {
                get { return columns.Count; }
            }

            /// <summary>
            /// number of rows in table.
            /// </summary>
            public int RowCount
            {
                get { return rows.Count; }
            }

            /// <summary>
            /// table row count is zero.
            /// </summary>
            public bool EmptyOfRows
            {
                get { return rows.Count == 0; }
            }

            /// <summary>
            /// returns row at index <paramref name="rowIndex"/>.
            /// </summary>
            /// <param name="rowIndex"></param>
            /// <returns>
            /// row at index <paramref name="rowIndex"/>.
            /// </returns>
            /// <exception cref="SystemIndexOutOfRangeException"></exception>
            public Row GetRow(int rowIndex)
            {
                return rows[rowIndex];
            }

            /// <summary>
            /// adds <paramref name="tableRow"/> to <see cref="Table"/>'s row list.
            /// </summary>
            /// <param name="tableRow"></param>
            /// <exception cref="RowColumnCountMismatchException">
            /// <seealso cref="assertColumnCountMatchesTable(Row)"/>
            /// </exception>
            public void AddRow(Row tableRow)
            {
                assertColumnCountMatchesTable(tableRow);

                rows.Add(tableRow);
            }

            /// <summary>
            /// adds all rows in <paramref name="rows"/> to table row list.
            /// </summary>
            /// <typeparam name="T">inherits from <see cref="Row"/></typeparam>
            /// <param name="rows"></param>
            /// <exception cref="RowColumnCountMismatchException">
            /// <seealso cref="assertColumnCountMatchesTable(Row)"/>
            /// </exception>
            /// <exception cref="System.ArgumentNullException">
            /// <seealso cref="List{T}.AddRange(IEnumerable{T})"/>
            /// </exception>
            public void AddRowRange<T>(IList<T> rows) where T : Row
            {
                foreach(Row row in rows)
                {
                    assertColumnCountMatchesTable(row);
                }

                this.rows.AddRange(rows);
            }

            /// <summary>
            /// <para>
            /// removes <paramref name="row"/> from table row list.
            /// </para>
            /// <para>
            /// returns whether <paramref name="row"/> existed in <see cref="Table"/> row list prior to being removed.
            /// </para>
            /// </summary>
            /// <param name="row"></param>
            /// <returns>
            /// true if <paramref name="row"/> existed in <see cref="Table"/> row list prior to being removed,
            /// else false
            /// </returns>
            public bool RemoveRow(Row row)
            {
                return rows.Remove(row);
            }

            /// <summary>
            /// <para>
            /// removes each <see cref="Row"/> in <paramref name="rows"/> from <see cref="Table"/>'s
            /// row list, if it exists there.
            /// </para>
            /// <para>
            /// returns a bool array of length <paramref name="rows"/>.Count where the i'th item is
            /// true iff <paramref name="rows"/>[i] existed in <see cref="Table"/>'s row list.
            /// </para>
            /// </summary>
            /// <typeparam name="T">inherits from <see cref="Row"/>.</typeparam>
            /// <param name="rows"></param>
            /// <returns>
            /// bool array of length <paramref name="rows"/>.Count where the i'th item is
            /// true iff <paramref name="rows"/>[i] existed in <see cref="Table"/>'s row list
            /// </returns>
            public bool[] RemoveRowRange<T>(IList<T> rows) where T : Row
            {
                bool[] rowsRemoved = new bool[rows.Count];
  
                for(int i = 0; i < rows.Count; i++)
                {
                    // try removing i'th row in rows, and store whether removal was successful in rowsRemoved[i] 
                    rowsRemoved[i] = RemoveRow(rows[i]);
                }

                return rowsRemoved;
            }

            /// <summary>
            /// removes all rows from table row list.
            /// </summary>
            public void ClearRows()
            {
                rows.Clear();
            }

            /// <summary>
            /// adds <paramref name="column"/> to table column list.
            /// </summary>
            /// <param name="column"></param>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="assertTableEmptyOfRows(string)"/>
            /// </exception>
            public void AddColumn(Column column)
            {
                assertTableEmptyOfRows("AddColumn");

                columns.Add(column);
            }

            /// <summary>
            /// adds all columns in <paramref name="columns"/> to table column list.
            /// </summary>
            /// <typeparam name="T">inherits from <see cref="Column"/></typeparam>
            /// <param name="columns"></param>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="assertTableEmptyOfRows(string)"/>
            /// </exception>
            public void AddColumnRange<T>(IList<T> columns) where T : Column
            {
                assertTableEmptyOfRows("AddColumnRange");

                this.columns.AddRange(columns);
            }

            // false if remove was unsuccessful / item not found in row list
            /// <summary>
            /// removes <see cref="Column"/> from table column list, if it exists there.
            /// </summary>
            /// <param name="column"></param>
            /// <returns>
            /// true if column existed in table column list before removal, else false
            /// </returns>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="assertTableEmptyOfRows(string)"/>
            /// </exception>
            public bool RemoveColumn(Column column)
            {
                assertTableEmptyOfRows("RemoveColumn");
                return columns.Remove(column);
            }

            /// <summary>
            /// removes every <see cref="Column"/> in <paramref name="columns"/> from table column list,
            /// if it exists there.
            /// </summary>
            /// <typeparam name="T">inherits from <see cref="Column"/></typeparam>
            /// <param name="columns"></param>
            /// <returns>
            /// bool array of length <paramref name="columns"/>.Count where the i'th item is
            /// true iff <paramref name="columns"/>[i] existed in table column list
            /// </returns>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="RemoveColumn(Column)"/>
            /// </exception>
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

            /// <summary>
            /// removes all <see cref="Column"/>s from table column list.
            /// </summary>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="assertTableEmptyOfRows(string)"/>
            /// </exception>
            public void ClearColumns()
            {
                assertTableEmptyOfRows("RemoveColumn");

                columns.Clear();
            }

            /// <summary>
            /// returns a string representation of the table header, consisting of <see cref="Column"/> headers
            /// appended to each other in order of <see cref="Column"/>s.
            /// </summary>
            /// <returns>
            /// string representing the table header
            /// </returns>
            public string GetTableHeaderString()
            {
                StringBuilder tableHeaderStringBuilder = new StringBuilder();

                foreach(Column column in columns)
                {
                    tableHeaderStringBuilder.Append(column.ToString());
                }

                return tableHeaderStringBuilder.ToString();
            }

            /// <summary>
            /// returns a string representing the <paramref name="rowIndex"/>'th <see cref="Row"/>
            /// in table row list.
            /// </summary>
            /// <param name="rowIndex"></param>
            /// <returns>
            /// string representing the <paramref name="rowIndex"/>'th <see cref="Row"/>
            /// in table row list.
            /// </returns>
            /// <seealso cref="Row.ToString(IList{Column})"/>
            /// <exception cref="System.IndexOutOfRangeException">thrown if index was invalid.</exception>
            public string GetRowString(int rowIndex)
            {
                return rows[rowIndex].ToString(columns);
            }

            /// <summary>
            /// returns a string representation of the table,
            /// consisting of the table header and all table <see cref="Row"/>s.
            /// </summary>
            /// <returns>
            /// string representation of the table
            /// </returns>
            /// <seealso cref="GetTableHeaderString"/>
            /// <seealso cref="Row.ToString(IList{Column})"/>
            public string GetTableDisplayString()
            {
                StringBuilder tableStringBuilder = new StringBuilder();

                // append column header string
                string columnHeaderString = GetTableHeaderString();
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

            /// <summary>
            /// asserts that table <see cref="Row"/> list is empty.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// thrown if table row list is not empty
            /// </exception>
            private void assertTableEmptyOfRows(string operationName)
            {
                if (!EmptyOfRows)
                {
                    throw new OperationRequiresEmptyTableException(operationName);
                }
            }

            /// <summary>
            /// asserts that <see cref="Row.ColumnCount"/> is equal to number of columns in table.
            /// </summary>
            /// <param name="tableRow"></param>
            /// <exception cref="RowColumnCountMismatchException">
            /// thrown if <see cref="Row.ColumnCount"/> is not equal to number of columns in table.
            /// </exception>
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

