using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;
using CryptoBlock.Utils.IO.SQLite.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        public class TableSchema : Schema
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

            private readonly string name;
            private readonly ColumnSchema[] columnSchemas;
            private readonly ColumnSchema primaryKeyColumnSchema;

            private bool auditable;

            private TableSchema auditTableSchema;

            public TableSchema(
                string name,
                ColumnSchema[] columnSchemas,
                int primaryKeyColumnSchemaIndex,
                bool auditable = true)
                : this(
                      name,
                      columnSchemas,
                      columnSchemas[primaryKeyColumnSchemaIndex],
                      auditable)
            {

            }

            public TableSchema(
                string name,
                ColumnSchema[] columnSchemas,
                ColumnSchema primaryKeyColumnSchema = null,
                bool auditable = true)
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

                this.name = name;
                this.columnSchemas = columnSchemas;
                this.primaryKeyColumnSchema = primaryKeyColumnSchema;

                this.auditable = auditable;
            }

            public string Name
            {
                get { return name; }
            }

            public ColumnSchema[] ColumnSchemas
            {
                get { return columnSchemas; }
            }

            public ColumnSchema PrimaryKeyColumnSchema
            {
                get { return primaryKeyColumnSchema; }
            }

            public TableSchema AuditTableSchema
            {
                get
                {
                    if (auditTableSchema == null)
                    {
                        auditTableSchema = AuditUtils.GetAuditTableSchema(this);
                    }

                    return auditTableSchema;
                }
            }

            public bool Auditable
            {
                get { return auditable; }
            }

            // throws NullReferenceException
            public static TableSchema Parse(XmlNode tableSchemaXmlNode)
            {
                // get required name attribute
                string tableName = tableSchemaXmlNode.Attributes["name"].Value;

                // get optional notAuditable element
                bool auditable = !tableSchemaXmlNode.ContainsElement("not_auditable");

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
                    primaryKeyColumnSchema,
                    auditable);

                return tableSchema;
            }
           
            protected override string BuildQueryString()
            {
                StringBuilder representationStringBuilder = new StringBuilder();

                // append header and table name
                representationStringBuilder.AppendFormat("TABLE {0}", Name);

                // start column list definition
                representationStringBuilder.Append(" (");

                // append table columns query representations
                for (int i = 0; i < this.columnSchemas.Length; i++)
                {
                    ColumnSchema tableColumn = this.ColumnSchemas[i];
                    representationStringBuilder.Append(tableColumn.QueryString);

                    if (i < this.ColumnSchemas.Length - 1)
                    {
                        representationStringBuilder.Append(", ");
                    }
                }

                if (this.PrimaryKeyColumnSchema != null) // append primary key if defined for this table
                {
                    representationStringBuilder.AppendFormat(", PRIMARY KEY({0})", this.PrimaryKeyColumnSchema.Name);
                }

                // add column list definition
                representationStringBuilder.Append(" )");

                return representationStringBuilder.ToString();
            }
        }
    }
}