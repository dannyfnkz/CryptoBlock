using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        /// <summary>
        /// represents an SQL table column value.
        /// </summary>
        internal class ColumnValue
        {
            private const string NULL_VALUE_STRING_REPRESENTATION = "NULL";

            private readonly object value;

            internal ColumnValue(object value)
            {
                this.value = value;
            }

            public object Value
            {
                get
                {
                    // if value != null return value, else return the string representation of a null
                    // column value
                    object value = this.value ?? NULL_VALUE_STRING_REPRESENTATION;
                    return value;
                }
            }
        }
    }
}

