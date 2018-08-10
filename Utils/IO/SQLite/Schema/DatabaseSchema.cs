using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class DatabaseSchema : ISchema
        {
            private readonly string databaseName;
            private readonly TableSchema[] tableSchemas;

            private string queryString;

            public DatabaseSchema(string databaseName, TableSchema[] tableSchemas)
            {
                this.databaseName = databaseName;
                this.tableSchemas = tableSchemas;
            }

            public string QueryString
            {
                get
                {
                    if(this.queryString == null)
                    {
                        this.queryString = buildQueryString(this.databaseName);
                    }

                    return this.queryString;
                }
            }

            // throws NullReferenceException
            public static DatabaseSchema Parse(XmlNode databaseSchemaXmlNode)
            {
                // get database name
                string databaseName = databaseSchemaXmlNode.Attributes["name"].Value;

                // get database tables
                XmlNodeList tableSchemaXmlNodeList = databaseSchemaXmlNode.SelectNodes("table");
                TableSchema[] tableSchemas = new TableSchema[tableSchemaXmlNodeList.Count];

                for (int i = 0; i < tableSchemaXmlNodeList.Count; i++)
                {
                    XmlNode tableSchemaXmlNode = tableSchemaXmlNodeList[i];
                    tableSchemas[i] = TableSchema.Parse(tableSchemaXmlNode);
                }

                DatabaseSchema databaseSchema = new DatabaseSchema(databaseName, tableSchemas);

                return databaseSchema;
            }

            public string DatabaseName
            {
                get { return databaseName; }
            }

            public TableSchema[] TableSchemas
            {
                get { return tableSchemas; }
            }

            private static string buildQueryString(string databaseName)
            {
                string queryString = string.Format("DATABASE {0}", databaseName);

                return queryString;
            }
        }
    }
}

