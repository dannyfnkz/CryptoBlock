using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CryptoBlock.Utils.IO.SQLite.Xml;
using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
using static CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas.ColumnSchema;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represesnts a table <see cref="Schema"/>.
        /// </summary>
        public class TableSchema : Schema
        {
            /// <summary>
            /// thrown if <see cref="TableSchema"/> parse failed.
            /// </summary>
            public class TableSchemaParseException : SQLiteParseExcetion
            {
                public TableSchemaParseException(
                    string additionalDetails = null,
                    Exception innerException = null)
                    : base(typeof(TableSchemaParseException), additionalDetails, innerException)
                {

                }
            }

            /// <summary>
            /// thrown if specified primary key <see cref="ColumnSchema"/> does not exist
            /// in specified table <see cref="ColumnSchema"/> list.
            /// </summary>
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
                        "Table column '{0}', specified as primary key, was not in specified " +
                        "table column list.",
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

            /// <summary>
            /// whether audit should be carried out for queries performed on
            /// table corresponding to this <see cref="TableSchema"/>.
            /// </summary>
            public bool Auditable
            {
                get { return auditable; }
            }

            // throws NullReferenceException

            /// <summary>
            /// parses <see cref="TableSchema"/> from <paramref name="tableSchemaXmlNode"/>.
            /// </summary>
            /// <param name="tableSchemaXmlNode"></param>
            /// <returns>
            /// <see cref="TableSchema"/> parsed from <paramref name="tableSchemaXmlNode"/>
            /// </returns>
            /// <exception cref="TableSchemaParseException">
            /// thrown if <see cref="TableSchema"/> parse failed
            /// </exception>
            public static TableSchema Parse(XmlNode tableSchemaXmlNode)
            {
                try
                {
                    // get required name attribute
                    string tableName = tableSchemaXmlNode.GetAttributeValue("name");

                    // get optional notAuditable element
                    bool auditable = !tableSchemaXmlNode.ContainsNodes("not_auditable");

                    // get table columns
                    XmlNodeList columnSchemaXmlNodeList = tableSchemaXmlNode.GetNodes("column");

                    ColumnSchema[] columnSchemas = new ColumnSchema[columnSchemaXmlNodeList.Count];

                    for (int i = 0; i < columnSchemaXmlNodeList.Count; i++)
                    {
                        XmlNode columnSchemaXmlNode = columnSchemaXmlNodeList[i];
                        columnSchemas[i] = ColumnSchema.Parse(columnSchemaXmlNode);
                    }

                    // get primary key, if specified
                    ColumnSchema primaryKeyColumnSchema = null;

                    if (tableSchemaXmlNode.ContainsNodes("primary_key")) // primary key specified
                    {
                        int primaryKeyColumnIndex =
                                tableSchemaXmlNode.GetNodes("primary_key")[0]
                                .GetAttributeValue<int>("column_index");
                        primaryKeyColumnSchema = columnSchemas[primaryKeyColumnIndex];
                    }

                    TableSchema tableSchema = new TableSchema(
                        tableName,
                        columnSchemas,
                        primaryKeyColumnSchema,
                        auditable);

                    return tableSchema;
                }
                catch (Exception exception)
                {
                    if ( // exception while trying to parse TableSchema
                        exception is XmlNodeMissingAttributeException
                        || exception is XmlNodeMissingNodeException
                        || exception is InvalidAttributeTypeException
                        || exception is ColumnSchemaParseExcetion)
                    {
                        throw new TableSchemaParseException(null, exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }
           
            protected override string BuildExpressionString()
            {
                StringBuilder representationStringBuilder = new StringBuilder();

                // append header and table name
                representationStringBuilder.AppendFormat("TABLE {0}", this.Name);

                // start column list definition
                representationStringBuilder.Append(" (");

                // append table columns query representations
                for (int i = 0; i < this.columnSchemas.Length; i++)
                {
                    ColumnSchema tableColumn = this.ColumnSchemas[i];
                    representationStringBuilder.Append(tableColumn.ExpressionString);

                    if (i < this.ColumnSchemas.Length - 1)
                    {
                        representationStringBuilder.Append(", ");
                    }
                }

                if (this.PrimaryKeyColumnSchema != null) // append primary key if defined for this table
                {
                    representationStringBuilder.AppendFormat(
                        ", PRIMARY KEY({0})",
                        this.PrimaryKeyColumnSchema.Name);
                }

                // add column list definition
                representationStringBuilder.Append(" )");

                return representationStringBuilder.ToString();
            }
        }
    }
}