using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Collections;

namespace Utils.IO.SQLite
{
    public class ResultSet
    {
        private readonly List<OrderedDictionary> fieldRows = new List<OrderedDictionary>();

        public ResultSet(SQLiteDataReader sqliteDataReader)
        {
            while(sqliteDataReader.Read())
            {
                OrderedDictionary fieldRowDictionary = new OrderedDictionary();

                // OrderedDictionary indexes inserts in reverse order, so run from last column to first
                for (int i = sqliteDataReader.FieldCount - 1; i >= 0; i--)
                {
                    fieldRowDictionary.Add(sqliteDataReader.GetName(i), sqliteDataReader.GetValue(i));
                }

                fieldRows.Add(fieldRowDictionary);
            }
        }

        public int RowCount
        {
            get { return fieldRows.Count; }
        }

        public int ColumnCount
        {
            get { return fieldRows[0].Count; }
        }

        public string GetColumnName(int columnIndex)
        {
            return fieldRows[0].Cast<DictionaryEntry>().ElementAt(columnIndex).Key.ToString();
        }

        public object GetColumnValue(int rowIndex, int columnIndex)
        {
            return fieldRows[rowIndex][columnIndex];
        }

        public T GetColumnValue<T>(int rowIndex, int columnIndex)
        {
            return (T)GetColumnValue(rowIndex, columnIndex);
        }

        public object GetColumnValue(int rowIndex, string columnName)
        {
            return fieldRows[rowIndex][columnName];
        }

        public T GetColumnValue<T>(int rowIndex, string columnName)
        {
            return (T)GetColumnValue(rowIndex, columnName);
        }
    }
}
