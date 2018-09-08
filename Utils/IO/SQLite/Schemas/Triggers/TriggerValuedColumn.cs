using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema.Triggers
    {
        /// <summary>
        /// represents a <see cref="ValuedColumn"/> used as part of a <see cref="TriggerSchema"/>.
        /// </summary>
        public class TriggerValuedColumn : ValuedColumn
        {
            /// <summary>
            /// value of <see cref="TriggerValuedColumn"/>.
            /// </summary>
            public class ValueExpression : IExpression
            {
                /// <summary>
                /// whether column value should be taken before triggering query or after.
                /// </summary>
                public enum eTime
                {
                    Old, New
                }

                private readonly string triggeredTableColumnName;
                private readonly eTime time;

                private readonly string value;

                public ValueExpression(string triggeredTableColumnName, eTime time)
                {
                    this.triggeredTableColumnName = triggeredTableColumnName;
                    this.time = time;

                    this.value = buildValue(triggeredTableColumnName, time);
                }

                string IExpression.ExpressionString
                {
                    get
                    {
                        return Value;
                    }
                }

                public string TriggeredTableColumnName
                {
                    get { return triggeredTableColumnName; }
                }

                public eTime Time
                {
                    get { return time; }
                }

                public string Value
                {
                    get { return value; }
                }

                public static string TimeToString(eTime time)
                {
                    return time.ToString().ToLower();
                }

                private static string buildValue(string triggeredTableColumnName, eTime time)
                {
                    string value = string.Format("{0}.{1}", TimeToString(time), triggeredTableColumnName);
                    return value;
                }
            }

            private readonly ValueExpression.eTime valueExpressionTime;

            public TriggerValuedColumn(
                string name,
                string triggeredTableColumnName,
                ValueExpression.eTime valueExpressionTime)
                : base(name, buildValue(triggeredTableColumnName, valueExpressionTime))
            {
                this.valueExpressionTime = valueExpressionTime;
            }

            public ValueExpression.eTime ValueExpressionTime
            {
                get { return valueExpressionTime; }
            }

            private static ValueExpression buildValue(
                string triggeredTableColumnName,
                ValueExpression.eTime valueExpressionTime)
            {
                ValueExpression valueExpression = new ValueExpression(
                    triggeredTableColumnName,
                    valueExpressionTime);
                return valueExpression;
            }
        }
    }
}
