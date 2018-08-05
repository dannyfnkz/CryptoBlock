using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Conditions
    {
        public class ComplexCondition : Condition
        {
            public enum eConditionType
            {
                And, Or
            }

            private static readonly Dictionary<eConditionType, string> eComparisonTypeToString =
                new Dictionary<eConditionType, string>()
            {
                    { eConditionType.And, "AND" },
                    { eConditionType.Or, "OR" },
            };

            private Condition leftCondition;
            private eConditionType conditionType;
            private Condition rightCondition;
            private string queryString;

            public ComplexCondition(
                Condition leftCondition,
                Condition rightCondition,
                eConditionType conditionType)
            {
                this.leftCondition = leftCondition;
                this.rightCondition = rightCondition;
                this.conditionType = conditionType;

                this.queryString = buildQueryString();
            }

            public Condition LeftCondition
            {
                get { return leftCondition; }
            }

            public eConditionType ConditionType
            {
                get { return conditionType; }
            }

            public Condition RightCondition
            {
                get { return rightCondition; }
            }

            string Condition.QueryString
            {
                get { return queryString; }
            }

            public static string ConditionTypeToString(eConditionType conditionType)
            {
                return eComparisonTypeToString[conditionType];
            }

            private string buildQueryString()
            {
                string leftConditionQueryString = this.leftCondition.QueryString;
                string conditionTypeString = ConditionTypeToString(this.conditionType);
                string rightConditionQueryString = this.rightCondition.QueryString;

                return string.Format(
                    "({0} {1} {2})",
                    leftConditionQueryString,
                    conditionTypeString,
                    rightConditionQueryString);
            }
        }
    }
}