using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
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
        public class DeleteQuery : Query
        {
            private readonly string tableName;
            private readonly Condition queryCondition;

            private readonly string queryString;

            public DeleteQuery(string tableName, Condition queryCondition = null)
            {
                this.tableName = tableName;
                this.queryCondition = queryCondition;

                this.queryString = buildQueryString(tableName, queryCondition);
            }

            public override string QueryString
            {
                get { return queryString; }
            }

            private static string buildQueryString(string tableName, Condition queryCondition)
            {
                // build sql insert query 
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.AppendFormat("DELETE FROM {0}", tableName);

                if(queryCondition != null)
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", queryCondition.QueryString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}
