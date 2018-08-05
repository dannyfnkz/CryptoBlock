using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class ValuedTableColumn : TableColumn
        {
            private readonly object value;

            public ValuedTableColumn(string name, string tableName, object value)
                : base(name, tableName)
            {
                this.value = value;
            }

            public object Value
            {
                get { return value; }
            }
        }
    }

}
