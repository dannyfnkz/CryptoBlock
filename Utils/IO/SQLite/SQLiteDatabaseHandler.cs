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
        public class SQLiteDatabaseHandler : IDisposable
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

            public class InvalidTransactionHandleException : SQLiteDatabaseHandlerException
            {
                private readonly ulong transactionHandle;

                public InvalidTransactionHandleException(string databaseFilePath, ulong transactionHandle)
                    : base(databaseFilePath, formatExceptionMessage(transactionHandle))
                {
                    this.transactionHandle = transactionHandle;
                }

                public ulong TransactionHandle
                {
                    get { return transactionHandle; }
                }

                private static string formatExceptionMessage(ulong transactionHandle)
                {
                    return string.Format(
                        "Transaction handle '{0}' is invalid.",
                        transactionHandle);
                }
            }

            private const string SQLITE_FILE_EXTENSION = ".sqlite";

            private readonly string filePath;

            private SQLiteConnection connection;

            private SQLiteTransaction currentUnderwayTransaction;
            private ulong currentTransactionHandle;
            private bool rollbackTransactionOnException;

            public SQLiteDatabaseHandler(string filePath, bool createNewFile = false)
            {
                this.filePath = filePath;

                if(createNewFile)
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

            public string FilePath
            {
                get { return filePath; }
            }

            public bool ConnectionOpen
            {
                get { return this.connection != null; }
            }

            public bool TransactionUnderway
            {
                get { return this.currentUnderwayTransaction != null; }
            }

            public bool RollbackTransactionOnException
            {
                get { return rollbackTransactionOnException; }
                set { rollbackTransactionOnException = value; }
            }

            public void Dispose()
            {
                if(ConnectionOpen)
                {
                    CloseConnection();
                }
            }

            public void InitializeDatabaseSchema(FileXmlDocument databaseSchemaXmlDocument)
            {
                assertConnectionToDatabaseOpen("InitializeDatabaseSchema");

                try
                {
                    DatabaseSchema databaseSchema = XMLParser.ParseDatabaseSchema(databaseSchemaXmlDocument);

                    ulong transactionHandle =
                        BeginTransactionIfNotAlreadyUnderway(out bool transactionStarted);

                    foreach (TableSchema tableSchema in databaseSchema.TableSchemas)
                    {
                        CreateTable(tableSchema);
                    }

                    CommitTransactionIfStartedByCaller(transactionHandle, transactionStarted);
                }
                catch (XmlDocumentParseException xmlDocumentParseException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, xmlDocumentParseException);
                }
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
                    onExceptionThrown();
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

                    // connection is not actually closed until the garbage collector releases
                    // the SQLiteConnectionHandle
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    connection = null;
                }
                catch (SQLiteException sqliteException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public ulong BeginTransaction()
            {
                assertTransactionNotAlreadyUnderway();

                this.currentUnderwayTransaction = connection.BeginTransaction();

                ulong transactionHandle = getNewTransactionHandle();

                return transactionHandle;
            }

            public ulong BeginTransactionIfNotAlreadyUnderway(out bool startNewTransaction)
            {
                ulong transactionHandle = 0;

                startNewTransaction = !TransactionUnderway;

                if(startNewTransaction)
                {
                    transactionHandle = BeginTransaction();
                }

                return transactionHandle;
            }

            public void CommitTransaction(ulong transactionHandle)
            {
                assertTransactionStarted("CommitTransaction");
                assertValidTransactionHandle(transactionHandle);

                this.currentUnderwayTransaction.Commit();
                this.currentUnderwayTransaction = null;
            }

            public bool CommitTransactionIfStartedByCaller(
                ulong transactionHandle,
                bool transactionStartedByCaller)
            {
                assertTransactionStarted("CommitTransactionIfStartedByCaller");

                bool commitTransaction = transactionStartedByCaller;

                if(commitTransaction)
                {
                    CommitTransaction(transactionHandle);
                }

                return commitTransaction;
            }

            public void RollbackTransaction(ulong transactionHandle)
            {
                assertTransactionStarted("RollbackTransaction");
                assertValidTransactionHandle(transactionHandle);

                this.currentUnderwayTransaction.Rollback();
                this.currentUnderwayTransaction = null;
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

            public int DropTable(string tableName)
            {
                // build query string
                string queryString = string.Format("DROP TABLE {0}", tableName);
             
                // execute query
                int numOfRowsAffected = executeWriteQuery(queryString);

                return numOfRowsAffected;
            }

            public int TruncateTable(string tableName)
            {
                assertConnectionToDatabaseOpen("TruncateTable");

                // build query string
                string queryString = string.Format("DELETE FROM {0}", tableName);

                // execute query
                int numOfRowsAffected = executeWriteQuery(queryString);

                return numOfRowsAffected;
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
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                try
                {
                    int numAffectedRows;

                    // parse InsertBatch from tableDataXmlDocument
                    InsertBatch insertBatch = XMLParser.ParseInsertBatch(tableDataXmlDocument);

                    // execute insertBatch query
                    numAffectedRows = ExecuteInsertQuery(insertBatch);

                    return numAffectedRows;
                }
                catch(XmlDocumentParseException xmlDocumentParseException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, xmlDocumentParseException);
                }
            }

            public int ExecuteInsertQuery(InsertBatch insertBatch)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                int numAffectedRows = executeWriteQuery(insertBatch.QueryString);

                return numAffectedRows;
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

            public void ExecuteWithTransaction(Action databaseAction)
            {
                ulong transactionHandle = BeginTransactionIfNotAlreadyUnderway(out bool transactionStarted);

                databaseAction.Invoke();

                CommitTransactionIfStartedByCaller(transactionHandle, transactionStarted);
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
                    onExceptionThrown();
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
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            private ulong getNewTransactionHandle()
            {
                return ++this.currentTransactionHandle;
            }

            private void assertTransactionNotAlreadyUnderway()
            {
                if(TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionAlreadyUnderwayException(this.filePath);
                }
            }

            private void assertTransactionStarted(string operationName)
            {
                if(!TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionNotStartedException(this.filePath, operationName);
                }
            }

            private void assertValidTransactionHandle(ulong transactionHandle)
            {
                if(transactionHandle != this.currentTransactionHandle)
                {
                    onExceptionThrown();
                    throw new InvalidTransactionHandleException(this.filePath, transactionHandle);
                }
            }

            private void assertConnectionToDatabaseNotAlreadyOpen(string operationName)
            {
                if (ConnectionOpen)
                {
                    onExceptionThrown();
                    throw new ConnectionToDatabaseAlreadyOpenException(filePath, operationName);
                }
            }

            private void assertConnectionToDatabaseOpen(string operationName)
            {
                if (!ConnectionOpen)
                {
                    onExceptionThrown();
                    throw new ConnectionToDatabaseNotOpenException(filePath, operationName);
                }
            }

            private void onExceptionThrown()
            {
                if(TransactionUnderway && RollbackTransactionOnException)
                {
                    ulong underwayTransactionHandle = this.currentTransactionHandle - 1;
                    RollbackTransaction(underwayTransactionHandle);
                }
            }
        }

    }
}