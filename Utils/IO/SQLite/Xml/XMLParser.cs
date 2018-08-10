using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml
    {
        public static class XMLParser
        {
            public class XmlDocumentParseException : Exception
            {
                private readonly string filePath;

                public XmlDocumentParseException(
                    string filePath,
                    string additionalDetails = null,
                    Exception innerException = null)
                    : base(formatExceptionMessage(filePath, additionalDetails), innerException)
                {
                    this.filePath = filePath;
                }

                public string FilePath
                {
                    get { return filePath; }
                }

                private static string formatExceptionMessage(
                    string filePath,
                    string additionalDetails = null)
                {
                    string messageSuffix = additionalDetails == null
                        ? "."
                        : string.Format(": {0}.", additionalDetails);

                    return string.Format(
                        "An exception occurred while trying to parse XML file at location '{0}'{1}",
                        filePath,
                        messageSuffix);
                }
            }

            public class XmlNodeParseException : Exception
            {
                public XmlNodeParseException(string exceptionMessage, Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {

                }
            }

            public static DatabaseSchema ParseDatabaseSchema(FileXmlDocument databaseSchemaXmlDocument)
            {
                try
                {
                    XmlNode databaseSchemaXmlNode =
                        databaseSchemaXmlDocument.GetElementsByTagName("database")[0];
                    DatabaseSchema databaseSchema = DatabaseSchema.Parse(databaseSchemaXmlNode);

                    return databaseSchema;
                }
                catch(NullReferenceException nullReferenceException) // an required attribute was missing
                {
                    throw new XmlDocumentParseException(
                        databaseSchemaXmlDocument.FilePath,
                        "A required attribute was missing from XML document.",
                        nullReferenceException);
                }
                catch(XmlNodeParseException xmlNodeParseException)
                {
                    throw new XmlDocumentParseException(
                        databaseSchemaXmlDocument.FilePath,
                        null, xmlNodeParseException);
                }
            }

            public static InsertQuery[] ParseInsertQueries(FileXmlDocument tableDataXmlDocument)
            {
                try
                {
                    // get table data xml node
                    XmlNode tableDataXmlNode = tableDataXmlDocument.SelectNodes("table")[0];

                    // get table name
                    string tableName = tableDataXmlNode.Attributes["name"].Value;

                    // parse InsertQueries from row data XmlNodeList
                    XmlNodeList rowDataXmlNodeList = tableDataXmlNode.SelectNodes("row");

                    if (rowDataXmlNodeList.Count == 0) // number of rows must be greater than zero
                    {
                        string exceptionMessage = string.Format(
                            "(Table '{0}'): number of rows must be greater than zero.",
                            tableName);
                        throw new XmlNodeParseException(exceptionMessage);
                    }

                    InsertQuery[] insertQueries = new InsertQuery[rowDataXmlNodeList.Count];

                    for (int i = 0; i < rowDataXmlNodeList.Count; i++)
                    {
                        XmlNode rowDataXmlNode = rowDataXmlNodeList[i];
                        insertQueries[i] = InsertQuery.Parse(rowDataXmlNode, tableName);
                    }

                    return insertQueries;
                }
                catch (NullReferenceException nullReferenceException) // an required attribute was missing
                {
                    throw new XmlDocumentParseException(
                        tableDataXmlDocument.FilePath,
                        "A required attribute was missing from XML document.",
                        nullReferenceException);
                }
                catch (XmlNodeParseException xmlNodeParseException)
                {
                    throw new XmlDocumentParseException(
                        tableDataXmlDocument.FilePath,
                        null, xmlNodeParseException);
                }
            }
        }
    }
}