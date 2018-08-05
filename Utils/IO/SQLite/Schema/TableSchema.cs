using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class TableSchema
        {
            public class PrimaryKeyColumnSchemaNotInColumnSchemaListException : Exception
            {
                private ColumnSchema primaryKeyColumnSchema;

                public PrimaryKeyColumnSchemaNotInColumnSchemaListException(ColumnSchema primaryKeyColumnSchema)
                    : base(formatExceptionMessage(primaryKeyColumnSchema.Name))
                {
                    this.primaryKeyColumnSchema = primaryKeyColumnSchema;
                }

                public ColumnSchema PrimaryKeyColumnSchema
                {
                    get { return primaryKeyColumnSchema; }
                }

                private static string formatExceptionMessage(string columnName)
                {
                    return string.Format(
                        "Table column '{0}', specified as primary key, was not in table column list given as argument",
                        columnName);
                }
            }

            private string tableName;
            private ColumnSchema[] columnSchemas;
            private ColumnSchema primaryKeyColumnSchema;

            public TableSchema(
                string tableName,
                ColumnSchema[] columnSchemas,
                ColumnSchema primaryKeyColumnSchema = null)
            {
                if(primaryKeyColumnSchema != null)
                {
                    // try to find primaryKeyColumnSchema in tableColumns array
                    int primaryKeyIndexInColumnSchemaArray =
                        Array.FindIndex(columnSchemas, x => x == primaryKeyColumnSchema);

                    if(primaryKeyIndexInColumnSchemaArray == -1) // primaryKeyColumnSchema not found
                    {
                        throw new PrimaryKeyColumnSchemaNotInColumnSchemaListException(primaryKeyColumnSchema);
                    }
                }

                this.tableName = tableName;
                this.columnSchemas = columnSchemas;
                this.primaryKeyColumnSchema = primaryKeyColumnSchema;  
            }

            public string TableName
            {
                get { return tableName; }
            }

            public ColumnSchema[] ColumnSchemas
            {
                get { return columnSchemas; }
            }

            public ColumnSchema PrimaryKeyColumnSchema
            {
                get { return primaryKeyColumnSchema; }
            }

            public string GetQueryString()
            {
                StringBuilder representationStringBuilder = new StringBuilder();

                representationStringBuilder.AppendFormat("TABLE {0} (", tableName);

                // append table columns query representations
                for(int i = 0; i < this.columnSchemas.Length; i++)
                {
                    ColumnSchema tableColumn = this.columnSchemas[i];
                    representationStringBuilder.Append(tableColumn.GetQueryString());

                    if(i < this.columnSchemas.Length - 1)
                    {
                        representationStringBuilder.Append(", ");
                    }
                }

                if(primaryKeyColumnSchema != null) // append primary key if defined for this table
                {
                    representationStringBuilder.AppendFormat(", PRIMARY KEY({0})", primaryKeyColumnSchema.Name);
                }

                representationStringBuilder.Append(" )");

                return representationStringBuilder.ToString();
            }
        }
    }

}