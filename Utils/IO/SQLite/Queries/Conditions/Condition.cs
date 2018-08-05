using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Conditions
    {
        public interface Condition
        {
            string QueryString { get; }
        }
    }
}
