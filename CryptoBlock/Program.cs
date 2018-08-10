using CryptoBlock.Utils;
using CryptoBlock.Utils.IO.SqLite;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using CryptoBlock.Utils.IO.SQLite.Schema;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.Queries.SelectQuery;

namespace CryptoBlock
{

    class Program
    {
        static void Main(string[] args)
        {

            new ProgramManager().StartProgram();
        }
    }
}
