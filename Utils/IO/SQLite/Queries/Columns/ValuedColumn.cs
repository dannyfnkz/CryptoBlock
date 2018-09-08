using CryptoBlock.Utils.IO.SQLite.Xml;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
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
        /// <summary>
        /// represents a <see cref="Column"/> with an associated <see cref="ColumnValue"/>.
        /// </summary>
        public class ValuedColumn : Column
        {
            public class ValuedColumnParseException : SQLiteParseExcetion
            {
                public ValuedColumnParseException(
                    string additionalDetails = null,
                    Exception innerException = null)
                    : base(typeof(ValuedColumnParseException), additionalDetails, innerException)
                {

                }
            }

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

            /// <summary>
            /// returns a <see cref="ValuedColumn"/> parsed from <paramref name="valuedColumnXmlNode"/>.
            /// </summary>
            /// <param name="valuedColumnXmlNode"></param>
            /// <returns>
            /// <see cref="ValuedColumn"/> parsed from <paramref name="valuedColumnXmlNode"/>
            /// </returns>
            /// <exception cref="MissingAttributeException">
            /// <seealso cref="XmlNodeExtensionMethods.GetAttributeValue(XmlNode, string)"/>
            /// </exception>
            /// <exception cref="MissingNodeException">
            /// <seealso cref="XmlNodeExtensionMethods.GetNodes(XmlNode, string)"/>
            /// </exception>
            public static ValuedColumn Parse(XmlNode valuedColumnXmlNode)
            {
                try
                {
                    string columnName = valuedColumnXmlNode.GetAttributeValue("name");
                    object columnValue = valuedColumnXmlNode.GetNodes("value")[0].FirstChild.Value;

                    ValuedColumn valuedColumn = new ValuedColumn(columnName, columnValue);

                    return valuedColumn;
                }
                catch(Exception exception)
                {
                    if(
                        exception is XmlNodeMissingAttributeException
                        || exception is XmlNodeMissingNodeException) // ValuedColumn parse failed
                    {
                        throw new ValuedColumnParseException(null, exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            /// <summary>
            /// parses an array of <see cref="ValuedColumn"/>s, each having a corresponding
            /// name and value from 
            /// <paramref name="columnNames"/> and <paramref name="columnValues"/>
            /// </summary>
            /// <param name="columnNames"></param>
            /// <param name="columnValues"></param>
            /// <returns>
            /// array of <see cref="ValuedColumn"/>s, each having a corresponding
            /// name and value from 
            /// <paramref name="columnNames"/> and <paramref name="columnValues"/>
            /// </returns>
            /// <exception cref="NamesAndValuesCountMismatchException">
            /// <seealso cref="assertColumnNamesAndValuesCountMatches(IList{string}, IList{object})"/>
            /// </exception>
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

            /// <summary>
            /// asserts that <paramref name="columnNames"/> and <paramref name="columnValues"/> have
            /// the same count.
            /// </summary>
            /// <param name="columnNames"></param>
            /// <param name="columnValues"></param>
            /// <exception cref="NamesAndValuesCountMismatchException">
            /// thrown in case of <paramref name="columnNames"/> and <paramref name="columnValues"/>
            /// count mismatch
            /// </exception>
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
