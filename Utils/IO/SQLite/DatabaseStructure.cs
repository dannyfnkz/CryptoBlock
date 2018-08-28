using CryptoBlock.Utils.IO.SQLite.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        internal class DatabaseStructure
        {
            internal static readonly string ID_COLUMN_NAME = ColumnSchema.IdColumnSchema.Name;

            internal static class MasterTableStructure
            {
                internal static readonly string TABLE_NAME = "sqlite_master";

                internal static readonly string NAME_COLUMN_NAME = "name";
            }

            internal static class QueryTypeTableStructure
            {
                internal static readonly string TABLE_NAME = "QueryType";
               
                internal static readonly string NAME_COLUMN_NAME = "name";
            }
        }
    }
}
