using CryptoBlock.Utils.IO.SQLite.Schemas;
using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        /// <summary>
        /// <see cref="DatabaseSchema"/> data for a generic database.
        /// </summary>
        internal static class DatabaseStructure
        {
            internal static readonly string ID_COLUMN_NAME = ColumnSchema.IdColumnSchema.Name;

            /// <summary>
            /// <see cref="TableSchema"/> data for the master table in a generic database.
            /// </summary>
            internal static class MasterTableStructure
            {
                internal static readonly string TABLE_NAME = "sqlite_master";

                internal static readonly string NAME_COLUMN_NAME = "name";
            }

            /// <summary>
            /// <see cref="TableSchema"/> data for the QueryType table in a generic database.
            /// </summary>
            internal static class QueryTypeTableStructure
            {
                internal static readonly string TABLE_NAME = "QueryType";
               
                internal static readonly string NAME_COLUMN_NAME = "name";
            }
        }
    }
}
