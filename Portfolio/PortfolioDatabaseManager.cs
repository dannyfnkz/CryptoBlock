﻿using CryptoBlock.CMCAPI;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils.IO.SqLite;
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

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        internal class PortfolioDatabaseManager
        {
            internal class PortfolioDatabaseManagerException : Exception
            {
                internal PortfolioDatabaseManagerException(
                    string message = null,
                    Exception innerException = null)
                    : base(message, innerException)
                {

                }
            }

            internal class UndoableLastActionNotAvailableException : PortfolioDatabaseManagerException
            {
                internal UndoableLastActionNotAvailableException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "No undoable last action available.";
                }
            }

            private static class DatabaseStructure
            {
                internal static class PortfolioEntryTableStructure
                {
                    internal static readonly string TABLE_NAME = "PortfolioEntry";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string COIN_ID_COLUMN_NAME = "coinId";
                    internal static readonly string HOLDINGS_COLUMN_NAME = "holdings";
                    internal static readonly string AVERAGE_BUY_PRICE_COLUMN_NAME = "averageBuyPrice";
                }

                internal static class PortfolioEntryTransactionTableStructure
                {
                    internal static readonly string TABLE_NAME = "PortfolioEntryCoinTransaction";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string PORTFOLIO_ENTRY_ID_COLUMN_NAME = "portfolioEntryId";
                    internal static readonly string COIN_TRANSACTION_ID_COLUMN_NAME = "coinTransactionId";
                }

                internal static class TransactionTypeTableStructure
                {
                    internal static readonly string TABLE_NAME = "CoinTransactionType";

                    internal static readonly string ID_COLUMN_NAME = "_id";
                    internal static readonly string NAME_COLUMN_NAME = "name";
                }

                internal static class CoinTransactionTableStructure
                {
                    internal static readonly string TABLE_NAME = "CoinTransaction";

                    internal static readonly string ID_COLUMN_NAME = "_id";
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
                if (!FileIOManager.Instance.FileExists(SQLite_DATABASE_FILE_PATH))
                {
                    try
                    {
                        createNewPortfolioDatabaseFile();
                    }
                    catch(Exception exception) 
                    {
                        if(this.sqliteDatabaseHandler != null)
                        {
                            sqliteDatabaseHandler.Dispose();
                        }
                        
                        FileIOManager.Instance.DeleteFile(SQLite_DATABASE_FILE_PATH);

                        throw exception;
                    }                  
                }
                else
                {
                    useExistingPortfolioDatabaseFile();
                }
            }

            internal static PortfolioDatabaseManager Instance
            {
                get { return instance; }
            }

            internal bool UndoableLastActionAvailable
            {
                get { return undoableLastActionAvailable; }
            }

            internal static void Initialize()
            {
                instance = new PortfolioDatabaseManager();
            }

            internal void UndoLastAction()
            {
                assertUndoableLastActionAvailable();

                this.sqliteDatabaseHandler.UndoLastTransaction();
                this.undoableLastActionAvailable = false;
            }

            // executes database operations described in portfolioDatabaseAction atomically
            internal void ExecuteAsOneAction(Action portfolioDatabaseAction)
            {
                this.sqliteDatabaseHandler.ExecuteWithinTransaction(portfolioDatabaseAction);
            }

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
                                    DatabaseStructure.CoinTransactionTableStructure.TRANSACTIO_TYPE_ID_COLUMN_NAME,
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

                            // create database association between inserted Transaction
                            // and its corresponding PortfolioEntry
                            associatePortfolioEntryAndLastInsertedTransaction(portfolioEntry.Id);
                        }
                );

                this.undoableLastActionAvailable = true;
            }

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

            private SelectQuery buildTransactionTypeIdSelectQuery(Transaction transaction)
            {
                SelectQuery transactionTypeIdSelectQuery;

                string transactionTypeString = transaction.Type.ToString();

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

            // assumes SQL transaction is underway
            private void deleteTransactionsAssociatedWithPortfolioEntry(long portfolioEntryId)
            {
                // select transactionIds associated with portfolioEntryId
                SelectQuery transactionIdSelectQuery = new SelectQuery
                    (DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.PortfolioEntryTransactionTableStructure
                            .COIN_TRANSACTION_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTransactionTableStructure
                            .PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME,
                            portfolioEntryId),
                        BasicCondition.eOperatorType.Equal
                        )
                    );

                // delete Transactions with selected transactionIds from "CoinTransaction" table
                DeleteQuery transactionDeleteQuery = new DeleteQuery(
                    DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.CoinTransactionTableStructure.ID_COLUMN_NAME,
                            DatabaseStructure.CoinTransactionTableStructure.TABLE_NAME,
                            transactionIdSelectQuery),
                        BasicCondition.eOperatorType.In
                        )
                    );

                sqliteDatabaseHandler.DeleteFromTable(transactionDeleteQuery);

                // delete association between deleted Transactions and deleted PortfolioEntry
                // from "PortfolioEntryTransaction" table
                deletePortfolioEntryTransactionAssociations(portfolioEntryId);
            }

            private void deletePortfolioEntryTransactionAssociations(long portfolioEntryId)
            {
                DeleteQuery transactionToPortfolioEntryAssociationDeleteQuery =
                    new DeleteQuery(
                        DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME,
                        new BasicCondition(
                            new ValuedTableColumn(
                                DatabaseStructure.PortfolioEntryTransactionTableStructure
                                .PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME,
                                portfolioEntryId),
                            BasicCondition.eOperatorType.Equal
                            )
                   );

                sqliteDatabaseHandler.DeleteFromTable(transactionToPortfolioEntryAssociationDeleteQuery);
            }

            private void deletePortfolioEntry(long coinId)
            {
                // delete PortfolioEntry with specified coinId from "PortfolioEntry" table
                DeleteQuery portfolioEntryDeleteQuery = new DeleteQuery(
                    DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                    new BasicCondition(
                        new ValuedTableColumn(
                            DatabaseStructure.PortfolioEntryTableStructure.COIN_ID_COLUMN_NAME,
                            DatabaseStructure.PortfolioEntryTableStructure.TABLE_NAME,
                            coinId),
                        BasicCondition.eOperatorType.Equal)
                    );

                sqliteDatabaseHandler.DeleteFromTable(portfolioEntryDeleteQuery);
            }

            private void associatePortfolioEntryAndLastInsertedTransaction(long portfolioEntryId)
            {
                // get id of inserted Transaction
                SelectQuery transactionIdSelectQuery = new SelectQuery(
                    null,
                    new TableColumn[]
                    {
                                new FunctionTableColumn(FunctionTableColumn.eFunctionType.LastInsertRowid)
                    });

                // create association between inserted Transaction and PortfolioEntry
                // by INSERTing into "PortfolioEntryTransaction" table
                InsertQuery associateTransactionWithPortfolioEntryInsertQuery = new InsertQuery(
                    DatabaseStructure.PortfolioEntryTransactionTableStructure.TABLE_NAME,
                    new ValuedColumn[]
                    {
                                new ValuedColumn(
                                    DatabaseStructure.PortfolioEntryTransactionTableStructure
                                    .PORTFOLIO_ENTRY_ID_COLUMN_NAME,
                                    portfolioEntryId),
                                new ValuedColumn(
                                    DatabaseStructure.PortfolioEntryTransactionTableStructure
                                    .COIN_TRANSACTION_ID_COLUMN_NAME,
                                    transactionIdSelectQuery)
                    });

                sqliteDatabaseHandler.InsertIntoTable(
                    associateTransactionWithPortfolioEntryInsertQuery);
            }

            private void createNewPortfolioDatabaseFile()
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
                sqliteDatabaseHandler.LoadTableData(transactionTypeTableDataXmlDocument);

                // commit initialization transaction
                this.sqliteDatabaseHandler.CommitTransactionIfStartedByCaller(
                    transactionHandle,
                    transactionStarted);
            }

            private void useExistingPortfolioDatabaseFile()
            {
                sqliteDatabaseHandler = new SQLiteDatabaseHandler(SQLite_DATABASE_FILE_PATH);
                sqliteDatabaseHandler.OpenConnection();
            }

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