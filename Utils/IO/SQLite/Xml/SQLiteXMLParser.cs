using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Write;
using CryptoBlock.Utils.IO.SQLite.Schema;
using CryptoBlock.Utils.IO.SQLite.Schemas;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents.Exceptions;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Schemas.DatabaseSchema;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml
    {
        /// <summary>
        /// contains utility methods for parsing SQLite objects from XML.
        /// </summary>
        public static class SQLiteXmlParser
        {
            /// <summary>
            /// thrown if parsing an SQLite object from <see cref="FileXmlDocument"/> failed.
            /// </summary>
            public class InvalidFileXmlDocumentException : Exception
            {
                private readonly string filePath;

                public InvalidFileXmlDocumentException(
                    FileXmlDocument databaseFileXmlDocument,
                    Exception innerException = null)
                    : base(formatExceptionMessage(databaseFileXmlDocument.FilePath), innerException)
                {
                    this.filePath = databaseFileXmlDocument.FilePath;
                }

                public string Filepath
                {
                    get { return filePath; }
                }

                private static string formatExceptionMessage(string filePath)
                {
                    return string.Format(
                        "Parsing SQLite data from XML document parsed from file at location " +
                        "'{0}' failed.",
                        filePath);
                }
            }

            /// <summary>
            /// parses <see cref="DatabaseSchema"/> from <paramref name="databaseFileXmlDocument"/>.
            /// </summary>
            /// <seealso cref="DatabaseSchema.Parse(XmlNode)"/>
            /// <param name="databaseFileXmlDocument"></param>
            /// <returns>
            /// <see cref="DatabaseSchema"/> parsed from <paramref name="databaseFileXmlDocument"/>
            /// </returns>
            /// <exception cref="InvalidFileXmlDocumentException">
            /// thrown if parsing <see cref="DatabaseSchema"/> from
            /// <paramref name="databaseFileXmlDocument"/> fails
            /// </exception>
            public static DatabaseSchema ParseDatabaseSchema(FileXmlDocument databaseFileXmlDocument)
            {
                try
                {
                    XmlNode databaseSchemaXmlNode =
                        databaseFileXmlDocument.GetElementsByTagName("database")[0];
                    DatabaseSchema databaseSchema = DatabaseSchema.Parse(databaseSchemaXmlNode);

                    return databaseSchema;
                }
                catch(DatabaseSchemaParseException databaseSchemaParseException)
                {
                    throw new InvalidFileXmlDocumentException(
                        databaseFileXmlDocument,
                        databaseSchemaParseException);
                }
            }

            /// <summary>
            /// parses an <see cref="InsertQuery"/> array from <paramref name="tableDataXmlDocument"/>.
            /// </summary>
            /// <seealso cref="ParseInsertBatchQuery(FileXmlDocument)"/>
            /// <param name="tableDataXmlDocument"></param>
            /// <returns>
            /// <see cref="InsertQuery"/> array parsed from <paramref name="tableDataXmlDocument"/>
            /// </returns>
            /// <exception cref="InvalidFileXmlDocumentException">
            /// <seealso cref="ParseInsertQueries(FileXmlDocument)"/>
            /// </exception>
            public static InsertQuery[] ParseInsertQueries(FileXmlDocument tableDataXmlDocument)
            {
                InsertQuery[] insertQueries;

                InsertBatchQuery insertBatchQuery = ParseInsertBatchQuery(tableDataXmlDocument);

                insertQueries = insertBatchQuery.InsertQueries;

                return insertQueries;
            }

            /// <summary>
            /// parses an <see cref="InsertBatchQuery"/> from <paramref name="tableDataXmlDocument"/>.
            /// </summary>
            /// <seealso cref="InsertBatchQuery.Parse(XmlNode)"/>
            /// <param name="tableDataXmlDocument"></param>
            /// <returns>
            /// <see cref="InsertBatchQuery"/> parsed from <paramref name="tableDataXmlDocument"/>
            /// </returns>
            /// <exception cref="InvalidFileXmlDocumentException">
            /// thrown if parsing <see cref="InsertBatchQuery"/> failed
            /// </exception>
            public static InsertBatchQuery ParseInsertBatchQuery(FileXmlDocument tableDataXmlDocument)
            {
                try
                {
                    InsertBatchQuery insertBatchQuery;

                    // get table data xml node
                    XmlNode tableDataXmlNode = tableDataXmlDocument.GetNodes("table")[0];

                    insertBatchQuery = InsertBatchQuery.Parse(tableDataXmlNode);

                    return insertBatchQuery;
                }
                catch(Exception exception)
                {
                    // parsing InsertBatchQuery failed
                    if (
                        exception is XmlNodeMissingNodeException
                        || exception is InsertBatchQueryParseException)
                    {
                        throw new InvalidFileXmlDocumentException(
                            tableDataXmlDocument,
                            exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }
        }
    }
}