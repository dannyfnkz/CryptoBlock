using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries
    {
        /// <summary>
        /// represents an SQLite query.
        /// </summary>
        public abstract class Query : IExpression
        {
            public enum eQueryType
            {
                Select, Insert, Update, Delete
            }

            private string queryString;

            string IExpression.ExpressionString
            {
                get
                {
                    return QueryString;
                }
            }

            /// <summary>
            /// string representation of queryץ
            /// </summary>
            public string QueryString
            {
                get
                {
                    if(queryString == null)
                    {
                        queryString = BuildQueryString();
                    }

                    return queryString;
                }
            }

            protected abstract string BuildQueryString();

            public static string QueryTypeToString(eQueryType queryType)
            {
                string queryTypeString = queryType.ToString().ToUpper();
                return queryTypeString;
            }
        }
    }
}