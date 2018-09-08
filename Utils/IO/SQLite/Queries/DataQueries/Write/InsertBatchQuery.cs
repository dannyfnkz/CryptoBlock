using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Xml;
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
        /// thrown if <see cref="InsertBatchQuery"/> parse fails.
        /// </summary>
        public class InsertBatchQueryParseException : SQLiteParseExcetion
        {
            private static readonly Type INSERT_BATCH_TYPE = typeof(InsertBatchQuery);

            public InsertBatchQueryParseException(
                string additionalDetails = null,
                Exception innerException = null)
                : base(INSERT_BATCH_TYPE, additionalDetails)
            {

            }
        }

        /// <summary>
        /// represents an insert batch <see cref="DataWriteQuery"/>.
        /// </summary>
        public class InsertBatchQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly List<InsertQuery> insertQueryList = new List<InsertQuery>();

            public InsertBatchQuery(string tableName, IList<InsertQuery> insertQueries)
            {
                this.tableName = tableName;
                this.insertQueryList.AddRange(insertQueries);
            }

            public InsertQuery[] InsertQueries
            {
                get { return insertQueryList.ToArray(); }
            }

            public string TableName
            {
                get { return tableName; }
            }

            public int InsertQueryCount
            {
                get { return insertQueryList.Count; }
            }

            public InsertQuery GetInsertQuery(int index)
            {
                return insertQueryList[index];
            }

            /// <summary>
            /// parses <see cref="InsertBatchQuery"/> from <paramref name="tableDataXmlNode"/>.
            /// </summary>
            /// <param name="tableDataXmlNode"></param>
            /// <returns>
            /// <see cref="InsertBatchQuery"/> parsed from <paramref name="tableDataXmlNode"/>
            /// </returns>
            /// <exception cref="InsertBatchQueryParseException">
            /// thrown if <see cref="InsertBatchQuery"/> parse failed
            /// </exception>
            public static InsertBatchQuery Parse(XmlNode tableDataXmlNode)
            {
                try
                {
                    InsertBatchQuery insertBatchQuery;

                    // get table name
                    string tableName = tableDataXmlNode.GetAttributeValue("name");

                    // parse InsertQueries from row data XmlNodeList
                    XmlNodeList rowDataXmlNodeList = tableDataXmlNode.GetNodes("row");

                    InsertQuery[] insertQueries = new InsertQuery[rowDataXmlNodeList.Count];

                    for (int i = 0; i < rowDataXmlNodeList.Count; i++)
                    {
                        XmlNode rowDataXmlNode = rowDataXmlNodeList[i];
                        insertQueries[i] = InsertQuery.Parse(rowDataXmlNode, tableName);
                    }

                    insertBatchQuery = new InsertBatchQuery(tableName, insertQueries);

                    return insertBatchQuery;
                }
                catch (Exception exception)
                {
                    // exception while trying to parse InsertBatch
                    if (
                        exception is XmlNodeMissingNodeException
                        || exception is XmlNodeMissingAttributeException
                        || exception is InsertQueryParseException)
                    {
                        throw new InsertBatchQueryParseException(null, exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                appendQueryStringHeader(tableName, insertQueryList[0], queryStringBuilder);
                appendQueryStringBody(insertQueryList, queryStringBuilder);

                return queryStringBuilder.ToString();
            }

            /// <summary>
            /// appends query string header (including table name and column names) to
            /// <paramref name="queryStringBuilder"/>.
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="firstInserQuery"></param>
            /// <param name="queryStringBuilder"></param>
            private static void appendQueryStringHeader(
                string tableName,
                InsertQuery firstInserQuery,
                StringBuilder queryStringBuilder)
            {
                queryStringBuilder.AppendFormat("INSERT INTO '{0}' (", tableName);

                Column[] columns = firstInserQuery.ValuedColumns;

                for (int i = 0; i < columns.Length; i++)
                {
                    Column column = columns[i];
                    queryStringBuilder.AppendFormat("'{0}'", column.Name);

                    if (i < columns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                queryStringBuilder.Append(")");
            }

            /// <summary>
            /// appends query string body (including inserted value tuples)
            /// to <paramref name="queryStringBuilder"/>.
            /// </summary>
            /// <param name="insertQueryList"></param>
            /// <param name="queryStringBuilder"></param>
            private static void appendQueryStringBody(
                List<InsertQuery> insertQueryList,
                StringBuilder queryStringBuilder)
            {
                queryStringBuilder.Append(" VALUES ");

                for (int i = 0; i < insertQueryList.Count; i++)
                {
                    InsertQuery insertQuery = insertQueryList[i];
                    appendInsertQuery(insertQuery, queryStringBuilder);

                    if(i < insertQueryList.Count - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }
            }

            /// <summary>
            /// appends a single insert value tuple to <paramref name="queryStringBuilder"/>.
            /// </summary>
            /// <param name="insertQuery"></param>
            /// <param name="queryStringBuilder"></param>
            private static void appendInsertQuery(
                InsertQuery insertQuery,
                StringBuilder queryStringBuilder)
            {
                queryStringBuilder.Append("(");

                ValuedColumn[] valuedColumns = insertQuery.ValuedColumns;

                for(int i = 0; i < valuedColumns.Length; i++)
                {
                    ValuedColumn valuedColumn = valuedColumns[i];
                    queryStringBuilder.AppendFormat("'{0}'", valuedColumn.Value);

                    if(i < valuedColumns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                queryStringBuilder.Append(")");
            }
        }
    }
}