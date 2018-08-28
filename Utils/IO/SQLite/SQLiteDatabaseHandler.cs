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
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.DatabaseStructure;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;
using static Utils.IO.SQLite.ResultSet;

namespace CryptoBlock
{
    namespace Utils.IO.SqLite
    {
        public class SQLiteDatabaseHandler : IDisposable
        {
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

            public class TransactionUnderwayException : SQLiteDatabaseHandlerException
            {
                public TransactionUnderwayException(
                    string databaseFilePath,
                    string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                       "Transaction must not be underway when performing following operation: '{0}'.",
                       operationName);
                }
            }

            public class TransactionNotUnderwayException : SQLiteDatabaseHandlerException
            {
                public TransactionNotUnderwayException(
                    string databaseFilePath,
                    string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "A transaction must be underway prior to performing following operation: '{0}'.",
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
            private const string INITIAL_DATABASE_SCHEMA_FILE_PATH = "InitialDatabaseSchema.xml ";
            private const string QUERY_TYPE_TABLE_DATA_FILE_PATH = "QueryTypeTableData.xml";

            private const bool TRANSACTION_UNDO_ENABLED_DEFAULT_VALUE = true;

            private readonly string filePath;

            private SQLiteConnection connection;

            private SQLiteTransaction underwayTransaction;
            private ulong underwayTransactionHandle;
            private bool rollbackTransactionOnException;

            private Dictionary<long, Query.eType> queryTypeIdToQueryType = 
                new Dictionary<long, Query.eType>();
            private bool transactionUndoEnabled = TRANSACTION_UNDO_ENABLED_DEFAULT_VALUE;

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

            public string FilePath
            {
                get { return filePath; }
            }

            public bool ConnectionOpen
            {
                get { return this.connection != null; }
            }

            public bool TransactionUndoEnabled
            {
                get { return transactionUndoEnabled; }
            }

            public bool TransactionUnderway
            {
                get { return this.underwayTransaction != null; }
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

            public void LoadDatabaseSchema(FileXmlDocument databaseSchemaXmlDocument)
            {
                assertConnectionToDatabaseOpen("LoadDatabaseSchema");

                try
                {
                    DatabaseSchema databaseSchema = XMLParser.ParseDatabaseSchema(
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
                catch (XmlDocumentParseException xmlDocumentParseException)
                {

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

            public ulong BeginTransaction()
            {
                assertTransactionNotUnderway("BeginTransaction");

                const bool clearTableAuditData = true;
                return beginTransaction(clearTableAuditData);
            }

            public ulong BeginTransactionIfNotAlreadyUnderway(out bool newTransactionStarted)
            {
                const bool clearTableAuditData = true;

                return beginTransactionIfNotAlreadyUnderway(
                    out newTransactionStarted, 
                    clearTableAuditData);
            }

            public void CommitTransaction(ulong transactionHandle)
            {
                assertTransactionStarted("CommitTransaction");
                assertValidTransactionHandle(transactionHandle);

                this.underwayTransaction.Commit();
                this.underwayTransaction = null;
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

                this.underwayTransaction.Rollback();
                this.underwayTransaction = null;
            }

            public void UndoLastTransaction()
            {
                assertConnectionToDatabaseOpen("UndoLastTransaction");
                assertTransactionNotUnderway("UndoLastTransaction");

                List<WriteQuery> undoWriteQueries = new List<WriteQuery>();
                
                string[] auditTableNames = getAuditTableNames();

                foreach(string auditTableName in auditTableNames)
                {
                    WriteQuery[] auditTableUndoQueries = getUndoWriteQueries(auditTableName);
                    undoWriteQueries.AddRange(auditTableUndoQueries);
                }

                // undo changes made to audited tables in database since last transaction
                executeWriteQueries(undoWriteQueries);

                // clear table audit data from database
                clearTableAuditData();
            }

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
                                SelectQuery.OrderBy.TableColumn.eType.Descending)
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

            public void CreateTrigger(CreateTriggerQuery createTriggerQuery)
            {
                assertConnectionToDatabaseOpen("CreateTrigger");

                // execute query
                executeWriteQuery(createTriggerQuery);
            }

            public int UpdateTable(UpdateQuery updateQuery)
            {
                assertConnectionToDatabaseOpen("UpdateTable");

                // execute query
                int numAffectedRows = executeWriteQuery(updateQuery);

                return numAffectedRows;
            }

            // (triggers are automatically dropped)
            public int DropTable(DropTableQuery dropTableQuery)
            {
                assertConnectionToDatabaseOpen("DropTable");

                const bool dropAuditTable = true;

                int numOfRowsAffected = dropTable(dropTableQuery, dropAuditTable);

                return numOfRowsAffected;
            }

            private int dropTable(DropTableQuery dropTableQuery, bool dropAuditTable)
            {
                // execute table drop query
                int numOfRowsAffected = executeWriteQuery(dropTableQuery);

                if(dropAuditTable) 
                {
                    // remove table audit (if exists)
                    removeTableAudit(dropTableQuery);
                }

                return numOfRowsAffected;
            }

            public int InsertIntoTable(InsertQuery insertQuery)
            {
                assertConnectionToDatabaseOpen("InsertIntoTable");

                // execute query
                int numAffectedRows = executeWriteQuery(insertQuery);

                return numAffectedRows;
            }

            public int LoadTableData(FileXmlDocument tableDataXmlDocument)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                try
                {
                    int numAffectedRows;

                    // parse InsertBatch from tableDataXmlDocument
                    InsertBatch insertBatch = XMLParser.ParseInsertBatch(tableDataXmlDocument);

                    // execute insertBatch query
                    numAffectedRows = InsertIntoTable(insertBatch);

                    return numAffectedRows;
                }
                catch(XmlDocumentParseException xmlDocumentParseException)
                {
                    onExceptionThrown();
                    throw new SQLiteDatabaseHandlerException(filePath, null, xmlDocumentParseException);
                }
            }

            public int InsertIntoTable(InsertBatch insertBatch)
            {
                assertConnectionToDatabaseOpen("ExecuteInsertQuery");

                int numAffectedRows = executeWriteQuery(insertBatch);

                return numAffectedRows;
            }

            public int DeleteFromTable(DeleteQuery deleteQuery)
            {
                assertConnectionToDatabaseOpen("DeleteFromTable");

                // execute query
                int numAffectedRows = executeWriteQuery(deleteQuery);

                return numAffectedRows;
            }

            public ResultSet SelectFromTable(SelectQuery selectQuery)
            {
                assertConnectionToDatabaseOpen("SelectFromTable");

                return executeReadQuery(selectQuery);
            }

            public void ExecuteWithinTransaction(Action databaseAction)
            {
                ulong transactionHandle = BeginTransactionIfNotAlreadyUnderway(
                    out bool newTransactionStarted);

                databaseAction.Invoke();

                CommitTransactionIfStartedByCaller(transactionHandle, newTransactionStarted);
            }

            private ulong beginTransaction(bool shouldClearTableAuditData)
            {
                if (shouldClearTableAuditData)
                {
                    clearTableAuditData();
                }

                this.underwayTransaction = connection.BeginTransaction();

                ulong transactionHandle = getNewTransactionHandle();

                return transactionHandle;
            }

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

            private void clearTableAuditData()
            {
                const bool clearTableAuditData = false;
                ulong transactionHandle = beginTransactionIfNotAlreadyUnderway(
                    out bool newTransactionStarted,
                    clearTableAuditData);

                string[] auditTableNames = getAuditTableNames();

                foreach (string auditTableName in auditTableNames)
                {
                    DeleteQuery deleteQuery = new DeleteQuery(auditTableName);
                    DeleteFromTable(deleteQuery);
                }

                CommitTransactionIfStartedByCaller(transactionHandle, newTransactionStarted);
            }

            private string[] getAuditTableNames()
            {
                string[] auditTableNames;

                SelectQuery auditTableNamesSelectQuery = AuditUtils.AuditTableNamesSelectQuery;

                ResultSet auditTableNameResultSet = SelectFromTable(auditTableNamesSelectQuery);

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

            private void removeTableAudit(DropTableQuery dropTableQuery)
            {
                // remove audit table associated with tableSchema, if exists

                string auditTableName = AuditUtils.GetAuditTableName(dropTableQuery.TableName);
                const bool existsConstriant = true;
                DropTableQuery auditDropTableQuery = new DropTableQuery(auditTableName, existsConstriant);

                const bool dropAuditTable = false;

                dropTable(auditDropTableQuery, dropAuditTable);
            }

            private void initializeTableAudit(TableSchema tableSchema)
            {
                TableSchema auditTableSchema = tableSchema.AuditTableSchema;
                CreateTableQuery auditTableCreateTableQuery = new CreateTableQuery(
                    auditTableSchema);

                CreateTable(auditTableCreateTableQuery);

                // create corresponding triggers
                TriggerSchema insertTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eType.Insert);
                TriggerSchema updateTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eType.Update);
                TriggerSchema deleteTriggerSchema = AuditUtils.GetAuditTriggerSchema(
                    tableSchema,
                    auditTableSchema,
                    Query.eType.Delete);

                CreateTrigger(new CreateTriggerQuery(insertTriggerSchema));
                CreateTrigger(new CreateTriggerQuery(updateTriggerSchema));
                CreateTrigger(new CreateTriggerQuery(deleteTriggerSchema));
            }

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
                    LoadDatabaseSchema(initialDatabaseSchemaXmlDocument);
                    LoadTableData(queryTypeTableDataXmlDocument);
                    CloseConnection();
                }
                catch (Exception exception) // database file initialization failed
                {
                    onExceptionThrown();
                    Dispose(); // dispose of this SQLiteDatabaseHandler object

                    SQLiteDatabaseHandlerException sqliteDatabaseHandlerException
                        = new SQLiteDatabaseHandlerException(filePath, null, exception);
                    try
                    {
                        if(FileIOUtils.FileExists(this.filePath))
                        {
                            FileIOUtils.DeleteFile(this.filePath);
                        }
                    }
                    catch(Exception exception1)
                    {
                        throw new AggregateException(sqliteDatabaseHandlerException, exception1);
                    }

                    throw sqliteDatabaseHandlerException;
                }
            }

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
                    Query.eType queryType = EnumUtils.ParseEnum <Query.eType>(
                        queryTypeName.ToEnumNameFormat());

                    // place in dictionary
                    this.queryTypeIdToQueryType[queryTypeId] = queryType;
                }
            }

            private int executeWriteQueries(IEnumerable<WriteQuery> writeQueries)
            {
                int numAffectedRows = 0;

                foreach (WriteQuery writeQuery in writeQueries)
                {
                    numAffectedRows += executeWriteQuery(writeQuery);
                }

                return numAffectedRows;
            }

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

            private ResultSet executeReadQuery(ReadQuery readQuery)
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(readQuery.QueryString, connection);
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
                return ++this.underwayTransactionHandle;
            }

            private void assertTransactionNotUnderway(string operationName)
            {
                if(TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionUnderwayException(this.filePath, operationName);
                }
            }

            private void assertTransactionStarted(string operationName)
            {
                if(!TransactionUnderway)
                {
                    onExceptionThrown();
                    throw new TransactionNotUnderwayException(this.filePath, operationName);
                }
            }

            private void assertValidTransactionHandle(ulong transactionHandle)
            {
                if(transactionHandle != this.underwayTransactionHandle)
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
                    RollbackTransaction(this.underwayTransactionHandle);
                }
            }
        }

    }
}