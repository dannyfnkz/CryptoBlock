using CryptoBlock.Utils.IO.SQLite.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.SchemaQueries.Write
    {
        public class CreateTableQuery : SchemaWriteQuery
        {
            private readonly TableSchema tableSchema;

            public CreateTableQuery(TableSchema tableSchema)
            {
                this.tableSchema = tableSchema;
            }

            public TableSchema TableSchema
            {
                get { return tableSchema; }
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.Append("CREATE ");

                // append table schema query string
                queryStringBuilder.Append(this.tableSchema.QueryString);

                return queryStringBuilder.ToString();
            }
        }
    }
}