namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        public abstract class Schema : IExpression
        {
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
        }
    }
}