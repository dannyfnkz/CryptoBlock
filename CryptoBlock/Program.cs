using CryptoBlock.Utils;
using CryptoBlock.Utils.IO.SqLite;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using CryptoBlock.Utils.IO.SQLite.Schema;
using CryptoBlock.Utils.IO.SQLite.Schemas;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Utils.IO.SQLite;

namespace CryptoBlock
{

    class Program
    {
        static void Main(string[] args)
        {

            new ProgramManager().StartProgram();

            //SQLiteDatabaseHandler handler = new SQLiteDatabaseHandler(
            //    "temp123.sqlite");
            //handler.OpenConnection();

            //TableSchema tableSchema = new TableSchema(
            //    "Customer",
            //    new ColumnSchema[]
            //    {
            //        ColumnSchema.IdColumnSchema,
            //        new VarcharColumnSchema("name", 50, true)
            //    },
            //    0);

            //handler.CreateTable(tableSchema);

            //handler.ExecuteInsertQuery(new InsertQuery(tableSchema.Name,
            //    new ValuedColumn[]
            //    {
            //        new ValuedColumn("name", "yos")
            //    }));

            //handler.DropTable(new DropTableQuery(tableSchema.Name));

            //handler.CloseConnection();
        }


    }
}
