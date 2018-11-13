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
                }

                internal static class ExchangeCoinHoldingTableStructure
                {
                    internal static readonly string TABLE_NAME = "ExchangeCoinHolding";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string EXCHANGE_ID_COLUMN_NAME = "exchangeId";
                    internal static readonly string PORTFOLIO_ENTRY_ID_COLUMN_NAME = "portfolioEntryId";
                    internal static readonly string HOLDING_COLUMN_NAME = "holding";
                    internal static readonly string AVERAGE_BUY_PRICE_COLUMN_NAME = "averageBuyPrice";
                }

                internal static class ExchangeTableStructure
                {
                    internal static readonly string TABLE_NAME = "Exchange";

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
                    internal static readonly string EXCHANGE_ID_COLUMN_NAME = "exchangeId";
                    internal static readonly string COIN_TRANSACTION_TYPE_ID_COLUMN_NAME =
                        "coinTransactionTypeId";
                    internal static readonly string AMOUNT_COLUMN_NAME = "amount";
                    internal static readonly string PRICE_PER_COIN_COLUMN_NAME = "pricePerCoin";
                    internal static readonly string UNIX_TIMESTAMP_COLUMN_NAME = "unixTimestamp";

                }

                internal static class CoinTransactionTypeTableStructure
                {
                    internal static readonly string TABLE_NAME = "CoinTransactionType";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string NAME_COLUMN_NAME = "name";
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
            /// adds a <see cref="PortfolioEntry"/> to database, corresponding to specified
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
                ExecuteAsOneAction(
                    () =>
                    {
                        // get portfolio id associated with specified coinId
                        long portfolioEntryId = GetPortfolioEntryId(coinId);

                        // delete portfolio entry with specified coinId
                        deletePortfolioEntry(coinId);

                        // delete transactions associated with portfolio entry
                        deleteTransactionsAssociatedWithPortfolioEntry(portfolioEntryId);

                        // delete ExchangeCoinHoldings associated with portfolio entry
                        deleteExchangeCoinHoldingsAssociatedWithPortfolioEntry(portfolioEntryId);
                    }
                );

                this.undoableLastActionAvailable = true;
            }

            private void deleteExchangeCoinHoldingsAssociatedWithPortfolioEntry(long portfolioEntryId)
            {
                DeleteQuery exchangeCoinHoldingsAssociatedwithPortfolioEntryDeleteQuery = new DeleteQuery(
                    DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                            portfolioEntryId),
                        BasicCondition.eOperatorType.Equal
                    )
                );

                this.sqliteDatabaseHandler.DeleteFromTable(
                    exchangeCoinHoldingsAssociatedwithPortfolioEntryDeleteQuery);
            }

            /// <summary>
            /// returns id of <see cref="PortfolioEntry"/> in database associated with specified
            /// <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// id of <see cref="PortfolioEntry"/> in database associated with specified
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

            internal void AddTransaction(Transaction transaction, long portfolioEntryId)
            {            
                ExecuteAsOneAction(
                    () =>
                        {
                            addExchangeIfNotExists(transaction.ExchangeName);

                            // get Transacion.eType id based on Transaction.eType name ("Buy" \ "Sell")
                            SelectQuery transactionTypeIdSelectQuery =
                                buildTransactionTypeIdSelectQuery(transaction);

                            // get exchange id based on exchange name
                            SelectQuery exchangeIdSelectQuery =
                                buildExchangeIdSelectQuery(transaction.ExchangeName);

                            // insert Transaction into CoinTransaction table
                            InsertQuery insertTransactionIntoTableQuery = new InsertQuery(
                                DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                                new ValuedColumn[]
                                {
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.
                                        PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                        portfolioEntryId),
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.
                                        EXCHANGE_ID_COLUMN_NAME,
                                        exchangeIdSelectQuery),
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.
                                        COIN_TRANSACTION_TYPE_ID_COLUMN_NAME,
                                        transactionTypeIdSelectQuery),
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.AMOUNT_COLUMN_NAME,
                                        transaction.Amount),
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.
                                        PRICE_PER_COIN_COLUMN_NAME,
                                        transaction.PricePerCoin),
                                    new ValuedColumn(
                                        DatabaseStructure.CoinTransactionTableStructure.
                                        UNIX_TIMESTAMP_COLUMN_NAME,
                                        transaction.UnixTimestamp)
                                });

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
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                ResultSet resultSet = this.sqliteDatabaseHandler.SelectFromTable(selectQuery);

                long numberOfPortfolioEntriesWithCoinId = resultSet.GetColumnValue<long>(0, 0);

                return numberOfPortfolioEntriesWithCoinId == 1;
            }

            internal void AddExchangeCoinHolding(
                ExchangeCoinHolding exchangeCoinHolding,
                long portfolioEntryId)
            {
                ExecuteAsOneAction(
                    () =>
                        {
                            addExchangeIfNotExists(exchangeCoinHolding.ExchangeName);

                            SelectQuery exchangeIdSelectQuery = buildExchangeIdSelectQuery(
                                exchangeCoinHolding.ExchangeName);

                            InsertQuery exchangeCoinHoldingInsertQuery = new InsertQuery(
                                DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                                new ValuedColumn[]
                                {
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                        EXCHANGE_ID_COLUMN_NAME,
                                        exchangeIdSelectQuery),
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                        PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                        portfolioEntryId),
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                        HOLDING_COLUMN_NAME,
                                        exchangeCoinHolding.Amount),
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                        AVERAGE_BUY_PRICE_COLUMN_NAME,
                                        exchangeCoinHolding.AverageBuyPrice)
                                });

                            this.sqliteDatabaseHandler.InsertIntoTable(exchangeCoinHoldingInsertQuery);                            
                        }
                );

                this.undoableLastActionAvailable = true;
            }

            internal void UpdateExchangeCoinHolding(
                ExchangeCoinHolding exchangeCoinHolding,
                long portfolioEntryId)
            {
                long exchangeId = getExchangeId(exchangeCoinHolding.ExchangeName);

                ExecuteAsOneAction(
                    () =>
                        {
                            UpdateQuery exchangeCoinHoldingUpdateQuery = new UpdateQuery(
                                DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                                new ValuedColumn[]
                                {
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.HOLDING_COLUMN_NAME,
                                        exchangeCoinHolding.Amount),
                                    new ValuedColumn(
                                        DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                        AVERAGE_BUY_PRICE_COLUMN_NAME,
                                        exchangeCoinHolding.AverageBuyPrice)
                                },
                                new ComplexCondition(
                                    new BasicCondition(
                                        new ValuedTableColumn(
                                            DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                            PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                                            portfolioEntryId),
                                        BasicCondition.eOperatorType.Equal
                                    ),
                                    new BasicCondition(
                                        new ValuedTableColumn(
                                            DatabaseStructure.ExchangeCoinHoldingTableStructure.
                                            EXCHANGE_ID_COLUMN_NAME,
                                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                                            exchangeId),
                                        BasicCondition.eOperatorType.Equal
                                    ),
                                    ComplexCondition.eLogicalOperator.And
                                )
                            );

                            this.sqliteDatabaseHandler.UpdateTable(exchangeCoinHoldingUpdateQuery);
                        }
                );

                this.undoableLastActionAvailable = true;
            }

            // to be implemented
            internal void RemoveExchangeCoinHolding(
                long exchangeCoinHoldingId)
            {
 
            }

            internal PortfolioEntry GetPortfolioEntry(long coinId)
            {
                PortfolioEntry portfolioEntry;

                long portfolioEntryId = GetPortfolioEntryId(coinId);
                ExchangeCoinHolding[] exchangeCoinHoldings = getExchangeCoinHoldings(coinId);

                portfolioEntry = new PortfolioEntry(
                    portfolioEntryId,
                    coinId,
                    exchangeCoinHoldings);

                return portfolioEntry;
            }

            private void addExchangeIfNotExists(string exchangeName)
            {
                SelectQuery exchangeNameCountSelectQuery = new SelectQuery(
                    DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new FunctionTableColumn(
                            FunctionTableColumn.eFunctionType.Count,
                            DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                            exchangeName),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                ResultSet exchangeNameCountResultSet = this.sqliteDatabaseHandler.SelectFromTable(
                    exchangeNameCountSelectQuery);

                long numberOfExchangesWithSpecifiedName = 
                    exchangeNameCountResultSet.GetColumnValue<long>(0, 0);

                if(numberOfExchangesWithSpecifiedName == 0)
                {
                    InsertQuery exchangeInsertQuery = new InsertQuery(
                        DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                        new ValuedColumn[]
                        {
                            new ValuedColumn(
                                DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                                exchangeName)
                        }
                    );

                    this.sqliteDatabaseHandler.InsertIntoTable(exchangeInsertQuery);
                }
            }

            private ExchangeCoinHolding[] getExchangeCoinHoldings(long coinId)
            {
                ExchangeCoinHolding[] exchangeCoinHoldings;

                long portfolioEntryId = GetPortfolioEntryId(coinId);

                SelectQuery exchangeCoinHoldingSelectQuery = new SelectQuery(
                    DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.EXCHANGE_ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME),
                        new TableColumn(
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.HOLDING_COLUMN_NAME,
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME),
                        new TableColumn(
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.AVERAGE_BUY_PRICE_COLUMN_NAME,
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeCoinHoldingTableStructure.TABLE_NAME,
                            portfolioEntryId),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                ResultSet exchangeCoinHoldingResultSet = this.sqliteDatabaseHandler.SelectFromTable(
                    exchangeCoinHoldingSelectQuery);

                exchangeCoinHoldings = new ExchangeCoinHolding[exchangeCoinHoldingResultSet.RowCount];

                for(int i = 0; i < exchangeCoinHoldingResultSet.RowCount; i++)
                {
                    Row exchangeCoinHoldingRow = exchangeCoinHoldingResultSet.GetRow(i);

                    long exchangeId = exchangeCoinHoldingRow.GetColumnValue<long>(
                        DatabaseStructure.ExchangeCoinHoldingTableStructure.EXCHANGE_ID_COLUMN_NAME);
                    string exchangeName = getExchangeName(exchangeId);
                    double coinHolding = exchangeCoinHoldingRow.GetColumnValue<double>(
                        DatabaseStructure.ExchangeCoinHoldingTableStructure.HOLDING_COLUMN_NAME);
                    double coinAverageBuyPrice = exchangeCoinHoldingRow.GetColumnValue<double>(
                        DatabaseStructure.ExchangeCoinHoldingTableStructure.AVERAGE_BUY_PRICE_COLUMN_NAME);

                    exchangeCoinHoldings[i] = new ExchangeCoinHolding(
                        coinId,
                        exchangeName,
                        coinHolding,
                        coinAverageBuyPrice);
                }

                return exchangeCoinHoldings;
            }

            private long getExchangeId(string exchangeName)
            {
                long exchangeId;

                SelectQuery exchangeIdSelectQuery = new SelectQuery(
                    DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ExchangeTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME
                        )
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                            exchangeName),
                        BasicCondition.eOperatorType.Equal
                    )
                );

                ResultSet exchangeIdResultSet = this.sqliteDatabaseHandler.SelectFromTable(
                    exchangeIdSelectQuery);

                exchangeId = exchangeIdResultSet.GetColumnValue<long>(0, 0);

                return exchangeId;
            }

            private string getExchangeName(long exchangeId)
            {
                string exchangeName;

                SelectQuery exchangeNameSelectQuery = new SelectQuery(
                    DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME
                        )
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                            exchangeId),
                        BasicCondition.eOperatorType.Equal
                    )
                );

                ResultSet exchangeNameResultSet = this.sqliteDatabaseHandler.SelectFromTable(
                    exchangeNameSelectQuery);

                exchangeName = exchangeNameResultSet.GetColumnValue<string>(0, 0);

                return exchangeName;
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
                    = new SelectQuery(DatabaseStructure.CoinTransactionTypeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                                new TableColumn(
                                    DatabaseStructure.CoinTransactionTypeTableStructure.ID_COLUMN_NAME,
                                    DatabaseStructure.CoinTransactionTypeTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.CoinTransactionTypeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.CoinTransactionTypeTableStructure.TABLE_NAME,
                            transactionTypeString),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                return transactionTypeIdSelectQuery;
            }

            private SelectQuery buildExchangeIdSelectQuery(string exchangeName)
            {
                SelectQuery exchangeIdSelectQuery = new SelectQuery(
                    DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ExchangeTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.ExchangeTableStructure.NAME_COLUMN_NAME,
                            DatabaseStructure.ExchangeTableStructure.TABLE_NAME,
                            exchangeName),
                        BasicCondition.eOperatorType.Equal
                    )
                );

                return exchangeIdSelectQuery;
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

                this.sqliteDatabaseHandler.DeleteFromTable(portfolioEntryDeleteQuery);
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