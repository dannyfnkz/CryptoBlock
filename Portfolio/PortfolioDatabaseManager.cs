using CryptoBlock.CMCAPI;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using CryptoBlock.Utils.IO.SQLite.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using Utils.IO.SQLite;
using static Utils.IO.SQLite.ResultSet;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Write;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Read;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents;
using CryptoBlock.Utils.IO.FileIO;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents.Exceptions;
using CryptoBlock.Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.SQLiteDatabaseHandler;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// manages <see cref="PortfolioManager"/> database operations.
        /// </summary>
        internal class PortfolioDatabaseManager
        {
            /// <summary>
            /// thrown if an exception occurred while performing a <see cref="PortfolioDatabaseManager"/>
            /// operation.
            /// </summary>
            internal class PortfolioDatabaseManagerException : Exception
            {
                internal PortfolioDatabaseManagerException(
                    string message = null,
                    Exception innerException = null)
                    : base(message, innerException)
                {

                }
            }

            /// <summary>
            /// thrown if <see cref="PortfolioDatabaseManager"/> initialization failed.
            /// </summary>
            internal class PortfolioDatabaseManagerInitializationException : PortfolioDatabaseManagerException
            {
                internal PortfolioDatabaseManagerInitializationException(
                    Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Initializing PortfolioDatabaseManager failed.";
                }
            }

            /// <summary>
            /// thrown if an undoable recently performed database action was requested to be undone,
            /// but one is not available.
            /// </summary>
            internal class UndoableLastActionNotAvailableException : PortfolioDatabaseManagerException
            {
                internal UndoableLastActionNotAvailableException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "No undoable recently performed database action available.";
                }
            }

            /// <summary>
            /// contains data regarding database structure.
            /// </summary>
            private static class DatabaseStructure
            {
                /// <summary>
                /// contains data regarding PortfolioEntry table structure.
                /// </summary>
                internal static class PortfolioEntryTableStructure
                {
                    internal static readonly string TABLE_NAME = "PortfolioEntry";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string COIN_ID_COLUMN_NAME = "coinId";
                    internal static readonly string HOLDINGS_COLUMN_NAME = "holdings";
                    internal static readonly string AVERAGE_BUY_PRICE_COLUMN_NAME = "averageBuyPrice";
                }

                /// <summary>
                /// contains data regarding TransactionType table structure.
                /// </summary>
                internal static class TransactionTypeTableStructure
                {
                    internal static readonly string TABLE_NAME = "CoinTransactionType";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string NAME_COLUMN_NAME = "name";
                }

                /// <summary>
                /// contains data regarding CoinTransaction table structure.
                /// </summary>
                internal static class CoinTransactionTableStructure
                {
                    internal static readonly string TABLE_NAME = "CoinTransaction";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string PORTFOLIO_ENTRY_ID_COLUMN_NAME = "portfolioEntryId";
                    internal static readonly string TRANSACTIO_TYPE_ID_COLUMN_NAME = "coinTransactionTypeId";
                    internal static readonly string AMOUNT_COLUMN_NAME = "amount";
                    internal static readonly string PRICE_PER_COIN_COLUMN_NAME = "pricePerCoin";
                    internal static readonly string UNIX_TIMESTAMP_COLUMN_NAME = "unixTimestamp";
                }

                internal static readonly string DATABASE_NAME = "PortfolioData";
            }

            private const string SQLite_DATABASE_FILE_PATH = "PortfolioData.sqlite";
            private const string DATABASE_SCHEMA_FILE_PATH = "DatabaseSchema.xml";
            private const string TRANSACTION_TYPE_TABLE_DATA_FILE_PATH = "TransactionTypeTableData.xml";

            private static PortfolioDatabaseManager instance;

            private SQLiteDatabaseHandler sqliteDatabaseHandler;

            private bool undoableLastActionAvailable;

            private PortfolioDatabaseManager()
            {
                if (!FileIOUtils.FileExists(SQLite_DATABASE_FILE_PATH)) // data file does not exist
                {
                    try
                    {
                        ConsoleIOManager.Instance.LogNotice(
                            "Portfolio data file not found. Creating new data file ..",
                            ConsoleIOManager.eOutputReportType.SystemCritical);

                        initializeByCreatingNewPortfolioDatabaseFile();

                        ConsoleIOManager.Instance.LogNotice(
                            "New portfolio data file created successfully.",
                            ConsoleIOManager.eOutputReportType.SystemCritical);
                    }
                    catch(Exception exception) 
                    {
                        if(this.sqliteDatabaseHandler != null)
                        {
                            sqliteDatabaseHandler.Dispose();
                        }
                        
                        FileIOManager.Instance.DeleteFile(SQLite_DATABASE_FILE_PATH);

                        throw new PortfolioDatabaseManagerInitializationException(exception);
                    }                  
                }
                else // data file exists
                {
                    try
                    {
                        ConsoleIOManager.Instance.LogNotice(
                            "Portfolio data file found. Using existing file.",
                            ConsoleIOManager.eOutputReportType.System);

                        initializeUsingExistingPortfolioDatabaseFile();
                    }
                    catch (SQLiteDatabaseHandlerException SQLiteDatabaseHandlerException)
                    {
                        throw new PortfolioDatabaseManagerInitializationException(
                            SQLiteDatabaseHandlerException);
                    }
                }

                this.sqliteDatabaseHandler.OpenConnection();
            }

            internal static PortfolioDatabaseManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// whether an undoable, recently performed database action exists.
            /// </summary>
            internal bool UndoableLastActionAvailable
            {
                get { return undoableLastActionAvailable; }
            }

            internal static void Initialize()
            {
                instance = new PortfolioDatabaseManager();
            }

            /// <summary>
            /// undoes the last action performed on database.
            /// </summary>
            /// <exception cref="UndoableLastActionNotAvailableException">
            /// <seealso cref="assertUndoableLastActionAvailable"/>
            /// </exception>
            internal void UndoLastAction()
            {
                assertUndoableLastActionAvailable();

                this.sqliteDatabaseHandler.UndoLastTransaction();
                this.undoableLastActionAvailable = false;
            }

            // executes database operations described in portfolioDatabaseAction atomically

            /// <summary>
            /// executes database operations contained in <paramref name="action"/> in an automic manner.
            /// </summary>
            /// <param name="action"></param>
            internal void ExecuteAsOneAction(Action action)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(action);
            }

            /// <summary>
            /// adds empty <see cref="PortfolioEntry"/>s to database, corresponding to specified
            /// <paramref name="coinIds"/>.
            /// </summary>
            /// <seealso cref="AddCoin(long)"/>
            /// <param name="coinIds"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="AddCoin(long)"/>
            /// </exception>
            internal void AddCoins(IEnumerable<long> coinIds)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(() => 
                    {
                        foreach(long coinId in coinIds)
                        {
                            AddCoin(coinId);
                        }
                    }
                );

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// adds empty <see cref="PortfolioEntry"/> to database, corresponding to specified
            /// <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.InsertIntoTable(InsertQuery)"/>
            /// </exception>
            internal void AddCoin(long coinId)
            {
                // create a new portfolio entry associated with specified coin id
                InsertQuery insertQuery = new InsertQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new ValuedColumn[]
                    {
                        new ValuedColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME, coinId)
                    });

                this.sqliteDatabaseHandler.InsertIntoTable(insertQuery);

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/>s in database corresponding to specified 
            /// <paramref name="coinIds"/>.
            /// </summary>
            /// <seealso cref="RemoveCoin(long)"/>
            /// <param name="coinIds"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="RemoveCoin(long)"/>
            /// </exception>
            internal void RemoveCoins(IEnumerable<long> coinIds)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(
                    () =>
                        {
                            foreach(long coinId in coinIds)
                            {
                                RemoveCoin(coinId);
                            }
                        }
                );

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/> in database corresponding to specified
            /// <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="GetPortfolioEntryId(long)"/>
            /// <seealso cref="deletePortfolioEntry(long)"/>
            /// <seealso cref="deleteTransactionsAssociatedWithPortfolioEntry(long)"/>
            /// </exception>
            internal void RemoveCoin(long coinId)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(
                    () =>
                    {
                        // get portfolio id associated with specified coinId
                        long portfolioEntryId = GetPortfolioEntryId(coinId);

                        // delete portfolio entry with specified coinId
                        deletePortfolioEntry(coinId);

                        // delete transactions associated with portfolioEntryId
                        deleteTransactionsAssociatedWithPortfolioEntry(portfolioEntryId);
                    }
                );

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// returns id of <see cref="PortfolioEntry"/> in database corresponding to specified
            /// <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// id of <see cref="PortfolioEntry"/> in database corresponding to specified
            /// <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.SelectFromTable(SelectQuery)"/>
            /// </exception>
            internal long GetPortfolioEntryId(long coinId)
            {
                // get portfolio id associated with specified coinId
                SelectQuery portfolioIdSelectQuery = new SelectQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            coinId),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                ResultSet portfolioIdResultSet =
                    sqliteDatabaseHandler.SelectFromTable(portfolioIdSelectQuery);

                long portfolioEntryId = portfolioIdResultSet.GetColumnValue<long>(
                    0,
                    DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME);

                return portfolioEntryId;
            }

            /// <summary>
            /// adds specified <paramref name="transaction"/>, associated to specified 
            /// <paramref name="portfolioEntry"/> to database.
            /// </summary>
            /// <param name="transaction"></param>
            /// <param name="portfolioEntry"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.InsertIntoTable(InsertQuery)"/>
            /// </exception>
            internal void AddTransaction(Transaction transaction, PortfolioEntry portfolioEntry)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(
                    () =>
                        {
                            // get Transacion.eType id based on Transaction.eType name ("Buy" \ "Sell")
                            SelectQuery transactionTypeIdSelectQuery =
                                buildTransactionTypeIdSelectQuery(transaction);

                            // insert Transaction into "CoinTransaction" table with Transacion.eType id
                            // fetched by using transactionTypeIdSelectQuery
                            InsertQuery insertTransactionIntoTableQuery = new InsertQuery(
                                DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                                new ValuedColumn[]
                                {
                                new ValuedColumn(
                                    DatabaseStructure.CoinTransactionTableStructure.
                                    PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                    portfolioEntry.Id),
                                new ValuedColumn(
                                    DatabaseStructure.CoinTransactionTableStructure.
                                    TRANSACTIO_TYPE_ID_COLUMN_NAME,
                                    transactionTypeIdSelectQuery),
                                new ValuedColumn(
                                    DatabaseStructure.CoinTransactionTableStructure.AMOUNT_COLUMN_NAME,
                                    transaction.Amount),
                                new ValuedColumn(
                                    DatabaseStructure.CoinTransactionTableStructure.PRICE_PER_COIN_COLUMN_NAME,
                                    transaction.PricePerCoin),
                                new ValuedColumn(
                                    DatabaseStructure.CoinTransactionTableStructure.UNIX_TIMESTAMP_COLUMN_NAME,
                                    transaction.UnixTimestamp)
                                }
                            );

                            sqliteDatabaseHandler.InsertIntoTable(insertTransactionIntoTableQuery);
                        }
                );

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// returns whether a <see cref="PortfolioEntry"/> corresponding to
            /// specified <paramref name="coinId"/> exists in portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// true if a <see cref="PortfolioEntry"/> corresponding to
            /// specified <paramref name="coinId"/> exists in portfolio,
            /// else false
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.SelectFromTable(SelectQuery)"/>
            /// </exception>
            internal bool IsCoinIdInPortfolio(long coinId)
            {
                // count portfolio entries with specified coinId from PortfolioEntry table
                SelectQuery selectQuery = new SelectQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new FunctionTableColumn(
                            FunctionTableColumn.eFunctionType.Count,
                            DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            coinId),
                        BasicCondition.eOperatorType.Equal));

                ResultSet resultSet = this.sqliteDatabaseHandler.SelectFromTable(selectQuery);

                long numberOfPortfolioEntriesWithCoinId = resultSet.GetColumnValue<long>(0, 0);

                return numberOfPortfolioEntriesWithCoinId == 1;
            }

            /// <summary>
            /// updates <see cref="PortfolioEntry"/> in database with data from specified
            /// <paramref name="portfolioEntry"/>.
            /// </summary>
            /// <param name="portfolioEntry"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.UpdateTable(UpdateQuery)"/>
            /// </exception>
            internal void UpdatePortfolioEntry(PortfolioEntry portfolioEntry)
            {
                // update PortfolioEntry table with PortfolioEntry data which might have changed
                UpdateQuery updateQuery = new UpdateQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new ValuedColumn[]
                    {
                        new ValuedColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.HOLDINGS_COLUMN_NAME,
                            portfolioEntry.Holdings),
                        new ValuedColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.AVERAGE_BUY_PRICE_COLUMN_NAME,
                            portfolioEntry.AverageBuyPriceUsd)
                    },
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            portfolioEntry.Id),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                sqliteDatabaseHandler.UpdateTable(updateQuery);

                this.undoableLastActionAvailable = true;
            }

            /// <summary>
            /// returns <see cref="PortfolioEntry"/> in database
            /// corresponding to specified <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="PortfolioEntry"/> in database
            /// corresponding to specified <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.SelectFromTable(SelectQuery)"/>
            /// </exception>
            internal PortfolioEntry GetPortfolioEntry(long coinId)
            {
                // select all columns of PortfolioEntry with specified coinId
                SelectQuery selectQuery = new SelectQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME),
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME),
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.HOLDINGS_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME),
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.AVERAGE_BUY_PRICE_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            coinId),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                ResultSet resultSet = this.sqliteDatabaseHandler.SelectFromTable(selectQuery);

                // get single row in result set, containing PortfolioEntry columns
                Row portfolioEntryRow = resultSet.GetRow(0);

                // get CoinTicker from manager, if available
                CoinTicker portfolioEntryCoinTicker = CoinTickerManager.Instance.HasCoinTicker(coinId)
                    ? CoinTickerManager.Instance.GetCoinTicker(coinId)
                    : null;

                // init a new PortfolioEntry having fetched data
                PortfolioEntry portfolioEntry = new PortfolioEntry(
                    portfolioEntryRow.GetColumnValue<long>(
                        DatabaseStructure.PortfolioEntryTableStructure.ID_COLUMN_NAME),
                    portfolioEntryRow.GetColumnValue<long>(
                        DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME),
                    portfolioEntryRow.GetColumnValue<double>(
                        DatabaseStructure.PortfolioEntryTableStructure.HOLDINGS_COLUMN_NAME),
                    portfolioEntryRow.GetColumnValue<double?>(
                        DatabaseStructure.PortfolioEntryTableStructure.AVERAGE_BUY_PRICE_COLUMN_NAME),
                    portfolioEntryCoinTicker);

                return portfolioEntry;
            }

            /// <summary>
            /// returns coin ids corresponding to all <see cref="PortfolioEntry"/>s in database.
            /// </summary>
            /// <returns>
            /// coin id array, containing coin ids corresponding to all
            /// <see cref="PortfolioEntry"/>s in database
            /// </returns>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.SelectFromTable(SelectQuery)"/>
            /// </exception>
            internal long[] GetCoinIdsInPortfolio()
            {
                long[] coinIds;

                // select all coinIds from "PortfolioEntry" table
                SelectQuery selectQuery = new SelectQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME)
                    },
                    null);

                ResultSet resultSet = sqliteDatabaseHandler.SelectFromTable(selectQuery);

                // insert fetched coinIds into result array
                coinIds = new long[resultSet.RowCount];

                for(int i = 0; i < resultSet.RowCount; i++)
                {
                    coinIds[i] = resultSet.Rows[i].GetColumnValue<long>(
                        DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME);
                }

                return coinIds;
            }

            /// <summary>
            /// returns a <see cref="SelectQuery"/>, selecting transaction type id
            /// corresponding to specified <paramref name="transaction"/>.
            /// </summary>
            /// <param name="transaction"></param>
            /// <returns>
            /// <see cref="SelectQuery"/>, selecting transaction type id
            /// corresponding to specified <paramref name="transaction"/>
            /// </returns>
            private SelectQuery buildTransactionTypeIdSelectQuery(Transaction transaction)
            {
                SelectQuery transactionTypeIdSelectQuery;

                string transactionTypeString = transaction.TransactionType.ToString();

                transactionTypeIdSelectQuery
                    = new SelectQuery(DatabaseStructure.TransactionTypeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                                new TableColumn(
                                    DatabaseStructure.TransactionTypeTableStructure.ID_COLUMN_NAME,
                                    DatabaseStructure.TransactionTypeTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.TransactionTypeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.TransactionTypeTableStructure.TABLE_NAME,
                            transactionTypeString),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                return transactionTypeIdSelectQuery;
            }

            /// <summary>
            /// deletes from database all <see cref="Transaction"/>s associated with specified
            /// <paramref name="portfolioEntryId"/>.
            /// </summary>
            /// <param name="portfolioEntryId"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.DeleteFromTable(DeleteQuery)"/>
            /// </exception>
            private void deleteTransactionsAssociatedWithPortfolioEntry(long portfolioEntryId)
            {
                DeleteQuery transactionDeleteQuery = new DeleteQuery(
                    DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.CoinTransactionTableStructure.PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                            DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                            portfolioEntryId),
                        BasicCondition.eOperatorType.Equal
                        )
                    );
                this.sqliteDatabaseHandler.DeleteFromTable(transactionDeleteQuery);
            }

            /// <summary>
            /// deletes from database <see cref="PortfolioEntry"/> associated with specified
            /// <paramref name="portfolioEntryCoinId"/>.
            /// </summary>
            /// <param name="portfolioEntryCoinId"></param>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler.DeleteFromTable(DeleteQuery)"/>
            /// </exception>
            private void deletePortfolioEntry(long portfolioEntryCoinId)
            {
                // delete PortfolioEntry with specified coinId from "PortfolioEntry" table
                DeleteQuery portfolioEntryDeleteQuery = new DeleteQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            portfolioEntryCoinId),
                        BasicCondition.eOperatorType.Equal)
                    );

                sqliteDatabaseHandler.DeleteFromTable(portfolioEntryDeleteQuery);
            }

            /// <summary>
            /// initializes this <see cref="PortfolioDatabaseManager"/> by creating a new 
            /// portfolio database file.
            /// </summary>
            /// <exception cref="FileXmlDocumentInitializationException">
            /// <seealso cref="FileXmlDocument(string)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler(string,bool)"/>
            /// <seealso cref="SQLiteDatabaseHandler.LoadDatabaseSchema(FileXmlDocument)"/>
            /// <seealso cref="SQLiteDatabaseHandler.LoadTableData(FileXmlDocument)"/>
            /// </exception>
            private void initializeByCreatingNewPortfolioDatabaseFile()
            {
                // parse XML files
                
                // read DatabaseSchema XML from file
                FileXmlDocument databaseSchemaXmlDocument = new FileXmlDocument(DATABASE_SCHEMA_FILE_PATH);

                // read TransactionTypeTable data from XML file
                FileXmlDocument transactionTypeTableDataXmlDocument =
                    new FileXmlDocument(TRANSACTION_TYPE_TABLE_DATA_FILE_PATH);

                // initialize SQLiteDatabaseHandler
                this.sqliteDatabaseHandler = new SQLiteDatabaseHandler(SQLite_DATABASE_FILE_PATH, true);
                this.sqliteDatabaseHandler.OpenConnection();

                // start initialization transaction
                ulong transactionHandle = this.sqliteDatabaseHandler.BeginTransactionIfNotAlreadyUnderway(
                    out bool transactionStarted);

                // initialize SQLiteDatabaseHandler with DatabaseSchema
                // create database tables specified in databaseSchemaXmlDocument
                this.sqliteDatabaseHandler.LoadDatabaseSchema(databaseSchemaXmlDocument);

                // initialize TransactionTypeTable (representing Transaction.eType) with enum data 
                // insert rows specfied in FileXmlDocument into TransactionTypeTable
                this.sqliteDatabaseHandler.LoadTableData(transactionTypeTableDataXmlDocument);

                // commit initialization transaction
                this.sqliteDatabaseHandler.CommitTransactionIfStartedByCaller(
                    transactionHandle,
                    transactionStarted);

                // close connection
                this.sqliteDatabaseHandler.CloseConnection();
            }

            /// <summary>
            /// initializes this <see cref="PortfolioDatabaseManager"/> using an existing database file.
            /// </summary>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="SQLiteDatabaseHandler(string, bool)"/>
            /// </exception>
            private void initializeUsingExistingPortfolioDatabaseFile()
            { 
                sqliteDatabaseHandler = new SQLiteDatabaseHandler(SQLite_DATABASE_FILE_PATH);
            }

            /// <summary>
            /// asserts that a recently performed undoable action is available.
            /// </summary>
            /// <exception cref="UndoableLastActionNotAvailableException">
            /// thrown if a recently performed undoable action is not available
            /// </exception>
            private void assertUndoableLastActionAvailable()
            {
                if(!this.undoableLastActionAvailable)
                {
                    throw new UndoableLastActionNotAvailableException();
                }
            }
        }
    }
}