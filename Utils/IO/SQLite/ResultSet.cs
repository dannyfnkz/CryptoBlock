using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Collections;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections;
using static Utils.IO.SQLite.ResultSet.Row;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Read;

namespace Utils.IO.SQLite
{
    /// <summary>
    /// represents a set of data <see cref="Row"/>s, returned as a result of executing
    /// a <see cref="SelectQuery"/>.
    /// </summary>
    public class ResultSet
    {    
        /// <summary>
        /// represents a data row in <see cref="ResultSet"/>.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// thrown if specified column name does not exist in <see cref="Row"/>.
            /// </summary>
            public class ColumnNameNotFoundException : Exception
            {
                private readonly string columnName;

                public ColumnNameNotFoundException(string columnName)
                    : base(formatExceptionMessage(columnName))
                {
                    this.columnName = columnName;
                }

                public string ColumnName
                {
                    get { return columnName; }
                }

                private static string formatExceptionMessage(string columnName)
                {
                    return string.Format(
                        "Column with specified name '{0}' does not exist in Row.",
                        columnName);
                }
            }

            /// <summary>
            /// thrown if specified column index is a negative number, or
            /// larger than or equal to <see cref="Row"/> column count.
            /// </summary>
            public class ColumnIndexOutOfRangeException : Exception
            {
                private readonly int columnIndex;

                public ColumnIndexOutOfRangeException(int columnIndex)
                    : base(formatExceptionMessage(columnIndex))
                {
                    this.columnIndex = columnIndex;
                }

                public int ColumnIndex
                {
                    get { return columnIndex; }
                }

                private static string formatExceptionMessage(int columnIndex)
                {
                    return string.Format(
                        "Specified column index '{0}' in Row is out of range.",
                        columnIndex);
                }
            }

            private readonly OrderedDictionary columnNameToColumnValue;

            private string[] columnNames;
            private Object[] columnValues;

            public Row(OrderedDictionary columnNameToColumnValue)
            {
                this.columnNameToColumnValue = columnNameToColumnValue;
            }

            /// <summary>
            /// number of columns in this <see cref="Row"/>.
            /// </summary>
            public int ColumnCount
            {
                get { return columnNameToColumnValue.Count; }
            }

            /// <summary>
            /// array containing names of all columns in this <see cref="Row"/>.
            /// </summary>
            public string[] ColumnNames
            {
                get
                {
                    if(columnNames == null)
                    {
                        columnNames =
                            this.columnNameToColumnValue.Keys.ToArray().CastAll<object,string>();
                    }

                    return columnNames;
                }
            }

            /// <summary>
            /// array containing values of all columns in this <see cref="Row"/>.
            /// </summary>
            public Object[] ColumnValues
            {
                get
                {
                    if(columnValues == null)
                    {
                        columnValues = this.columnNameToColumnValue.Values.ToArray();
                    }

                    return columnValues;
                }
            }

            /// <summary>
            /// returns the name of the column at <paramref name="columnIndex"/>.
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns>
            /// name of the column at <paramref name="columnIndex"/>
            /// </returns>
            /// <exception cref="ColumnIndexOutOfRangeException">
            /// <seealso cref="assertColumnIndexWithinRange(int)"/>
            /// </exception>
            public string GetColumnName(int columnIndex)
            {
                assertColumnIndexWithinRange(columnIndex);
                return columnNameToColumnValue.Cast<DictionaryEntry>()
                    .ElementAt(columnIndex).Key.ToString();
            }

            /// <summary>
            /// returns value of column at <paramref name="columnIndex"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="columnIndex"></param>
            /// <returns>
            /// value of column at <paramref name="columnIndex"/>
            /// </returns>
            /// <exception cref="ColumnIndexOutOfRangeException">
            /// <seealso cref="GetColumnValue(int)"/>
            /// </exception>
            public T GetColumnValue<T>(int columnIndex)
            {
                return (T)GetColumnValue(columnIndex);
            }

