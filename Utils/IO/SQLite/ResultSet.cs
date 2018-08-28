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

namespace Utils.IO.SQLite
{
    public class ResultSet
    {
        public class Row
        {
            private readonly OrderedDictionary columnNameToColumnValue;
            private readonly int columnCount;

            private string[] columnNames;
            private Object[] columnValues;

            public Row(OrderedDictionary columnNameToColumnValue)
            {
                this.columnNameToColumnValue = columnNameToColumnValue;
                this.columnCount = columnNameToColumnValue.Count;
            }

            public int ColumnCount
            {
                get { return columnCount; }
            }

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

            public string GetColumnName(int columnIndex)
            {
                return columnNameToColumnValue.Cast<DictionaryEntry>().ElementAt(columnIndex).Key.ToString();
            }

            public object GetColumnValue(int columnIndex)
            {
                object columnValue = columnNameToColumnValue[columnIndex];
                return IsNullColumnValue(columnValue) ? null : columnValue;
            }

            public T GetColumnValue<T>(int columnIndex)
            {
                return (T)GetColumnValue(columnIndex);
            }

            public object GetColumnValue(string columnName)
            {
                object columnValue = columnNameToColumnValue[columnName];

                return IsNullColumnValue(columnValue) ? null : columnValue;
            }

            public T GetColumnValue<T>(string columnName)
            {
                return (T)GetColumnValue(columnName);
            }
        }

        private readonly List<Row> rowList = new List<Row>();

        public ResultSet(SQLiteDataReader sqliteDataReader)
        {
            while(sqliteDataReader.Read())
            {
                // <string, object> dictionary
                OrderedDictionary fieldRowDictionary = new OrderedDictionary();

                for (int i = 0; i < sqliteDataReader.FieldCount; i++)
                {
                    fieldRowDictionary.Add(sqliteDataReader.GetName(i), sqliteDataReader.GetValue(i));
                }

                Row row = new Row(fieldRowDictionary);
                rowList.Add(row);
            }
        }

        public int RowCount
        {
            get { return rowList.Count; }
        }

        public Row[] Rows
        {
            get { return rowList.ToArray(); }
        }

        public int ColumnCount
        {
            get { return rowList[0].ColumnCount; }
        }

        public static bool IsNullColumnValue(object columnValue)
        {
            return columnValue.GetType() == typeof(DBNull);
        }

        public string GetColumnName(int columnIndex)
        {
            return rowList[0].GetColumnName(columnIndex);
        }

        public object GetColumnValue(int rowIndex, int columnIndex)
        {
            return rowList[rowIndex].GetColumnValue(columnIndex);
        }

        public Row GetRow(int rowIndex)
        {
            return rowList[rowIndex];
        }

        public T GetColumnValue<T>(int rowIndex, int columnIndex)
        {
            return (T)GetColumnValue(rowIndex, columnIndex);
        }

        public bool IsNullColumnValue(int rowIndex, int ColumnIndex)
        {
            return GetColumnValue(rowIndex, ColumnIndex).GetType() == typeof(DBNull);
        }

        // note that SQLite's INTEGER is equivlent to .NET's long (int64),
        // SQLite's REAL is equivlent to .NET's double,
        // SQLite's NULL is equivlent to .NET's DBNull.
        // returns null for DBNull
        public object GetColumnValue(int rowIndex, string columnName)
        {
            return rowList[rowIndex].GetColumnValue(columnName);
        }

        public T GetColumnValue<T>(int rowIndex, string columnName)
        {
            return (T)GetColumnValue(rowIndex, columnName);
        }
    }
}
