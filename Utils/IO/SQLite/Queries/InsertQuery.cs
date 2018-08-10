using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utils.IO.SQLite.Queries;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries
    {
        public class InsertQuery : Query
        {
            private readonly string tableName;
            private readonly ValuedColumn[] valuedColumns;

            private readonly string queryString;

            public InsertQuery(string tableName, ValuedColumn[] valuedColumns)
            {
                this.tableName = tableName;
                this.valuedColumns = valuedColumns;

                this.queryString = buildQueryString();
            }

            public override string QueryString
            {
                get { return queryString; }
            }

            public static InsertQuery Parse(XmlNode rowDataXmlNode, string tableName)
            {
                // get ValuedColumns
                XmlNodeList valuedColumnsXmlNodeList = rowDataXmlNode.SelectNodes("column");

                // number of specified columns might be zero (?)
                ValuedColumn[] valuedColumns = new ValuedColumn[valuedColumnsXmlNodeList.Count];

                for(int i = 0; i < valuedColumnsXmlNodeList.Count; i++)
                {
                    XmlNode valuedColumnXmlNode = valuedColumnsXmlNodeList[0];
                    valuedColumns[i] = ValuedColumn.Parse(valuedColumnXmlNode);
                }

                InsertQuery insertQuery = new InsertQuery(tableName, valuedColumns);

                return insertQuery;
            }

            public string GetValuedColumnName(int valuedColumnIndex)
            {
                return this.valuedColumns[valuedColumnIndex].Name;
            }

            public object GetValuedColumnValue(int valuedColumnIndex)
            {
                return this.valuedColumns[valuedColumnIndex].Value;
            }

            private string buildQueryString()
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("INSERT INTO {0} (", this.tableName);

                // append valuedColumn names
                for (int i = 0; i < this.valuedColumns.Length; i++)
                {
                    string valuedColumnName = this.valuedColumns[i].Name;
                    queryStringBuilder.AppendFormat("{0}", valuedColumnName);

                    if (i < this.valuedColumns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                // append values
                queryStringBuilder.Append(") VALUES (");

                for (int i = 0; i < this.valuedColumns.Length; i++)
                {
                    object valuedColumnValue = this.valuedColumns[i].Value;

                    if(valuedColumnValue is Query)
                    {
                        string queryString = (valuedColumnValue as Query).QueryString;
                        queryStringBuilder.AppendFormat("({0})", queryString);
                    }
                    else
                    {
                        queryStringBuilder.AppendFormat("'{0}'", valuedColumnValue.ToString());
                    }
                    

                    if (i < this.valuedColumns.Length - 1)
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