            /// <summary>
            /// returns value of column at <paramref name="columnIndex"/>.
            /// </summary>
            /// <seealso cref="getColumnValue(object)"/>
            /// <param name="columnIndex"></param>
            /// <returns>
            /// value of column at <paramref name="columnIndex"/>
            /// </returns>
            /// <exception cref="ColumnIndexOutOfRangeException">
            /// <seealso cref="assertColumnIndexWithinRange(int)"/>
            /// </exception>
            public object GetColumnValue(int columnIndex)
            {
                assertColumnIndexWithinRange(columnIndex);

                object columnValueObject = columnNameToColumnValue[columnIndex];

                return getColumnValue(columnValueObject);
            }

            /// <summary>
            /// returns value of column having <paramref name="columnName"/>.
            /// </summary>
            /// <seealso cref="getColumnValue(object)"/>
            /// <typeparam name="T"></typeparam>
            /// <param name="columnName"></param>
            /// <returns>
            /// value of column having <paramref name="columnName"/>
            /// </returns>
            /// <exception cref="ColumnNameNotFoundException">
            /// <seealso cref="GetColumnValue(String)"/>
            /// </exception>
            public T GetColumnValue<T>(string columnName)
            {
                return (T)GetColumnValue(columnName);
            }

            /// <summary>
            /// returns value of column having <paramref name="columnName"/>.
            /// </summary>
            /// <param name="columnName"></param>
            /// <returns>
            /// value of column having <paramref name="columnName"/>
            /// </returns>
            /// <exception cref="ColumnNameNotFoundException">
            /// <seealso cref="assertColumnNameExists(string)"/>
            /// </exception>
            public object GetColumnValue(string columnName)
            {
                assertColumnNameExists(columnName);

                object columnValueObject = columnNameToColumnValue[columnName];

                return getColumnValue(columnValueObject);
            }

            /// <summary>
            /// returns the value contained in <paramref name="columnValueSQLiteObject"/>.
            /// </summary>
            /// <remarks>
            /// note that SQLite's INTEGER is equivlent to .NET's long (int64),
            /// SQLite's REAL is equivlent to .NET's double,
            /// SQLite's DBNull is equivlent to .NET's null. 
            /// </remarks>
            /// <param name="columnValueObject"></param>
            /// <returns>
            /// value contained in <paramref name="columnValueSQLiteObject"/>
            /// </returns>
            private object getColumnValue(object columnValueSQLiteObject)
            {
                return isNullSQLiteColumnValue(columnValueSQLiteObject) ? null : columnValueSQLiteObject;
            }

            /// <summary>
            /// returns whether <paramref name="columnValueSQLiteObject"/> has a NULL
            /// SQLite database value.
            /// </summary>
            /// <param name="columnValue"></param>
            /// <returns>
            /// true if <paramref name="columnValueSQLiteObject"/> has a NULL
            /// SQLite database value,
            /// else false
            /// </returns>
            private static bool isNullSQLiteColumnValue(object columnValueSQLiteObject)
            {
                return columnValueSQLiteObject.GetType() == typeof(DBNull);
            }

            /// <summary>
            /// asserts that <paramref name="columnIndex"/> is non-negative and smaller than
            /// <see cref="ColumnCount"/>
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <exception cref="ColumnIndexOutOfRangeException">
            /// thrown if <paramref name="columnIndex"/> is either negative, or larger than or equal to 
            /// <see cref="ColumnCount"/>
            /// </exception>
            private void assertColumnIndexWithinRange(int columnIndex)
            {
                if(columnIndex < 0 || columnIndex > this.ColumnCount)
                {
                    throw new ColumnIndexOutOfRangeException(columnIndex);
                }
            }

            /// <summary>
            /// asserts that a column with <paramref name="columnName"/> exists in this <see cref="Row"/>. 
            /// </summary>
            /// <param name="columnName"></param>
            /// <exception cref="ColumnNameNotFoundException">
            /// thrown if a column with <paramref name="columnName"/> does not exist
            /// in this <see cref="Row"/>
            /// </exception>
            private void assertColumnNameExists(string columnName)
            {
                if(!this.columnNameToColumnValue.Contains(columnName))
                {
                    throw new ColumnNameNotFoundException(columnName);
                }
            }
        }

