using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries
    {
        public abstract class Query : IExpression
        {
            public enum eType
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

            public static string QueryTypeToString(eType queryType)
            {
                string queryTypeString = queryType.ToString().ToUpper();
                return queryTypeString;
            }
        }
    }
}