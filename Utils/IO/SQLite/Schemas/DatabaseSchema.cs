using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Schemas.TableSchema;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        ///  represents a database <see cref="Schema"/>.
        /// </summary>
        public class DatabaseSchema : Schema
        {
            public class DatabaseSchemaParseException : SQLiteParseExcetion
            {
                public DatabaseSchemaParseException(
                    string additionalDetails = null,
                    Exception innerException = null)
                    : base(typeof(DatabaseSchemaParseException), additionalDetails, innerException)
                {
                    
                }
            }

            private readonly string databaseName;
            private readonly TableSchema[] tableSchemas;

            public DatabaseSchema(string databaseName, TableSchema[] tableSchemas)
            {
                this.databaseName = databaseName;
                this.tableSchemas = tableSchemas;
            }

            /// <summary>
            /// parses a <see cref="DatabaseSchema"/> from <paramref name="databaseSchemaXmlNode"/>.
            /// </summary>
            /// <param name="databaseSchemaXmlNode"></param>
            /// <returns>
            ///  <see cref="DatabaseSchema"/> parsed from <paramref name="databaseSchemaXmlNode"/>
            /// </returns>
            /// <exception cref="DatabaseSchemaParseException">
            /// thrown if parsing <see cref="DatabaseSchema"/> failed
            /// </exception>
            public static DatabaseSchema Parse(XmlNode databaseSchemaXmlNode)
            {
                try
                {
                    // get database name
                    string databaseName = databaseSchemaXmlNode.ContainsAttribute("name")
                        ? databaseSchemaXmlNode.GetAttributeValue("name")
                        : null;

                    // get database tables
                    XmlNodeList tableSchemaXmlNodeList = databaseSchemaXmlNode.GetNodes("table");
                    TableSchema[] tableSchemas = new TableSchema[tableSchemaXmlNodeList.Count];

                    for (int i = 0; i < tableSchemaXmlNodeList.Count; i++)
                    {
                        XmlNode tableSchemaXmlNode = tableSchemaXmlNodeList[i];
                        tableSchemas[i] = TableSchema.Parse(tableSchemaXmlNode);
                    }

                    DatabaseSchema databaseSchema = new DatabaseSchema(databaseName, tableSchemas);

                    return databaseSchema;
                }
                catch (Exception exception)
                {
                    if ( // exception while trying to parse DatabaseSchema
                        exception is XmlNodeMissingAttributeException
                        || exception is XmlNodeMissingNodeException
                        || exception is TableSchemaParseException)
                    {
                        throw new DatabaseSchemaParseException(null, exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            public string DatabaseName
            {
                get { return databaseName; }
            }

            public TableSchema[] TableSchemas
            {
                get { return tableSchemas; }
            }

            protected override string BuildExpressionString()
            {
                string queryString = string.Format("DATABASE {0}", this.databaseName);

                return queryString;
            }
        }
    }
}

