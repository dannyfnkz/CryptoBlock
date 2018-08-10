using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class ValuedColumn : Column
        {
            private const string NULL_VALUE_STRING_REPRESENTATION = "NULL";

            private readonly object value;

            public ValuedColumn(string name, object value)
                : base(name)
            {
                this.value = value;
            }

            public object Value
            {
                get
                {
                    object value = this.value ?? NULL_VALUE_STRING_REPRESENTATION;
                    return value;
                }
            }

            public static ValuedColumn Parse(XmlNode ValuedColumnXmlNode)
            {
                string columnName = ValuedColumnXmlNode.Attributes["name"].Value;
                object columnValue = ValuedColumnXmlNode.SelectNodes("value")[0].FirstChild.Value;

                ValuedColumn valuedColumn = new ValuedColumn(columnName, columnValue);

                return valuedColumn;
            }
        }
    }

}
