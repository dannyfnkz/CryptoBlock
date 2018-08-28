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
        public class DeleteQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly Condition queryCondition;

            public DeleteQuery(string tableName, Condition queryCondition = null)
            {
                this.tableName = tableName;
                this.queryCondition = queryCondition;
            }

            protected override string BuildQueryString()
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("DELETE FROM {0}", this.tableName);

                if (queryCondition != null)
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", this.queryCondition.QueryString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}
