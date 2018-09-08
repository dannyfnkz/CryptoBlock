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
        /// represents a complex <see cref="ICondition"/>, consisting of two 
        /// other <see cref="ICondition"/>s.
        /// </summary>
        public class ComplexCondition : ICondition
        {
            // logical operator which glues left and rights conditions
            public enum eLogicalOperator
            {
                And, Or
            }

            private static readonly Dictionary<eLogicalOperator, string> logicalOperatorToString =
                new Dictionary<eLogicalOperator, string>()
            {
                    { eLogicalOperator.And, "AND" },
                    { eLogicalOperator.Or, "OR" },
            };

            private ICondition leftCondition;
            private eLogicalOperator logicalOperator;
            private ICondition rightCondition;

            private readonly string expressionString;

            public ComplexCondition(
                ICondition leftCondition,
                ICondition rightCondition,
                eLogicalOperator logicalOperator)
            {
                this.leftCondition = leftCondition;
                this.rightCondition = rightCondition;
                this.logicalOperator = logicalOperator;

                this.expressionString = buildExpressionString();
            }

            public string ExpressionString
            {
                get { return expressionString; }
            }

            public ICondition LeftCondition
            {
                get { return leftCondition; }
            }

            /// <summary>
            /// logical operator glueing <see cref="LeftCondition"/> and <see cref="RightCondition"/>.
            /// </summary>
            public eLogicalOperator LogicalOperator
            {
                get { return logicalOperator; }
            }

            public ICondition RightCondition
            {
                get { return rightCondition; }
            }

            public static string LogicalOperatorToString(eLogicalOperator conditionType)
            {
                return logicalOperatorToString[conditionType];
            }

            private string buildExpressionString()
            {
                string leftConditionQueryString = this.LeftCondition.ExpressionString;
                string conditionTypeString = LogicalOperatorToString(this.LogicalOperator);
                string rightConditionQueryString = this.RightCondition.ExpressionString;

                return string.Format(
                    "({0} {1} {2})",
                    leftConditionQueryString,
                    conditionTypeString,
                    rightConditionQueryString);
            }
        }
    }
}