        /// <summary>
        /// thrown if an operation was performed for which a non-empty <see cref="ResultSet"/> is required.
        /// </summary>
        public class EmptyResultSetException : Exception
        {
            private readonly string operationName;

            public EmptyResultSetException(string operationName)
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
                    "Operation '{0}' cannot be performed on an empty result set.",
                    operationName);
            }
        }

        /// <summary>
        /// thrown if specified row index is a negative number, or
        /// larger than or equal to <see cref="ResultSet.RowCount"/>.
        /// </summary>
        public class RowIndexOutOfRangeException : Exception
        {
            private readonly int rowIndex;

            public RowIndexOutOfRangeException(int rowIndex)
                : base(formatExceptionMessage(rowIndex))
            {
                this.rowIndex = rowIndex;
            }

            public int RowIndex
            {
                get { return rowIndex; }
            }

            private static string formatExceptionMessage(int rowIndex)
            {
                return string.Format(
                    "Specified row index '{0}' in Row is out of range.",
                    rowIndex);
            }
        }

        private readonly List<Row> rowList = new List<Row>();

        public ResultSet(SQLiteDataReader sqliteDataReader)
        {
            while(sqliteDataReader.Read()) // add rows to rowList based on data read from sqliteDataReader
            {
                // init <string, object> dictionary
                OrderedDictionary fieldRowDictionary = new OrderedDictionary();

                // add each column in row to fieldRowDictionary
                for (int i = 0; i < sqliteDataReader.FieldCount; i++)
                {
                    fieldRowDictionary.Add(sqliteDataReader.GetName(i), sqliteDataReader.GetValue(i));
                }

                // init a new Row based on underlying fieldRowDictionary
                Row row = new Row(fieldRowDictionary);

                // add row to rowList
                rowList.Add(row);
            }
        }

        /// <summary>
        /// number of <see cref="Row"/>s in this <see cref="ResultSet"/>.
        /// </summary>
        public int RowCount
        {
            get { return rowList.Count; }
        }

        /// <summary>
        /// array containing all <see cref="Row"/>s in this <see cref="ResultSet"/>
        /// </summary>
        public Row[] Rows
        {
            get { return rowList.ToArray(); }
        }

        /// <summary>
        /// number of columns in this <see cref="ResultSet"/>.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                assertResultSetNotEmpty("ColumnCount");
                return rowList[0].ColumnCount;
            }
        }

        /// <summary>
        /// returns the name of the column at <paramref name="columnIndex"/>.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns>
        /// name of the column at <paramref name="columnIndex"/>
        /// </returns>
        /// <exception cref="EmptyResultSetException">
        /// <seealso cref="assertResultSetNotEmpty(string)"/>
        /// </exception>
        /// <exception cref="ColumnIndexOutOfRangeException>">
        /// <seealso cref="Row.GetColumnName(int)"/>
        /// </exception>
        public string GetColumnName(int columnIndex)
        {
            assertResultSetNotEmpty("GetColumnName");
            return rowList[0].GetColumnName(columnIndex);
        }

        /// <summary>
        /// returns the <see cref="Row"/> at <paramref name="rowIndex"/>.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns>
        /// <see cref="Row"/> at <paramref name="rowIndex"/>
        /// </returns>
        /// <exception cref="assertRowIndexWithinRange(int)">
        /// <seealso cref="assertRowIndexWithinRange(int)"/>
        /// </exception>
        public Row GetRow(int rowIndex)
        {
            assertRowIndexWithinRange(rowIndex);
            return rowList[rowIndex];
        }

        /// <summary>
        /// returns the value of the <paramref name="columnIndex"/>'th column in the 
        /// <paramref name="rowIndex"/>'th row, cast to <typeparamref name="T"/>.
        /// </summary>
        /// <seealso cref="GetColumnValue(int, int)"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns>
        /// value of the <paramref name="columnIndex"/>'th column in the 
        /// <paramref name="rowIndex"/>'th row, cast to <typeparamref name="T"/>
        /// </returns>
        /// <exception cref="RowIndexOutOfRangeException">
        /// <seealso cref="GetColumnValue(int, int)"/>
        /// </exception>
        /// <exception cref="ColumnIndexOutOfRangeException">
        /// <seealso cref="GetColumnValue(int, int)"/>
        /// </exception>
        public T GetColumnValue<T>(int rowIndex, int columnIndex)
        {
            return (T)GetColumnValue(rowIndex, columnIndex);
        }

        /// <summary>
        /// returns the value of the <paramref name="columnIndex"/>'th column in the 
        /// <paramref name="rowIndex"/>'th row.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns>
        /// value of the <paramref name="columnIndex"/>'th column in the 
        /// <paramref name="rowIndex"/>'th row
        /// </returns>
        /// <exception cref="RowIndexOutOfRangeException">
        /// <seealso cref="assertRowIndexWithinRange(int)"/>
        /// </exception>
        /// <exception cref="ColumnIndexOutOfRangeException">
        /// <seealso cref="Row.GetColumnValue(int)"/>
        /// </exception>
        public object GetColumnValue(int rowIndex, int columnIndex)
        {
            assertRowIndexWithinRange(rowIndex);
            return rowList[rowIndex].GetColumnValue(columnIndex);
        }

        /// <summary>
        /// returns the value of column '<paramref name="columnName"/>' in the
        /// <paramref name="rowIndex"/>'th row, cast to <typeparamref name="T"/>.
        /// </summary>
        /// <seealso cref="GetColumnValue(int, string)"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowIndex"></param>
        /// <param name="columnName"></param>
        /// <returns>
        /// value of column '<paramref name="columnName"/>' in the
        /// <paramref name="rowIndex"/>'th row, cast to <typeparamref name="T"/>
        /// </returns>
        /// <exception cref="RowIndexOutOfRangeException">
        /// <seealso cref="GetColumnValue(int, string)"/>
        /// </exception>
        /// <exception cref="ColumnNameNotFoundException">
        /// <seealso cref="GetColumnValue(int, string)"/>
        /// </exception>
        public T GetColumnValue<T>(int rowIndex, string columnName)
        {
            return (T)GetColumnValue(rowIndex, columnName);
        }

        /// <summary>
        /// returns the value of column '<paramref name="columnName"/>' in the
        /// <paramref name="rowIndex"/>'th row.
        /// </summary>
        /// <seealso cref="Row.GetColumnValue(string)"/>
        /// <param name="rowIndex"></param>
        /// <param name="columnName"></param>
        /// <returns>
        /// value of column '<paramref name="columnName"/>' in the
        /// <paramref name="rowIndex"/>'th row
        /// </returns>
        /// <exception cref="RowIndexOutOfRangeException">
        /// <seealso cref="assertRowIndexWithinRange(int)"/>
        /// </exception>
        /// <exception cref="ColumnNameNotFoundException">
        /// <seealso cref="Row.GetColumnValue(string)"/>
        /// </exception>
        public object GetColumnValue(int rowIndex, string columnName)
        {
            assertRowIndexWithinRange(rowIndex);
            return rowList[rowIndex].GetColumnValue(columnName);
        }

        /// <summary>
        /// asserts that <paramref name="rowIndex"/> is non-negative and smaller than
        /// <see cref="RowCount"/>.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <exception cref="RowIndexOutOfRangeException">
        /// thrown if <paramref name="rowIndex"/> is negative or larger than or equal to
        /// <see cref="RowCount"/>
        /// </exception>
        private void assertRowIndexWithinRange(int rowIndex)
        {
            if(rowIndex < 0 || rowIndex >= this.RowCount)
            {
                throw new RowIndexOutOfRangeException(rowIndex);
            }
        }

        /// <summary>
        /// asserts that this <see cref="ResultSet"/> contains at least one row.
        /// </summary>
        /// <param name="operationName"></param>
        /// <exception cref="EmptyResultSetException">
        /// thrown if this <see cref="ResultSet"/> contains no rows
        /// </exception>
        private void assertResultSetNotEmpty(string operationName)
        {
            if(rowList.Count == 0)
            {
                throw new EmptyResultSetException(operationName);
            }
        }
    }
}
