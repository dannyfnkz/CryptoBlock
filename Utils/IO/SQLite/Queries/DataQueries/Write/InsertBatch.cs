using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Xml;
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
        public class InsertBatch : DataWriteQuery
        {
            private readonly string tableName;
            private readonly List<InsertQuery> insertQueryList = new List<InsertQuery>();

            private string queryString;

            public InsertBatch(string tableName, IList<InsertQuery> insertQueries)
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

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                appendQueryHeader(tableName, insertQueryList[0], queryStringBuilder);
                appendQueryBody(insertQueryList, queryStringBuilder);

                return queryStringBuilder.ToString();
            }

            private static void appendQueryHeader(
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

            public static void appendQueryBody(
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

            public static void appendInsertQuery(
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