using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class ValuedColumn : Column
        {
            private readonly object value;

            public ValuedColumn(string name, object value)
                : base(name)
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
