using CryptoBlock.Utils.IO.FileIO;
using CryptoBlock.Utils.IO.SQLite;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Read;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Write;
using CryptoBlock.Utils.IO.SQLite.Queries.SchemaQueries;
using CryptoBlock.Utils.IO.SQLite.Queries.SchemaQueries.Write;
using CryptoBlock.Utils.IO.SQLite.Schemas;
using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using CryptoBlock.Utils.IO.SQLite.Xml;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.DatabaseStructure;
using static CryptoBlock.Utils.IO.SQLite.Xml.SQLiteXmlParser;
using static Utils.IO.SQLite.ResultSet;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        /// <summary>
        /// handles performing database operations on a single SQLite database local file,
        /// in a single-threaded manner.
        /// </summary>
        public class SQLiteDatabaseHandler : IDisposable
        {
            /// <summary>
            /// thrown if an exception occurs while performing a database operation.
            /// </summary>
            public class SQLiteDatabaseHandlerException : Exception
            {
                private readonly string databaseFilePath;

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

            /// <summary>
            /// thrown if a requested operation could not be performed on database.
            /// </summary>
            public abstract class OperationException : SQLiteDatabaseHandlerException
            {
                private readonly string operationName;

                public OperationException(
                    string databaseFilePath,
                    string operationName,
                    string exceptionDetails = null,
                    Exception innerException = null)
                    : base(databaseFilePath, exceptionDetails, innerException)
                {
                    this.operationName = operationName;
                }

                public string OperationName
                {
                    get { return operationName; }
                }
            }

            /// <summary>
            /// thrown if user requests an operation which requires no connection to database
            /// to be open, while a connection is in fact open.
            /// </summary>
            public class ConnectionToDatabaseOpenException : OperationException
            {
                public ConnectionToDatabaseOpenException(string databaseFilePath, string operationName)
                    : base(operationName, databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be closed prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown if user requests an operation which requires on an open connection to database,
            /// while no connection to database is open.
            /// </summary>
            public class ConnectionToDatabaseNotOpenException : OperationException
            {
                public ConnectionToDatabaseNotOpenException(
                    string databaseFilePath,
                    string operationName)
                    : base(operationName, databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be opened prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown if user requests an operation which requires no transaction to be underway,
            /// while a transaction is in fact underway.
            /// </summary>
            public class TransactionUnderwayException : OperationException
            {
                public TransactionUnderwayException(
                    string databaseFilePath,
                    string operationName)
                    : base(operationName, databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                       "Transaction must not be underway while performing following operation: '{0}'.",
                       operationName);
                }
            }

            /// <summary>
            /// thrown if user requests a transaction-dependent operation while a transaction
            /// is not underway.
            /// </summary>
            public class TransactionNotUnderwayException : OperationException
            {
                public TransactionNotUnderwayException(
                    string databaseFilePath,
                    string operationName)
                    : base(operationName, databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "A transaction must be underway while performing following operation: '{0}'.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown when specfieid transaction handle does not match currently underway
            /// transaction handle.
            /// </summary>
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
                        "Transaction handle '{0}' does not match currently underway transaction handle.",
                        transactionHandle);
                }
            }

            /// <summary>
            /// thrown if an undoLastTransaction operation is attempted to be performed
            /// when <see cref="SQLiteDatabaseHandler.transactionUndoEnabled"/> is false.
            /// </summary>
            public class TransactionUndoNotEnabeldException : SQLiteDatabaseHandlerException
            {
                public TransactionUndoNotEnabeldException(
                    string databaseFilePath)
                    : base(databaseFilePath)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Cannot undo last transaction while TransactionUndoEnabled property " +
                        "is set to false.";
                }
            }

            private const string SQLITE_FILE_EXTENSION = ".sqlite";
            private const string INITIAL_DATABASE_SCHEMA_FILE_PATH = "InitialDatabaseSchema.xml ";

            // used to intialize queryTypeIdToQueryType dictionary
            private const string QUERY_TYPE_TABLE_DATA_FILE_PATH = "QueryTypeTableData.xml";

            private const bool TRANSACTION_UNDO_ENABLED_DEFAULT_VALUE = true;
            private const bool ROLL_BACK_TRANSACTION_ON_EXCEPTION_DEFAULT_VALUE = true;

            private readonly string filePath;

            private SQLiteConnection connection;

            private SQLiteTransaction underwayTransaction;
            private ulong underwayTransactionHandle;

            // database handler settings
            private bool rollbackTransactionOnException = ROLL_BACK_TRANSACTION_ON_EXCEPTION_DEFAULT_VALUE;
            private bool transactionUndoEnabled = TRANSACTION_UNDO_ENABLED_DEFAULT_VALUE;

            // links row ID in QueryType table in database to Query.eQueryType
            private Dictionary<long, Query.eQueryType> queryTypeIdToQueryType = 
                new Dictionary<long, Query.eQueryType>();

            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="initializeEmptyDatabase"/>
            /// </exception>
            public SQLiteDatabaseHandler(string filePath, bool createNewDatabaseFile = false)
            {
                this.filePath = filePath;

                bool databaseFileExists = FileIOUtils.FileExists(filePath);

                if(!databaseFileExists || createNewDatabaseFile)
                {
                    initializeEmptyDatabase();
                }

                initializeQueryTypeIdToQueryTypeDictionary();
            }

            /// <summary>
            /// file path of database file.
            /// </summary>
            public string FilePath
            {
                get { return filePath; }
            }


            /// <summary>
            /// whether connection to database is currently open.
            /// </summary>
            public bool ConnectionOpen
            {
                get { return this.connection != null; }
            }

            /// <summary>
            /// whether database should support undoing changes made during last transaction.
            /// </summary>
            /// <remarks>
            /// only tables which:
            /// 1. are auditable
            /// 2. were created while <see cref="TransactionUndoEnabled"/> was set to true
            /// support undoing changes made to them during last transaction. 
            /// </remarks>
            public bool TransactionUndoEnabled
            {
                get { return transactionUndoEnabled; }
            }

            /// <summary>
            /// whether a transaction is currently underway.
            /// </summary>
            public bool TransactionUnderway
            {
                get { return this.underwayTransaction != null; }
            }

            /// <summary>
            /// whether currently underway transaction should be rolled back in case
            /// an exception occurs.
            /// </summary>
            public bool RollbackTransactionOnException
            {
                get { return rollbackTransactionOnException; }
                set { rollbackTransactionOnException = value; }
            }

            /// <summary>
            /// closes connection to database (if open) 
            /// and disposes of this <see cref="SQLiteDatabaseHandler"/>.
            /// </summary>
            /// <seealso cref="CloseConnection"/>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="CloseConnection"/>
            /// </exception>
            public void Dispose()
            {
                if(ConnectionOpen)
                {
                    CloseConnection();
                }
            }

            /// <summary>
            /// loads <see cref="DatabaseSchema"/> contained in <paramref name="databaseSchemaXmlDocument"/>
            /// into database.
            /// </summary>
            /// <seealso cref="SQLiteXmlParser.ParseDatabaseSchema(FileXmlDocument)"/>
            /// <seealso cref="CreateTable(CreateTableQuery)"/>
            /// <param name="databaseSchemaXmlDocument"></param>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if loading <see cref="DatabaseSchema"/> 
            /// from <paramref name="databaseSchemaXmlDocument"/> failed
            /// </exception>
            public void LoadDatabaseSchema(FileXmlDocument databaseSchemaXmlDocument)
            {
                assertConnectionToDatabaseOpen("LoadDatabaseSchema");

                try
                {
                    DatabaseSchema databaseSchema = SQLiteXmlParser.ParseDatabaseSchema(
                        databaseSchemaXmlDocument);

                    ExecuteWithinTransaction(() =>
                    {
                        foreach (TableSchema tableSchema in databaseSchema.TableSchemas)
                        {
                            CreateTableQuery createTableQuery = new CreateTableQuery(
                                tableSchema);
                            CreateTable(createTableQuery);
                        }
                    });
                }
                catch (InvalidFileXmlDocumentException invalidFileXmlDocumentException)
                {

                    throw new SQLiteDatabaseHandlerException(
                        filePath, 
                        null,
                        invalidFileXmlDocumentException);
                }
            }

            /// <summary>
            /// opens a new connection to database.
            /// </summary>
            /// <exception cref="ConnectionToDatabaseOpenException">
            /// <seealso cref="assertConnectionToDatabaseNotOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if opening a new connection failed
            /// </exception>
            public void OpenConnection()
            {
                assertConnectionToDatabaseNotOpen("OpenConnection()");

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

            /// <summary>
            /// closes the current connection to the database.
            /// </summary>
            /// <remarks>
            /// if a transaction is currently underway, commits underway transaction before
            /// closing the connection.
            /// </remarks>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if closing the connection failed
            /// </exception>
            public void CloseConnection()
            {
                assertConnectionToDatabaseOpen("CloseConnection()");

                if (TransactionUnderway)
                {
                    CommitTransaction(this.underwayTransactionHandle);
                }

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

            /// <summary>
            /// starts a new transaction and returns its handle.
            /// </summary>
            /// <seealso cref="beginTransaction(bool)"/>
            /// <returns>
            /// handle of newly started transaction
            /// </returns>
            /// <exception cref="TransactionUnderwayException">
            /// <seealso cref="assertTransactionNotUnderway(string)"/>
            /// </exception>
            public ulong BeginTransaction()
            {
                assertTransactionNotUnderway("BeginTransaction");

                // table audit data should be cleared when user starts a new transaction
                const bool clearTableAuditData = true;
                return beginTransaction(clearTableAuditData);
            }

            /// <summary>
            /// rolls back currently underway
            /// transaction having specified <paramref name="transactionHandle"/>.
            /// </summary>
            /// <seealso cref="SQLiteTransactionBase.Rollback"/>
            /// <param name="transactionHandle"></param>
            /// <exception cref="TransactionNotUnderwayException">
            /// <seealso cref="assertTransactionUnderway(string)"/>
            /// </exception>
            /// <exception cref="InvalidTransactionHandleException">
            /// <seealso cref="assertValidTransactionHandle(ulong)"/>
            /// </exception>
            public void RollbackTransaction(ulong transactionHandle)
            {
                assertTransactionUnderway("RollbackTransaction");
                assertValidTransactionHandle(transactionHandle);

                this.underwayTransaction.Rollback();
                this.underwayTransaction = null;
            }

            /// <summary>
            /// undoes all changes made to audited tables in database during last transaction.
            /// </summary>
            /// <remarks>
            /// if no transaction was commited within the timeframe of the current connection,
            /// no changes were made during the last transaction,
            /// or this method was already called once since current last transaction was committed,
            /// does nothing.
            /// </remarks>
            /// <seealso cref="getUndoWriteQueries(string)"/>
            /// <seealso cref="clearAllTableAuditData"/>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="TransactionUnderwayException">
            /// <seealso cref="assertTransactionNotUnderway(string)"/>
            /// </exception>
            /// <exception cref="TransactionUndoNotEnabeldException">
            /// <seealso cref="assertTransactionUndoEnabled"/>
            /// </exception>
            public void UndoLastTransaction()
            {
                assertConnectionToDatabaseOpen("UndoLastTransaction");
                assertTransactionNotUnderway("UndoLastTransaction");
                assertTransactionUndoEnabled();

                List<WriteQuery> undoWriteQueries = new List<WriteQuery>();
                
                string[] auditTableNames = getAuditTableNames();

                foreach(string auditTableName in auditTableNames)
                {
                    WriteQuery[] auditTableUndoQueries = getUndoWriteQueries(auditTableName);
                    undoWriteQueries.AddRange(auditTableUndoQueries);
                }

                // undo changes made to audited tables in database during last transaction
                executeWriteQueries(undoWriteQueries);

                // clear table audit data from database
                clearAllTableAuditData();
            }

            /// <summary>
            /// executes <paramref name="createTableQuery"/>.
            /// </summary>
            /// <remarks>
            /// if table specified in <paramref name="createTableQuery"/> is <seealso cref="TableSchema.Auditable"/>,
            /// initializes table audit apparatus for table.
            /// </remarks>
            /// <seealso cref="initializeTableAudit(TableSchema)"/>
            /// <param name="createTableQuery"></param>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception> 
            public void CreateTable(CreateTableQuery createTableQuery)
            {
                assertConnectionToDatabaseOpen("CreateTable");

                // execute query
                executeWriteQuery(createTableQuery);

                if (TransactionUndoEnabled && createTableQuery.TableSchema.Auditable)
                {
                    initializeTableAudit(createTableQuery.TableSchema);
                }
            }

            /// <summary>
            /// executes <paramref name="createTriggerQuery"/>.
            /// </summary>
            /// <param name="createTriggerQuery"></param>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception> 
            public void CreateTrigger(CreateTriggerQuery createTriggerQuery)
            {
                assertConnectionToDatabaseOpen("CreateTrigger");

                // execute query
                executeWriteQuery(createTriggerQuery);
            }

            /// <summary>
            /// executes <paramref name="updateQuery"/> and returns the number of rows affected
            /// by the <see cref="Query"/> execution.
            /// </summary>
            /// <param name="updateQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="updateQuery"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            public int UpdateTable(UpdateQuery updateQuery)
            {
                assertConnectionToDatabaseOpen("UpdateTable");

                // execute query
                int numAffectedRows = executeWriteQuery(updateQuery);

                return numAffectedRows;
            }

            // (triggers are automatically dropped)

            /// <summary>
            /// executes <paramref name="dropTableQuery"/> and returns the number of rows affected
            /// by the <see cref="Query"/> execution.
            /// </summary>
            /// <seealso cref="dropTable(DropTableQuery, bool)"/>
            /// <param name="dropTableQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="dropTableQuery"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="dropTable(DropTableQuery, bool)"/>
            /// </exception>
            public int DropTable(DropTableQuery dropTableQuery)
            {
                assertConnectionToDatabaseOpen("DropTable");

                // tables dropped by user should have their audit apparatus removed
                const bool removeTableAudit = true;
                int numOfRowsAffected = dropTable(dropTableQuery, removeTableAudit);

                return numOfRowsAffected;
            }

            /// <summary>
            /// executes <paramref name="insertQuery"/> and returns the number of rows affected
            /// by the <see cref="Query"/> execution.
            /// </summary>
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// <param name="insertQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="insertQuery"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            public int InsertIntoTable(InsertQuery insertQuery)
            {
                assertConnectionToDatabaseOpen("InsertIntoTable");

                // execute query
                int numAffectedRows = executeWriteQuery(insertQuery);

                return numAffectedRows;
            }

            /// <summary>
            /// loads table data specified in <paramref name="tableDataXmlDocument"/> 
            /// into the table specified in <paramref name="tableDataXmlDocument"/>,
            /// and returns the number of rows which were inserted into the appropriate table.
            /// </summary>
            ///  <seealso cref="SQLiteXmlParser.ParseInsertBatchQuery(FileXmlDocument)"/>
            /// <seealso cref="InsertIntoTable(InsertBatchQuery)"/>
            /// <param name="tableDataXmlDocument"></param>
            /// <returns>
            /// number of rows which were inserted into table specified
            /// in <paramref name="tableDataXmlDocument"/>
            /// <paramref name="tableDataXmlDocument"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteXmlParser.ParseInsertBatchQuery(FileXmlDocument)"/>
            /// <seealso cref="InsertIntoTable(InsertBatchQuery)"/>
            /// </exception>
            public int LoadTableData(FileXmlDocument tableDataXmlDocument)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                try
                {
                    int numAffectedRows;

                    // parse InsertBatchQuery from tableDataXmlDocument
                    InsertBatchQuery insertBatchQuery = SQLiteXmlParser.ParseInsertBatchQuery(
                        tableDataXmlDocument);

                    // execute insertBatch query
                    numAffectedRows = InsertIntoTable(insertBatchQuery);

                    return numAffectedRows;
                }
                catch(InvalidFileXmlDocumentException invalidFileXmlDocumentException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(
                        filePath,
                        null,
                        invalidFileXmlDocumentException);
                }
            }

            /// <summary>
            /// executes <paramref name="insertBatchQuery"/> and returns the number of rows affected
            /// by the <see cref="Query"/> execution.
            /// </summary>
            /// <param name="insertBatchQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="insertBatchQuery"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            public int InsertIntoTable(InsertBatchQuery insertBatchQuery)
            {
                assertConnectionToDatabaseOpen("InsertIntoTable");

                int numAffectedRows = executeWriteQuery(insertBatchQuery);

                return numAffectedRows;
            }

            /// <summary>
            /// executes <paramref name="deleteQuery"/> and returns the number of rows affected
            /// by the <see cref="Query"/> execution.
            /// </summary>
            /// <param name="deleteQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="deleteQuery"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            public int DeleteFromTable(DeleteQuery deleteQuery)
            {
                assertConnectionToDatabaseOpen("DeleteFromTable");

                // execute query
                int numAffectedRows = executeWriteQuery(deleteQuery);
                
                return numAffectedRows;
            }

            /// <summary>
            /// executes <paramref name="selectQuery"/> and returns a
            /// <see cref="ResultSet"/> containing the returned rows.
            /// </summary>
            /// <param name="selectQuery"></param>
            /// <returns>
            /// <see cref="ResultSet"/> containing rows returned as result of executing
            /// <paramref name="selectQuery"/>
            /// </returns>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// <seealso cref="assertConnectionToDatabaseOpen(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeReadQuery(ReadQuery)"/>
            /// </exception>
            public ResultSet SelectFromTable(SelectQuery selectQuery)
            {
                assertConnectionToDatabaseOpen("SelectFromTable");
                
                ResultSet resultSet = executeReadQuery(selectQuery);

                return resultSet;
            }

            /// <summary>
            /// if a transaction is not already underway: begins a new transaction,
            /// invokes <paramref name="action"/>, and then 
            /// commits said transaction;
            /// otherwise simply invokes <paramref name="action"/>.
            /// </summary>
            /// <seealso cref="BeginTransactionIfNotAlreadyUnderway(out bool)"/>
            /// <seealso cref="CommitTransactionIfStartedByCaller(ulong, bool)"/>
            /// <param name="action">
            /// action which should be invoked within transaction timeframe
            /// </param>
            public void ExecuteWithinTransaction(Action action)
            {
                try
                {
                    ulong transactionHandle = BeginTransactionIfNotAlreadyUnderway(
                        out bool newTransactionStarted);

                    action.Invoke();

                    CommitTransactionIfStartedByCaller(transactionHandle, newTransactionStarted);
                }
                catch(Exception exception)
                {
                    onExceptionThrown();
                    throw exception;
                }           
            }

            /// <summary>
            /// begins a new transaction if one is not already underway; if a new transaction was started,
            /// returns its handle.
            /// </summary>
            /// <seealso cref="beginTransactionIfNotAlreadyUnderway(out bool, bool)"/>
            /// <param name="newTransactionStarted">
            /// whether a new transaction was started 
            /// </param>
            /// <returns>
            /// handle of new transaction which was started
            /// </returns>
            public ulong BeginTransactionIfNotAlreadyUnderway(out bool newTransactionStarted)
            {
                // table audit data should be cleared before before a new transaction is started
                // by a user
                const bool clearTableAuditData = true;
                ulong transactionHandle = beginTransactionIfNotAlreadyUnderway(
                    out newTransactionStarted,
                    clearTableAuditData);

                return transactionHandle;
            }

            /// <summary>
            /// commits underway transaction having <paramref name="transactionHandle"/>
            /// if <paramref name="transactionStartedByCaller"/> is set to true, and returns whether
            /// transaction was commited.
            /// </summary>
            /// <param name="transactionHandle"></param>
            /// <param name="transactionStartedByCaller"></param>
            /// <returns>
            /// true if underway transaction was commited,
            /// else false
            /// </returns>
            /// <exception cref="TransactionNotUnderwayException">
            /// <seealso cref="CommitTransaction(ulong)"/>
            /// </exception>
            /// <exception cref="InvalidTransactionHandleException">
            /// <seealso cref="CommitTransaction(ulong)"/>
            /// </exception>
            public bool CommitTransactionIfStartedByCaller(
                ulong transactionHandle,
                bool transactionStartedByCaller)
            {
                bool commitTransaction;

                if (transactionStartedByCaller)
                {
                    CommitTransaction(transactionHandle);
                    commitTransaction = true;
                }
                else
                {
                    commitTransaction = false;
                }

                return commitTransaction;
            }

            /// <summary>
            /// commits transaction having <paramref name="transactionHandle"/>.
            /// </summary>
            /// <seealso cref="SQLiteTransaction.Commit"/>
            /// <param name="transactionHandle"></param>
            /// <exception cref="TransactionNotUnderwayException">
            /// thrown if a transaction is not currently underway
            /// </exception>
            /// <exception cref="TransactionNotUnderwayException">
            /// <seealso cref="assertTransactionUnderway(string)"/>
            /// </exception>
            /// <exception cref="InvalidTransactionHandleException">
            /// <seealso cref="assertValidTransactionHandle(ulong)"/>
            /// </exception>
            public void CommitTransaction(ulong transactionHandle)
            {
                assertTransactionUnderway("CommitTransaction");
                assertValidTransactionHandle(transactionHandle);

                this.underwayTransaction.Commit();
                this.underwayTransaction = null;
            }

            /// <summary>
            /// returns array containing all undo <see cref="WriteQuery"/>s corresponding to data
            /// stored in audit table having <paramref name="auditTableName"/>.
            /// </summary>
            /// <seealso cref="AuditUtils.GetAuditedTableName(string)"/>
            /// <seealso cref="AuditUtils.GetAuditTableUndoWriteQueries(
            /// ResultSet, string, Dictionary{long, Query.eQueryType})"/>
            /// <param name="auditTableName"></param>
            /// <returns>
            /// array containing all undo <see cref="WriteQuery"/>s corresponding to data
            /// stored in audit table having <paramref name="auditTableName"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SelectFromTable(SelectQuery)"/>
            /// </exception>
            private WriteQuery[] getUndoWriteQueries(string auditTableName)
            {
                WriteQuery[] auditTableUndoQueries;

                // select all rows from audit table
                SelectQuery auditTableSelectQuery = new SelectQuery(
                    auditTableName,
                    null,
                    null,
                    null,
                    new SelectQuery.OrderBy(
                        new SelectQuery.OrderBy.TableColumn[]
                        {
                            new SelectQuery.OrderBy.TableColumn(
                                DatabaseStructure.ID_COLUMN_NAME,
                                auditTableName,
                                SelectQuery.OrderBy.TableColumn.eColumnType.Descending)
                        }
                    )
                );
                ResultSet auditTableResultSet = SelectFromTable(auditTableSelectQuery);

                // get table name of audited table corresponding to audit table
                string auditedTableName = AuditUtils.GetAuditedTableName(auditTableName);

                // get undo queries corresponding to audit table rows
                auditTableUndoQueries = AuditUtils.GetAuditTableUndoWriteQueries(
                    auditTableResultSet,
                    auditedTableName,
                    this.queryTypeIdToQueryType);

                return auditTableUndoQueries;
            }

            /// <summary>
            /// begins a new transaction, if one is not already underway, and returns the handle
            /// associated with it
            /// </summary>
            /// <seealso cref="beginTransaction(bool)"/>
            /// <param name="newTransactionStarted">
            /// whether a new transaction was started as a result of calling this method
            /// </param>
            /// <param name="shouldClearTableAuditData">
            /// whether table audit data in database should be cleared prior to starting the transaction
            /// </param>
            /// <returns>
            /// if new transaction was started, handle associated with it
            /// </returns>
            private ulong beginTransactionIfNotAlreadyUnderway(
                out bool newTransactionStarted,
                bool shouldClearTableAuditData)
            {
                ulong transactionHandle = 0;

                bool startNewTransaction = !TransactionUnderway;

                if (startNewTransaction)
                {
                    transactionHandle = beginTransaction(shouldClearTableAuditData);
                }

                newTransactionStarted = startNewTransaction;

                return transactionHandle;
            }


            /// <summary>
            /// begins a new transaction and returns the handle associated with it.
            /// </summary>
            /// <param name="shouldClearTableAuditData">
            /// whether table audit data in database should be cleared prior to starting the transaction
            /// </param>
            /// <returns>
            /// handle associated with the new transaction which was started
            /// </returns>
            private ulong beginTransaction(bool shouldClearTableAuditData)
            {
                if (shouldClearTableAuditData)
                {
                    clearAllTableAuditData();
                }

                this.underwayTransaction = connection.BeginTransaction();

                ulong transactionHandle = getNewTransactionHandle();

                return transactionHandle;
            }

            /// <summary>
            /// clears data from all audit tables in database.
            /// </summary>
            private void clearAllTableAuditData()
            {
                // begin transaction without requesting to clear table audit data
                const bool clearTableAuditData = false;
                ulong transactionHandle = beginTransactionIfNotAlreadyUnderway(
                    out bool newTransactionStarted,
                    clearTableAuditData);

                string[] auditTableNames = getAuditTableNames();

                // delete data from each audit table name in database
                foreach (string auditTableName in auditTableNames)
                {
                    DeleteQuery deleteQuery = new DeleteQuery(auditTableName);
                    DeleteFromTable(deleteQuery);
                }

                CommitTransactionIfStartedByCaller(transactionHandle, newTransactionStarted);
            }

            /// <summary>
            /// returns an array containing table names of all audit tables in database.
            /// </summary>
            /// <returns>
            /// array containing table names of all audit tables in database
            /// </returns>
            private string[] getAuditTableNames()
            {
                string[] auditTableNames;

                // select names of all audit tables in database
                SelectQuery auditTableNamesSelectQuery = AuditUtils.AuditTableNamesSelectQuery;
                ResultSet auditTableNameResultSet = SelectFromTable(auditTableNamesSelectQuery);

                // fetch audit table names from ResultSet
                auditTableNames = new string[auditTableNameResultSet.RowCount];

                for (int i = 0; i < auditTableNameResultSet.RowCount; i++)
                {
                    Row auditTableNameRow = auditTableNameResultSet.Rows[i];
                    string auditTableName = auditTableNameRow.GetColumnValue<string>(
                        MasterTableStructure.NAME_COLUMN_NAME);

                    auditTableNames[i] = auditTableName;
                }

                return auditTableNames;
            }

            /// <summary>
            /// <para>
            /// executes <paramref name="dropTableQuery"/> and returns the number of rows affected by
            /// the <see cref="Query"/> execution.
            /// </para>
            /// <para>
            /// if <paramref name="removeTableAuditFlag"/> is true,
            /// drops table audit apparatus corresponding to dropped table, if one exists.
            /// </para>
            /// </summary>
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// <seealso cref="removeTableAudit(string)"/>
            /// <param name="dropTableQuery"></param>
            /// <param name="removeTableAuditFlag">
            /// whether table audit corresponding to dropped table, if one exists, should be dropped
            /// </param>
            /// <returns>
            /// number of rows affected by executing <paramref name="dropTableQuery"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            private int dropTable(DropTableQuery dropTableQuery, bool removeTableAuditFlag)
            {
                // execute table drop query
                int numOfRowsAffected = executeWriteQuery(dropTableQuery);

                if (removeTableAuditFlag)
                {
                    // remove table audit (if exists)
                    removeTableAudit(dropTableQuery.TableName);
                }

                return numOfRowsAffected;
            }

            /// <summary>
            /// removes audit apparatus, if one exists, for table having <paramref name="tablename"/>.
            /// </summary>
            /// <param name="tableName"></param>
            private void removeTableAudit(string tableName)
            {
                // create DropTableQuery for audit table
                string auditTableName = AuditUtils.GetAuditTableName(tableName);
                const bool existsConstriant = true;
                DropTableQuery auditDropTableQuery = new DropTableQuery(auditTableName, existsConstriant);

                // drop audit table 
                const bool dropAuditTable = false;
                dropTable(auditDropTableQuery, dropAuditTable);
            }

            /// <summary>
            /// initializes audit apparatus for table having <paramref name="tableSchema"/>.
            /// </summary>
            /// <param name="tableSchema"></param>
            private void initializeTableAudit(TableSchema tableSchema)
            {
                // create audit table
                TableSchema auditTableSchema = tableSchema.AuditTableSchema;
                CreateTableQuery auditTableCreateTableQuery = new CreateTableQuery(
                    auditTableSchema);
                CreateTable(auditTableCreateTableQuery);

                // create corresponding triggers
                TriggerSchema insertTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eQueryType.Insert);
                TriggerSchema updateTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eQueryType.Update);
                TriggerSchema deleteTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eQueryType.Delete);

                CreateTrigger(new CreateTriggerQuery(insertTriggerSchema));
                CreateTrigger(new CreateTriggerQuery(updateTriggerSchema));
                CreateTrigger(new CreateTriggerQuery(deleteTriggerSchema));
            }

            /// <summary>
            /// initializes an empty database file.
            /// </summary>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if initialization of an empty database file failed
            /// </exception>
            private void initializeEmptyDatabase()
            {
                try
                {
                    // create an empty SQLite database file
                    SQLiteConnection.CreateFile(filePath);

                    // parse XML files

                    // read initial database schema from file
                    FileXmlDocument initialDatabaseSchemaXmlDocument =
                        new FileXmlDocument(INITIAL_DATABASE_SCHEMA_FILE_PATH);

                    // read QueryType enum table data from file
                    FileXmlDocument queryTypeTableDataXmlDocument =
                        new FileXmlDocument(QUERY_TYPE_TABLE_DATA_FILE_PATH);

                    OpenConnection();

                    // load initial database schema and QueryType enum table data
                    LoadDatabaseSchema(initialDatabaseSchemaXmlDocument);
                    LoadTableData(queryTypeTableDataXmlDocument);

                    CloseConnection();
                }
                catch (Exception exception) // database file initialization failed
                {
                    onExceptionThrown();
                    Dispose(); // dispose of this SQLiteDatabaseHandler object

                    // wrap exception in an SQLiteDatabaseHandlerException
                    SQLiteDatabaseHandlerException sqliteDatabaseHandlerException
                        = exception is SQLiteDatabaseHandlerException ? 
                        exception as SQLiteDatabaseHandlerException
                        : new SQLiteDatabaseHandlerException(filePath, null, exception);
                    try
                    {
                        if(FileIOUtils.FileExists(this.FilePath))
                        {
                            FileIOUtils.DeleteFile(this.FilePath);
                        }

                        throw sqliteDatabaseHandlerException;
                    }
                    catch(Exception exception1)
                    {
                        // wrap both exceptions in an AggregateException,
                        // then wrap AggregateException in an SQLiteDatabaseHandlerException
                        AggregateException aggregateException = new AggregateException(
                            exception,
                            exception1);
                        throw new SQLiteDatabaseHandlerException(this.FilePath, null, aggregateException);
                    }
                }
            }

            /// <summary>
            /// initializes <see cref="queryTypeIdToQueryType"/> dictionary.
            /// </summary>
            private void initializeQueryTypeIdToQueryTypeDictionary()
            {
                OpenConnection();

                // load data rows from QueryType Table
                SelectQuery queryTypeSelectQuery = new SelectQuery(
                    QueryTypeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ID_COLUMN_NAME,
                            QueryTypeTableStructure.TABLE_NAME),
                        new TableColumn(QueryTypeTableStructure.NAME_COLUMN_NAME,
                        QueryTypeTableStructure.TABLE_NAME)
                    });

                // execute select query
                ResultSet queryTypeResultSet = SelectFromTable(queryTypeSelectQuery);

                CloseConnection();

                // fill queryTypeIdToQueryType dictionary
                // go through all rows in QueryType table
                foreach(Row row in queryTypeResultSet.Rows)
                {
                    // get query type id and query type name from row
                    long queryTypeId = row.GetColumnValue<long>(DatabaseStructure.ID_COLUMN_NAME);
                    string queryTypeName = row.GetColumnValue<string>(
                        QueryTypeTableStructure.NAME_COLUMN_NAME);

                    // convert queryTypeName to Query.eType
                    Query.eQueryType queryType = EnumUtils.ParseEnum <Query.eQueryType>(
                        queryTypeName.MakeOnlyCharactersAtIndicesUpper(0));

                    // place in dictionary
                    this.queryTypeIdToQueryType[queryTypeId] = queryType;
                }
            }

            /// <summary>
            /// executes <paramref name="writeQueries"/> and returns the total number of rows affected by
            /// the query execution.
            /// </summary>
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// <param name="writeQueries"></param>
            /// <returns>
            /// total number of rows affected by executing <paramref name="writeQueries"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandler">
            /// <seealso cref="executeWriteQuery(WriteQuery)"/>
            /// </exception>
            private int executeWriteQueries(IEnumerable<WriteQuery> writeQueries)
            {
                int numAffectedRows = 0;

                foreach (WriteQuery writeQuery in writeQueries)
                {
                    numAffectedRows += executeWriteQuery(writeQuery);
                }

                return numAffectedRows;
            }

            /// <summary>
            /// executes <paramref name="writeQuery"/> and returns the number of rows affected by the
            /// query execution.
            /// </summary>
            /// <param name="writeQuery"></param>
            /// <returns>
            /// number of rows affected by executing <paramref name="writeQuery"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if executing <paramref name="writeQuery"/> failed
            /// </exception>
            private int executeWriteQuery(WriteQuery writeQuery)
            {
                try
                {
                    int numAffectedRows;

                    // if transaction is not already underway, start one for this query
                    ulong transactionHandle = BeginTransactionIfNotAlreadyUnderway(
                        out bool newTransactionStarted);

                    SQLiteCommand command = new SQLiteCommand(writeQuery.QueryString, connection);

                    CommitTransactionIfStartedByCaller(transactionHandle, newTransactionStarted);

                    numAffectedRows = command.ExecuteNonQuery();

                    return numAffectedRows;
                }
                catch (SQLiteException sqliteException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            /// <summary>
            /// executes <paramref name="readQuery"/>, and returns result of execution as a 
            /// <see cref="ResultSet"/>.
            /// </summary>
            /// <param name="readQuery"></param>
            /// <returns>
            /// <see cref="ResultSet"/> resulting from execution of <paramref name="readQuery"/> 
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// thrown if execution of <paramref name="readQuery"/> fails
            /// </exception>
            private ResultSet executeReadQuery(ReadQuery readQuery)
            {
                try
                {
                    // init SQLiteCommand
                    SQLiteCommand command = new SQLiteCommand(readQuery.QueryString, connection);

                    // execute command
                    SQLiteDataReader sqliteDataReader = command.ExecuteReader();

                    // init ResultSet based on SQLiteDataReader corresponding to executed command
                    ResultSet resultSet = new ResultSet(sqliteDataReader);

                    return resultSet;
                }
                catch (SQLiteException sqliteException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            /// <summary>
            /// returns a unique handle for a new transaction.
            /// </summary>
            /// <returns>
            /// unique handle for a new transaction
            /// </returns>
            private ulong getNewTransactionHandle()
            {
                return ++this.underwayTransactionHandle;
            }

            /// <summary>
            /// asserts that <see cref="TransactionUndoEnabled"/> is true.
            /// </summary>
            /// <exception cref="TransactionUndoNotEnabeldException">
            /// thrown if <see cref="TransactionUndoEnabled"/> is false
            /// </exception>
            private void assertTransactionUndoEnabled()
            {
                if(!TransactionUndoEnabled)
                {
                    throw new TransactionUndoNotEnabeldException(this.FilePath);
                }
            }

            /// <summary>
            /// asserts that a transaction is not currently underway.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="TransactionUnderwayException">
            /// thrown if a transaction is currently underway
            /// </exception>
            private void assertTransactionNotUnderway(string operationName)
            {
                if(TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionUnderwayException(this.filePath, operationName);
                }
            }

            /// <summary>
            /// asserts that a transaction is currently underway.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="TransactionNotUnderwayException">
            /// thrown if a transaction is not currently underway
            /// </exception>
            private void assertTransactionUnderway(string operationName)
            {
                if(!TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionNotUnderwayException(this.FilePath, operationName);
                }
            }

            /// <summary>
            /// asserts that <paramref name="transactionHandle"/> matches the currently underway
            /// transaction handle.
            /// </summary>
            /// <param name="transactionHandle"></param>
            /// <exception cref="InvalidTransactionHandleException">
            /// thrown if <paramref name="transactionHandle"/> does not match the current transaction
            /// handle.
            /// </exception>
            private void assertValidTransactionHandle(ulong transactionHandle)
            {
                if(transactionHandle != this.underwayTransactionHandle)
                {
                    onExceptionThrown();
                    throw new InvalidTransactionHandleException(this.filePath, transactionHandle);
                }
            }

            /// <summary>
            /// asserts that a connection to the SQLite database file is not already open.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="ConnectionToDatabaseOpenException">
            /// thrown if a connection to the SQLite database file is already open.
            /// </exception>
            private void assertConnectionToDatabaseNotOpen(string operationName)
            {
                if (ConnectionOpen)
                {
                    onExceptionThrown();
                    throw new ConnectionToDatabaseOpenException(filePath, operationName);
                }
            }

            /// <summary>
            /// aserts that a connection to the SQLite database file is open.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="ConnectionToDatabaseNotOpenException">
            /// thrown if a connection to the SQLite database file is not open
            /// </exception>
            private void assertConnectionToDatabaseOpen(string operationName)
            {
                if (!ConnectionOpen)
                {
                    onExceptionThrown();
                    throw new ConnectionToDatabaseNotOpenException(filePath, operationName);
                }
            }

            /// <summary>
            /// performs required clean up after exception is thrown within
            /// <see cref="SQLiteDatabaseHandler"/>.
            /// </summary>
            private void onExceptionThrown()
            {
                if(TransactionUnderway && RollbackTransactionOnException)
                {
                    RollbackTransaction(this.underwayTransactionHandle);
                }
            }
        }

    }
}