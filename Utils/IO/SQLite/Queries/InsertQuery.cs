using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.IO.SQLite.Queries;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries
    {
        public class InsertQuery : Query
        {
            public class Column
            {
                private readonly string name;
                private readonly object value;

                public Column(string columnName, object columnValue)
                {
                    this.name = columnName;
                    this.value = columnValue;
                }

                public string Name
                {
                    get { return name; }
                }

                public object Value
                {
                    get { return value; }
                }
            }

            private readonly string tableName;
            private readonly Column[] columns;

            private readonly string queryString;

            public InsertQuery(string tableName, Column[] columns)
            {
                this.tableName = tableName;
                this.columns = columns;

                this.queryString = buildQueryString();
            }

            public override string QueryString
            {
                get { return queryString; }
            }

            public string GetColumnName(int columnIndex)
            {
                return this.columns[columnIndex].Name;
            }

            public object GetColumnValue(int columnIndex)
            {
                return this.columns[columnIndex].Value;
            }

            private string buildQueryString()
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("INSERT INTO {0} (", this.tableName);

                // append column names
                for (int i = 0; i < this.columns.Length; i++)
                {
                    string columnName = this.columns[i].Name;
                    queryStringBuilder.AppendFormat("{0}", columnName);

                    if (i < this.columns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                // append values
                queryStringBuilder.Append(") VALUES (");

                for (int i = 0; i < this.columns.Length; i++)
                {
                    string columnValue = this.columns[i].Value.ToString();
                    queryStringBuilder.AppendFormat("'{0}'", columnValue);

                    if (i < this.columns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                queryStringBuilder.Append(")");

                return queryStringBuilder.ToString();
            }
        }
    }
}