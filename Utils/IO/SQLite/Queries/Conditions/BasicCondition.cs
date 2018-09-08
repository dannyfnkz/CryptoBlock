using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Conditions
    {
        /// <summary>
        /// represents a basic <see cref="ICondition"/>.
        /// </summary>
        public class BasicCondition : ICondition
        {
            /// <summary>
            /// type of operator used to evaluate condition.
            /// </summary>
            public enum eOperatorType
            {
                Equal, NotEqual, LargerEqual, SmallerEqual, Larger, Smaller, In, Like
            }

            private static readonly Dictionary<eOperatorType, string> operatorTypeToString =
                new Dictionary<eOperatorType, string>()
            {
                    { eOperatorType.Equal, "=" },
                    { eOperatorType.NotEqual, "<>" },
                    { eOperatorType.LargerEqual, ">=" },
                    { eOperatorType.SmallerEqual, "<=" },
                    { eOperatorType.Larger, ">" },
                    { eOperatorType.Smaller, "<" },
                    { eOperatorType.In, "IN" },
                    { eOperatorType.Like, "LIKE" }
            };

            private readonly ValuedTableColumn valuedTableColumn;
            private readonly eOperatorType operatorType;

            private readonly string expressionString;

            public BasicCondition(ValuedTableColumn valuedTableColumn, eOperatorType operatorType)
            {
                this.valuedTableColumn = valuedTableColumn;
                this.operatorType = operatorType;
                this.expressionString = buildExpressionString();
            }

            public string ExpressionString
            {
                get { return expressionString; }
            }

            public string FullyQualifiedColumnName
            {
                get { return valuedTableColumn.FullyQualifiedName; }
            }

            public eOperatorType OperatorType
            {
                get { return operatorType; }
            }

            public object ColumnValue
            {
                get { return valuedTableColumn.Value; }
            }

            public static string OperatorTypeToString(eOperatorType operatorType)
            {
                return operatorTypeToString[operatorType];
            }

            private string buildExpressionString()
            {
                string comparisonTypeString = OperatorTypeToString(this.operatorType);

                string queryString = ColumnValue is Query ?
                    string.Format(
                        "{0} {1} ({2})",
                        FullyQualifiedColumnName,
                        comparisonTypeString,
                        (ColumnValue as Query).QueryString)

                    : string.Format(
                        "{0} {1} '{2}'",
                        FullyQualifiedColumnName,
                        comparisonTypeString,
                        ColumnValue.ToString());

                return queryString;
            }
        }
    }
}