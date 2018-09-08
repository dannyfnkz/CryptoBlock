using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.DataQueries.Write
    {
        /// <summary>
        ///  represents a <see cref="DataWriteQuery"/> which performs a DELETE operation.
        /// </summary>
        public class DeleteQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly ICondition queryCondition;

            public DeleteQuery(string tableName, ICondition queryCondition = null)
            {
                this.tableName = tableName;
                this.queryCondition = queryCondition;
            }

            public string TableName
            {
                get { return tableName; }
            }

            public ICondition QueryCondition
            {
                get { return queryCondition; }
            }

            protected override string BuildQueryString()
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("DELETE FROM {0}", this.TableName);

                if (queryCondition != null) // append query condition, if specified
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", this.QueryCondition.ExpressionString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}
