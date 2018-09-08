using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
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
        /// <summary>
        /// thrown if <see cref="InsertQuery"/> parse fails.
        /// </summary>
        public class InsertQueryParseException : SQLiteParseExcetion
        {
            private static readonly Type INSERT_QUERY_TYPE = typeof(InsertQuery);

            public InsertQueryParseException(
                Exception innerException = null)
                : base(INSERT_QUERY_TYPE, null, innerException)
            {

            }
        }

        /// <summary>
        ///  represents a <see cref="DataWriteQuery"/> which performs an INSERT operation.
        /// </summary>
        public class InsertQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly ValuedColumn[] valuedColumnArray;

            public InsertQuery(string tableName, IList<ValuedColumn> valuedColumns)
            {
                this.tableName = tableName;
                this.valuedColumnArray = valuedColumns.ToArray();
            }

            public string TableName
            {
                get { return tableName; }
            }
            
            public ValuedColumn[] ValuedColumns
            {
                get { return valuedColumnArray; }
            }

            /// <summary>
            /// parses <see cref="InsertQuery"/> having <paramref name="tableName"/> from
            /// <paramref name="rowDataXmlNode"/>.
            /// </summary>
            /// <param name="rowDataXmlNode"></param>
            /// <param name="tableName"></param>
            /// <returns>
            /// <see cref="InsertQuery"/> having <paramref name="tableName"/> parsed from
            /// <paramref name="rowDataXmlNode"/>
            /// </returns>
            /// <exception cref="InsertQueryParseException">
            /// thrown if <see cref="InsertQuery"/> parse failed
            /// </exception>
            public static InsertQuery Parse(XmlNode rowDataXmlNode, string tableName)
            {
                try
                {
                    // get ValuedColumns
                    XmlNodeList valuedColumnsXmlNodeList = rowDataXmlNode.GetNodes("column");

                    // number of specified columns might be zero (?)
                    ValuedColumn[] valuedColumns = new ValuedColumn[valuedColumnsXmlNodeList.Count];

                    for (int i = 0; i < valuedColumnsXmlNodeList.Count; i++)
                    {
                        XmlNode valuedColumnXmlNode = valuedColumnsXmlNodeList[0];
                        valuedColumns[i] = ValuedColumn.Parse(valuedColumnXmlNode);
                    }

                    InsertQuery insertQuery = new InsertQuery(tableName, valuedColumns);

                    return insertQuery;
                }
                catch(XmlNodeMissingAttributeException xmlNodeMissingAttributeException)
                {
                    throw new InsertQueryParseException(xmlNodeMissingAttributeException);
                }
            }

            public string GetValuedColumnName(int valuedColumnIndex)
            {
                return this.ValuedColumns[valuedColumnIndex].Name;
            }

            public object GetValuedColumnValue(int valuedColumnIndex)
            {
                return this.ValuedColumns[valuedColumnIndex].Value;
            }

            protected override string BuildQueryString()
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("INSERT INTO {0} (", this.tableName);

                // append valuedColumn names
                for (int i = 0; i < this.ValuedColumns.Length; i++)
                {
                    string valuedColumnName = this.ValuedColumns[i].Name;
                    queryStringBuilder.AppendFormat("{0}", valuedColumnName);

                    if (i < this.ValuedColumns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                // append values
                queryStringBuilder.Append(") VALUES (");

                for (int i = 0; i < this.ValuedColumns.Length; i++)
                {
                    object valuedColumnValue = this.ValuedColumns[i].Value;
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
                    
                    if (i < this.ValuedColumns.Length - 1)
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