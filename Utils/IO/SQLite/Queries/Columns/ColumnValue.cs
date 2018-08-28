using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
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
                    object value = this.value ?? NULL_VALUE_STRING_REPRESENTATION;
                    return value;
                }
            }
        }
    }
}

