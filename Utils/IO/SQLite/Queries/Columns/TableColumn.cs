using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class TableColumn : Column
        {
            private readonly string tableName;
            private readonly string fullyQualifiedName;

            public TableColumn(string name, string tableName)
                : base(name)
            {
                this.tableName = tableName;

                this.fullyQualifiedName = buildFullyQualifiedName(name, tableName);
            }

            public string TableName
            {
                get { return tableName; }
            }

            public override string FullyQualifiedName
            {
                get { return fullyQualifiedName; }
            }

            private static string buildFullyQualifiedName(string columnName, string tableName)
            {
                string fullyQualifiedName =
                    columnName == null && tableName == null ? string.Empty
                    : string.Format("{0}.{1}", tableName, columnName);

                return fullyQualifiedName;
            }
        }
    }

}
