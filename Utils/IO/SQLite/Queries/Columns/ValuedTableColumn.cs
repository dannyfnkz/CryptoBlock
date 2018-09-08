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
        /// represents a <see cref="TableColumn"/> having an associated value.
        /// </summary>
        public class ValuedTableColumn : TableColumn
        {
            private readonly ColumnValue columnValue;

            public ValuedTableColumn(string name, string tableName, object value)
                : base(name, tableName)
            {
                this.columnValue = new ColumnValue(value);
            }

            public object Value
            {
                get { return columnValue.Value; }
            }
        }
    }
}
