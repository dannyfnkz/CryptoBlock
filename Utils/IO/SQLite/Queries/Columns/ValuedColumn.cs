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
            public class NamesAndValuesCountMismatchException : MismatchException
            {
                public NamesAndValuesCountMismatchException()
                    : base("names.Length", "values.Length")
                {

                }
            }

            private readonly ColumnValue columnValue;

            public ValuedColumn(string name, object value)
                : base(name)
            {
                this.columnValue = new ColumnValue(value);
            }

            public object Value
            {
                get { return columnValue.Value; }
            }

            public static ValuedColumn Parse(XmlNode ValuedColumnXmlNode)
            {
                string columnName = ValuedColumnXmlNode.Attributes["name"].Value;
                object columnValue = ValuedColumnXmlNode.SelectNodes("value")[0].FirstChild.Value;

                ValuedColumn valuedColumn = new ValuedColumn(columnName, columnValue);

                return valuedColumn;
            }

            public static ValuedColumn[] Parse(IList<string> columnNames, IList<object> columnValues)
            {               
                assertColumnNamesAndValuesCountMatches(columnNames, columnValues);

                ValuedColumn[] parsedValuedColumns = new ValuedColumn[columnNames.Count];

                for (int i = 0; i < parsedValuedColumns.Length; i++)
                {
                    parsedValuedColumns[i] = new ValuedColumn(columnNames[i], columnValues[i]);
                }

                return parsedValuedColumns;
            }

            private static void assertColumnNamesAndValuesCountMatches(
                IList<string> columnNames,
                IList<object> columnValues)
            {
                if(columnNames.Count != columnValues.Count)
                {
                    throw new NamesAndValuesCountMismatchException();
                }
            }
        }
    }

}
