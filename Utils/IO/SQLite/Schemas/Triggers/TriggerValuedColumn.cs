using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema.Triggers
    {
        public class TriggerValuedColumn : ValuedColumn
        {
            public class ValueExpression : IExpression
            {
                private readonly string triggeredTableColumnName;
                private readonly eTime time;

                private readonly string value;

                public ValueExpression(string triggeredTableColumnName, eTime time)
                {
                    this.triggeredTableColumnName = triggeredTableColumnName;
                    this.time = time;

                    this.value = getValue(triggeredTableColumnName, time);
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

                private static string getValue(string triggeredTableColumnName, eTime time)
                {
                    string value = string.Format("{0}.{1}", TimeToString(time), triggeredTableColumnName);
                    return value;
                }
            }

            public enum eTime
            {
                Old, New
            }

            private readonly eTime time;

            public TriggerValuedColumn(
                string name,
                string triggeredTableColumnName,
                eTime time)
                : base(name, getValueExpression(triggeredTableColumnName, time))
            {
                this.time = time;
            }

            public eTime Time
            {
                get { return time; }
            }

            private static ValueExpression getValueExpression(
                string triggeredTableColumnName,
                eTime time)
            {
                ValueExpression valueExpression = new ValueExpression(
                    triggeredTableColumnName,
                    time);
                return valueExpression;
            }

            public static string TimeToString(eTime time)
            {
                return time.ToString().ToLower();
            }
        }
    }
}
