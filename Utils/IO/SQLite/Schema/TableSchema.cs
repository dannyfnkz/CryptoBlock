using System;
using System.Text;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class TableSchema : ISchema
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

            private readonly string tableName;
            private readonly ColumnSchema[] columnSchemas;
            private readonly ColumnSchema primaryKeyColumnSchema;

            private string queryString;

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

            public string QueryString
            {
                get
                { 
                    if(this.queryString == null)
                    {
                        this.queryString = buildQueryString(
                            this.tableName,
                            this.columnSchemas,
                            this.primaryKeyColumnSchema);
                    }

                    return this.queryString;
                }
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

            // throws NullReferenceException
            public static TableSchema Parse(XmlNode tableSchemaXmlNode)
            {
                // get table name
                string tableName = tableSchemaXmlNode.Attributes["name"].Value;

                // get table columns
                XmlNodeList columnSchemaXmlNodeList = tableSchemaXmlNode.SelectNodes("column");

                if(columnSchemaXmlNodeList.Count == 0) // table has no columns
                {
                    // table must have at least one column
                    string exceptionMessage = string.Format(
                        "(Table '{0}') Table cannot have less than one column",
                        tableName);
                    throw new XmlNodeParseException(exceptionMessage);
                }

                ColumnSchema[] columnSchemas = new ColumnSchema[columnSchemaXmlNodeList.Count];

                for(int i = 0; i < columnSchemaXmlNodeList.Count; i++)
                {
                    XmlNode columnSchemaXmlNode = columnSchemaXmlNodeList[i];
                    columnSchemas[i] = ColumnSchema.Parse(columnSchemaXmlNode);
                }

                // get primary key, if specified
                ColumnSchema primaryKeyColumnSchema = null;

                if(tableSchemaXmlNode.SelectNodes("primary_key").Count > 0) // primary key specified
                {
                    int primaryKeyColumnIndex =
                        int.Parse(
                            tableSchemaXmlNode.SelectNodes("primary_key")[0]
                            .Attributes["column_index"].Value);
                    primaryKeyColumnSchema = columnSchemas[primaryKeyColumnIndex];
                }

                TableSchema tableSchema = new TableSchema(
                    tableName,
                    columnSchemas,
                    primaryKeyColumnSchema);

                return tableSchema;
            }

            private static string buildQueryString(
                string tableName,
                ColumnSchema[] columnSchemas,
                ColumnSchema primaryKeyColumnSchema)
            {
                StringBuilder representationStringBuilder = new StringBuilder();

                representationStringBuilder.AppendFormat("TABLE {0} (", tableName);

                // append table columns query representations
                for (int i = 0; i < columnSchemas.Length; i++)
                {
                    ColumnSchema tableColumn = columnSchemas[i];
                    representationStringBuilder.Append(tableColumn.QueryString);

                    if (i < columnSchemas.Length - 1)
                    {
                        representationStringBuilder.Append(", ");
                    }
                }

                if (primaryKeyColumnSchema != null) // append primary key if defined for this table
                {
                    representationStringBuilder.AppendFormat(", PRIMARY KEY({0})", primaryKeyColumnSchema.Name);
                }

                representationStringBuilder.Append(" )");

                return representationStringBuilder.ToString();
            }
        }
    }

}