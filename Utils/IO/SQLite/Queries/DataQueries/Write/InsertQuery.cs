using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.DataQueries.Write
    {
        public class InsertQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly ValuedColumn[] valuedColumns;

            public InsertQuery(string tableName, IList<ValuedColumn> valuedColumns)
            {
                this.tableName = tableName;
                this.valuedColumns = valuedColumns.ToArray();
            }
            
            public ValuedColumn[] ValuedColumns
            {
                get { return valuedColumns; }
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

            protected override string BuildQueryString()
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
                    string valuedColumnValueString;

                    if (valuedColumnValue is IExpression) // value is an SQL expression
                    {
                        valuedColumnValueString = (valuedColumnValue as IExpression).ExpressionString;
                        queryStringBuilder.AppendFormat("({0})", valuedColumnValueString);
                    }
                    else // value is a literal
                    {
                        valuedColumnValueString = valuedColumnValue.ToString();
                        queryStringBuilder.AppendFormat("'{0}'", valuedColumnValueString);
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