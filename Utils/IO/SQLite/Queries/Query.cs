using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.IO.SQLite.Queries
{
    public abstract class Query
    {
        public abstract string QueryString { get; }
    }
}
