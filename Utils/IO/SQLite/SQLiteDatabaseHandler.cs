using CryptoBlock.Utils.IO.SQLite;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Schema;
using CryptoBlock.Utils.IO.SQLite.Xml;
using System;
using System.Data.SQLite;
using Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;

namespace CryptoBlock
{
    namespace Utils.IO.SqLite
    {
        public class SQLiteDatabaseHandler
        {
            public class SQLiteDatabaseHandlerException : Exception
            {
                private string databaseFilePath;

                public SQLiteDatabaseHandlerException(
                    string databaseFilePath,
                    string exceptionDetails = null,
                    Exception innerException = null)
                    : base(formatExceptionMessage(databaseFilePath, exceptionDetails), innerException)
                {
                    this.databaseFilePath = databaseFilePath;
                }

                public string DatabaseFilePath
                {
                    get { return databaseFilePath; }
                }

                private static string formatExceptionMessage(string databaseFilePath, string exceptionDetails = null)
                {
                    string messagePostfix = exceptionDetails != null ?
                        string.Format(": {0}", exceptionDetails)
                        : ".";

                    return string.Format(
                        "An exception occurred while performing an operation on '{0}'{1}",
                        databaseFilePath,
                        messagePostfix);
                }
            }

            public class ConnectionToDatabaseAlreadyOpenException : SQLiteDatabaseHandlerException
            {
                public ConnectionToDatabaseAlreadyOpenException(string databaseFilePath, string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be closed prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            public class ConnectionToDatabaseNotOpenException : SQLiteDatabaseHandlerException
            {
                public ConnectionToDatabaseNotOpenException(
                    string databaseFilePath,
                    string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be opened prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            public class TransactionAlreadyUnderwayException : SQLiteDatabaseHandlerException
            {
                public TransactionAlreadyUnderwayException(
                    string databaseFilePath)
                    : base(databaseFilePath, formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Cannot begin a new transaction before calling rollback or commit on existing"
                        + " one";
                }
            }

            public class TransactionNotStartedException : SQLiteDatabaseHandlerException
            {
                public TransactionNotStartedException(
                    string databaseFilePath,
                    string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "A transaction must be started before prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            private const string SQLITE_FILE_EXTENSION = ".sqlite";

            private readonly string filePath;

            private SQLiteConnection connection;
            private bool transactionUnderway;

            public SQLiteDatabaseHandler(string filePath, bool createNewEmptyFile = false)
            {
                this.filePath = filePath;

                if(createNewEmptyFile)
                {
                    // try creating a new db file
                    try
                    {
                        SQLiteConnection.CreateFile(filePath);
                    }
                    catch (SQLiteException sqliteException)
                    {
                        throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                    }
                }
            }

            public SQLiteDatabaseHandler(FileXmlDocument databaseSchemaXmlDocument, string filePath = null)
            {
                try
                {
                    DatabaseSchema databaseSchema = XMLParser.ParseDatabaseSchema(databaseSchemaXmlDocument);

                    if(filePath != null)
                    {
                        this.filePath = filePath;
                    }
                    else
                    {
                        this.filePath = databaseSchema.DatabaseName + SQLITE_FILE_EXTENSION;
                    }
                    
                    OpenConnection();

                    BeginTransaction();

                    foreach(TableSchema tableSchema in databaseSchema.TableSchemas)
                    {
                        CreateTable(tableSchema);
                    }

                    CommitTransaction();

                    CloseConnection();
                }
                catch (XmlDocumentParseException xmlDocumentParseException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, xmlDocumentParseException);
                }
            }

            public string FilePath
            {
                get { return filePath; }
            }

            public bool ConnectionOpen
            {
                get { return connection != null; }
            }

            public void OpenConnection()
            {
                assertConnectionToDatabaseNotAlreadyOpen("OpenConnection()");

                string connectionString = string.Format("Data Source={0}", filePath);

                // try opening an SQLite connection 
                try
                {
                    connection = new SQLiteConnection(connectionString);
                    connection.Open();
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }

            }

            public void CloseConnection()
            {
                assertConnectionToDatabaseOpen("CloseConnection()");

                // try closing SQLite connection 
                try
                {
                    connection.Close();
                    connection = null;
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public void BeginTransaction()
            {
                assertTransactionNotAlreadyUnderway();

                string queryString = "BEGIN TRANSACTION";

                executeWriteQuery(queryString);

                this.transactionUnderway = true;
            }

            public void CommitTransaction()
            {
                assertTransactionStarted("CommitTransaction");

                string queryString = "COMMIT TRANSACTION";

                executeWriteQuery(queryString);

                this.transactionUnderway = false;
            }

            public void RollbackTransaction()
            {
                assertTransactionStarted("RollbackTransaction");

                string queryString = "ROLLBACK TRANSACTION";

                executeWriteQuery(queryString);

                this.transactionUnderway = false;
            }

            public void CreateTable(TableSchema tableSchema)
            {
                // build query string
                string queryString = string.Format("CREATE {0}", tableSchema.QueryString);

                // execute query
                executeWriteQuery(queryString);             
            }

            public int ExecuteUpdateQuery(UpdateQuery updateQuery)
            {
                // execute query
                int numAffectedRows = executeWriteQuery(updateQuery.QueryString);

                return numAffectedRows;
            }

            public void DropTable(string tableName)
            {
                // build query string
                string queryString = string.Format("DROP TABLE {0}", tableName);
             
                // execute query
                executeWriteQuery(queryString);           
            }

            public void TruncateTable(string tableName)
            {
                assertConnectionToDatabaseOpen("TruncateTable");

                // build query string
                string queryString = string.Format("DELETE FROM {0}", tableName);

                // execute query
                executeWriteQuery(queryString);
            }

            public int ExecuteInsertQuery(InsertQuery insertQuery)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                // execute query
                int numAffectedRows = executeWriteQuery(insertQuery.QueryString);

                return numAffectedRows;
            }

            public int ExecuteInsertQueries(FileXmlDocument tableDataXmlDocument)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQueries");

                try
                {
                    int numAffectedRows = 0;

                    // parse InsertQueries from tableDataXmlDocument
                    InsertQuery[] insertQueries = XMLParser.ParseInsertQueries(tableDataXmlDocument);

                    // execute insert queries
                    foreach (InsertQuery insertQuery in insertQueries)
                    {
                        numAffectedRows += ExecuteInsertQuery(insertQuery);
                    }

                    return numAffectedRows;
                }
                catch(XmlDocumentParseException xmlDocumentParseException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, xmlDocumentParseException);
                }
            }

            public int ExecuteDeleteQuery(DeleteQuery deleteQuery)
            {
                assertConnectionToDatabaseOpen("ExecuteDeleteQuery");

                // execute query
                int numAffectedRows = executeWriteQuery(deleteQuery.QueryString);

                return numAffectedRows;
            }

            public ResultSet ExecuteSelectQuery(SelectQuery selectQuery)
            {
                return executeReadQuery(selectQuery.QueryString);
            }

            private int executeWriteQuery(string query)
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(query, connection);

                    return command.ExecuteNonQuery();
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            private ResultSet executeReadQuery(string query)
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    SQLiteDataReader sqliteDataReader = command.ExecuteReader();

                    ResultSet resultSet = new ResultSet(sqliteDataReader);

                    return resultSet;
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            private void assertTransactionNotAlreadyUnderway()
            {
                if(transactionUnderway)
                {
                    throw new TransactionAlreadyUnderwayException(this.filePath);
                }
            }

            private void assertTransactionStarted(string operationName)
            {
                if(!transactionUnderway)
                {
                    throw new TransactionNotStartedException(this.filePath, operationName);
                }
            }

            private void assertConnectionToDatabaseNotAlreadyOpen(string operationName)
            {
                if (ConnectionOpen)
                {
                    throw new ConnectionToDatabaseAlreadyOpenException(filePath, operationName);
                }
            }

            private void assertConnectionToDatabaseOpen(string operationName)
            {
                if (!ConnectionOpen)
                {
                    throw new ConnectionToDatabaseNotOpenException(filePath, operationName);
                }
            }
        }

    }
}