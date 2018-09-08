namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        /// <summary>
        /// represents an SQLite expression.
        /// </summary>
        public interface IExpression
        {
            /// <summary>
            /// string representation of <see cref="IExpression"/>.
            /// </summary>
            string ExpressionString
            {
                get;
            }
        }
    }
}