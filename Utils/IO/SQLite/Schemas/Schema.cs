namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represents an SQLite schema.
        /// </summary>
        public abstract class Schema : IExpression
        {
            private string expressionString;

            /// <summary>
            /// string representation of the schema, as it appears 
            /// </summary>
            public string ExpressionString
            {
                get
                {
                    if(expressionString == null)
                    {
                        expressionString = BuildExpressionString();
                    }

                    return expressionString;
                }
            }

            protected abstract string BuildExpressionString();
        }
    }
}