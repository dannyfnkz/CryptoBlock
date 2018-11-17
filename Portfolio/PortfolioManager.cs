using CryptoBlock.CMCAPI;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static CryptoBlock.Utils.IO.SQLite.SQLiteDatabaseHandler;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// manages user's <see cref="PortfolioEntry"/>s.
        /// </summary>
        public class PortfolioManager
        {
            /// <summary>
            /// thrown if an exception occurs while performing a <see cref="PortfolioManager"/> operation.
            /// </summary>
            public class PortfolioManagerException : Exception
            {
                public PortfolioManagerException(
                    string exceptionMessage = null,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {

                }
            }

            /// <summary>
            /// thrown if an operation on <see cref="PortfolioManager"/> is attempted to be performed before
            /// manager has been initialized.
            /// </summary>
            public class ManagerNotInitializedException : PortfolioManagerException
            {
                private readonly string operationName;

                public ManagerNotInitializedException(string operationName)
                    : base(formatExceptionMessage(operationName))
                {
                    this.operationName = operationName;
                }

                public string OperationName
                {
                    get { return operationName; }
                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Portfolio manager must be initialized before performing the following operation: {0}.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown if <see cref="PortfolioManager"/>.Initialize() is called when <see cref="PortfolioManager"/>
            /// is already initialized.
            /// </summary>
            public class ManagerAlreadyInitializedException : PortfolioManagerException
            {
                public ManagerAlreadyInitializedException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Portolio manager was already initialized.";
                }
            }

            /// <summary>
            /// thrown if an exception occurs while trying to performe a database operation.
            /// </summary>
            public class DatabaseCommunicationException : PortfolioManagerException
            {
                public DatabaseCommunicationException(string operationName, Exception innerException)
                    : base(formatExceptionMessage(operationName), innerException)
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "An exception occurred while trying to communicate with database."
                        + " Requested operation: '{0}'.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown if an undo action operation was attempted to be performed, while an
            /// undoable action was not available.
            /// </summary>
            public class UndoableLastActionNotAvailableException : PortfolioManagerException
            {
                public UndoableLastActionNotAvailableException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "No undoable action available.";
                }
            }

            /// <summary>
            /// thrown if an an exception occurs while trying to load <see cref="PortfolioManager"/> 
            /// data from file.
            /// </summary>
            public class DataFileLoadException : PortfolioManagerException
            {
                public DataFileLoadException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }
                
                private static string formatExceptionMessage()
                {
                    return "Could not load portfolio data from file.";
                }
            }

            /// <summary>
            /// thrown if an exception occurs while trying to save <see cref="PortfolioManager"/>'s data to
            /// file.
            /// </summary>
            public class DataFileSaveException : PortfolioManagerException
            {
                public DataFileSaveException(Exception exception)
                    : base(formatExceptionMessage(), exception)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Could not save portfolio data to file";
                }
            }

            /// <summary>
            /// thrown if an exception occurs while an operation on a specific coin is attempted to be performed.
            /// </summary>
            public abstract class CoinException : PortfolioManagerException
            {
                private readonly long coinId;

                public CoinException(long coinId, string message)
                    : base(message)
                {
                    this.coinId = coinId;
                }

                public long CoinId
                {
                    get { return coinId; }
                }
            }

            /// <summary>
            /// thrown if an operation is attempted to be performed on a coin whose coin ID does not
            /// exist in coin listing repository.
            /// </summary>
            public class InvalidCoinIdException : CoinException
            {
                public InvalidCoinIdException(long coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(long coinId)
                {
                    return string.Format(
                        "Coin with ID '{0}' does not exist in coin listing repository.",
                        coinId);
                }
            }

            /// <summary>
            /// thrown if a coin which already exists in portfolio is attempted to be added.
            /// </summary>
            public class CoinAlreadyInPortfolioException : CoinException
            {
                public CoinAlreadyInPortfolioException(long coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(long coinId)
                {
                    return string.Format(
                        "Coin with ID '{0}' already exists in portfolio.",
                        coinId);
                }
            }

            /// <summary>
            /// thrown if an operation is attempted to be performed on a coin which does not exist in portfolio.
            /// </summary>
            public class CoinNotInPortfolioException : CoinException
            {
                public CoinNotInPortfolioException(long coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(long coinId)
                {
                    return string.Format(
                        "Coin with ID '{0}' does not exist in portfolio.",
                        coinId);
                }
            }

            // sleep time for file data save thread
            private const int FILE_DATA_SAVE_THREAD_SLEEP_TIME_MILLIS = 10 * 1000;

            // max numerical value allowed for portfolio operations (e.g price, buy amount)
            private const double MAX_NUMERICAL_VALUE_ALLOWED = 1.0E15;
     

            private static PortfolioManager instance;

            [JsonProperty]
            private readonly Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry 
                = new Dictionary<int, PortfolioEntry>();

            private PortfolioManager()
            {

            }

            /// <summary>
            /// maximum numerical value allowed for portfolio operations (e.g price, buy amount).
            /// </summary>
            public static double MaxNumericalValueAllowed
            {
                get { return MAX_NUMERICAL_VALUE_ALLOWED; }
            }

            public static PortfolioManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// whether there exists a portfolio action, which was most recently performed and 
            /// can be undone.
            /// </summary>
            internal bool UndoableLastActionAvailable
            {
                get { return PortfolioDatabaseManager.Instance.UndoableLastActionAvailable; }
            }

            /// <summary>
            /// coin IDs associated with <see cref="PortfolioEntry"/>s which exist in portfolio.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception> 
            public long[] CoinIds
            {
                get
                {
                    assertManagerInitialized("CoinIds");

                    long[] coinIds = null;

                    try
                    {
                        coinIds = PortfolioDatabaseManager.Instance.GetCoinIdsInPortfolio();
                    }
                    catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                    {
                        handleDatabaseHandlerException("CoinIds", sqliteDatabaseHandlerException);
                    }

                    return coinIds;
                }
            }

            /// <summary>
            /// <para>
            /// initializes a new <see cref="PortfolioManager"/> instance and starts file data save thread.
            /// </para>
            /// <para>
            /// if a portfolio data save file exists, attempts to load portfolio data from file. 
            /// </para>
            /// </summary>
            /// <seealso cref="loadDataFromSaveFile"/>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// <seealso cref="assertManagerNotInitialized(string)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public static void Initialize()
            {
                assertManagerNotInitialized();

                try
                {
                    PortfolioDatabaseManager.Initialize();
                    instance = new PortfolioManager();
                }

                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("Initialize", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// undoes most recent undoable action which was performed on portfolio, if one exists.
            /// </summary>
            /// <seealso cref="PortfolioDatabaseManager.UndoLastAction"/>
            /// <exception cref="UndoableLastActionNotAvailableException">
            /// <seealso cref="assertUndoableLastActionAvailable"/>
            /// </exception>
            internal void UndoLastAction()
            {
                assertUndoableLastActionAvailable();

                PortfolioDatabaseManager.Instance.UndoLastAction();
            }

            /// <summary>
            /// returns whether a <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> exists
            /// in portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// true if <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> exists
            /// in portfolio,
            /// else false
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="assertCoinIdValid(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public bool IsInPortfolio(long coinId)
            {
                assertManagerInitialized("CreatePortfolioEntry");
                assertCoinIdValid(coinId);

                bool isInPortfolio = false;

                try
                {
                    isInPortfolio = PortfolioDatabaseManager.Instance.IsCoinIdInPortfolio(coinId);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("IsInPortfolio", sqliteDatabaseHandlerException);
                }

                return isInPortfolio;
            }

            /// <summary>
            /// adds <see cref="PortfolioEntry"/> corresponding to specified <paramref name="coinId"/>
            /// and performs specified <paramref name="buyTransaction"/> on it.
            /// </summary>
            /// <seealso cref="AddAndBuyCoin(long, IEnumerable{BuyTransaction})"/>
            /// <param name="coinId"></param>
            /// <param name="buyTransaction"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="AddAndBuyCoin(long, IEnumerable{BuyTransaction})"/>
            /// </exception>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// <seealso cref="AddAndBuyCoin(long, IEnumerable{BuyTransaction})"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="AddAndBuyCoin(long, IEnumerable{BuyTransaction})"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="AddAndBuyCoin(long, IEnumerable{BuyTransaction})"/>
            /// </exception>
            public void AddAndBuyCoin(long coinId, BuyTransaction buyTransaction)
            {
                assertManagerInitialized("AddAndBuyCoin");
                AddAndBuyCoin(coinId, new BuyTransaction[] { buyTransaction });
            }

            /// <summary>
            /// adds <see cref="PortfolioEntry"/> corresponding to specified <paramref name="coinId"/>
            /// and performs specified <paramref name="buyTransactions"/> on it.
            /// </summary>
            /// <seealso cref="AddCoin(long)"/>
            /// <seealso cref="BuyCoin(IList{BuyTransaction})"/>
            /// <param name="coinId"></param>
            /// <param name="buyTransactions"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="AddCoin(long)"/>
            /// </exception>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// <seealso cref="AddCoin(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="AddCoin(long)"/>
            /// <seealso cref="BuyCoin(IList{BuyTransaction})"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="BuyCoin(IList{BuyTransaction})"/>
            /// </exception>
            public void AddAndBuyCoin(long coinId, IList<BuyTransaction> buyTransactions)
            {
                assertManagerInitialized("AddAndBuyCoin");

                PortfolioDatabaseManager.Instance.ExecuteAsOneAction(
                    () =>
                    {
                        AddCoin(coinId);
                        BuyCoin(buyTransactions);
                    }
                );
            }

            /// <summary>
            /// adds a new <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> to portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="assertCoinIdValid(long)"/>
            /// </exception>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// <seealso cref="assertCoinNotAlreadyInPortfolio(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void AddCoin(long coinId)
            {
                assertManagerInitialized("CreatePortfolioEntry");
                assertCoinIdValid(coinId);
                assertCoinNotAlreadyInPortfolio(coinId);

                try
                {
                    PortfolioDatabaseManager.Instance.AddCoin(coinId);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("AddCoin", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// adds <see cref="PortfolioEntry"/>s corresponding to specified <paramref name="coinIds"/>.
            /// </summary>
            /// <seealso cref="PortfolioDatabaseManager.AddCoins(IEnumerable{long})"/>
            /// <param name="coinIds"></param>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="assertCoinIdValid(long)"/>
            /// </exception>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// <seealso cref="assertCoinNotAlreadyInPortfolio(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void AddCoins(IList<long> coinIds)
            { 
                assertManagerInitialized("AddCoins");

                foreach(long coinId in coinIds) // perform checks for all coinIds in advance
                {
                    assertCoinIdValid(coinId);
                    assertCoinNotAlreadyInPortfolio(coinId);
                }

                try
                {
                    PortfolioDatabaseManager.Instance.AddCoins(coinIds);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("AddCoins", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> from portfolio.
            /// </summary>
            /// <remarks>
            /// in addition, removes all <see cref="Transaction"/>s
            /// associated with said <see cref="PortfolioEntry"/>.
            /// </remarks>
            /// <param name="coinId"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="assertCoinInPortfolio(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void RemoveCoin(long coinId)
            {
                assertManagerInitialized("RemoveCoin");
                assertCoinIdValid(coinId);
                assertCoinInPortfolio(coinId);

                try
                {
                    PortfolioDatabaseManager.Instance.RemoveCoin(coinId);
                }         
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("RemoveCoin", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/>s corresponding to specified <paramref name="coinIds"/>
            /// from portfolio.
            /// </summary>
            /// <seealso cref="PortfolioDatabaseManager.RemoveCoins(IEnumerable{long})"/>
            /// <param name="coinIds"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="assertCoinIdValid(long)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="assertCoinInPortfolio(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void RemoveCoins(long[] coinIds)
            { 
                assertManagerInitialized("RemoveCoins");

                foreach (long coinId in coinIds) // perform checks for all coinIds
                {
                    assertCoinIdValid(coinId);
                    assertCoinInPortfolio(coinId);
                }

                try
                {
                    PortfolioDatabaseManager.Instance.RemoveCoins(coinIds);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("RemoveCoins", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// performs a buy transaction for coin having <paramref name="buyTransaction"/>.CoinId.
            /// </summary>
            /// <seealso cref="PortfolioEntry.Buy(Transaction)"/>
            /// <param name="buyTransaction"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="PortfolioEntry.Buy(Transaction))"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void BuyCoin(BuyTransaction buyTransaction)
            {
                assertManagerInitialized("BuyCoin");

                PortfolioEntry portfolioEntry = GetPortfolioEntry(buyTransaction.CoinId);

                try
                {
                    portfolioEntry.Buy(buyTransaction);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("BuyCoin", sqliteDatabaseHandlerException);
                }
            }

            /// <summary>
            /// performs the specified <paramref name="buyTransactions"/>.
            /// </summary>
            /// <remarks>
            /// can have a better implementation 
            /// (support on PortfolioDatabaseManager level for multiple buys).
            /// </remarks>
            /// <param name="buyTransactions"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="BuyCoin(BuyTransaction)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="BuyCoin(BuyTransaction)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="BuyCoin(BuyTransaction)"/>
            /// </exception>
            public void BuyCoin(IList<BuyTransaction> buyTransactions)
            {
                assertManagerInitialized("BuyCoin");

                PortfolioDatabaseManager.Instance.ExecuteAsOneAction(
                    () =>
                    {
                        foreach (BuyTransaction buyTransaction in buyTransactions)
                        {
                            BuyCoin(buyTransaction);
                        }
                    }
                );
            }

            /// <summary>
            /// performs a sell transaction.
            /// </summary>
            /// <seealso cref="PortfolioEntry.Sell(SellTransaction)"/>
            /// <param name="sellTransaction"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="GetPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="PortfolioEntry.Sell(SellTransaction)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            /// <seealso cref="PortfolioEntry.Sell(SellTransaction)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// <seealso cref="handleDatabaseHandlerException(string, SQLiteDatabaseHandlerException)"/>
            /// </exception>
            public void SellCoin(SellTransaction sellTransaction)
            {
                PortfolioEntry portfolioEntry = GetPortfolioEntry(sellTransaction.CoinId);

                try
                {
                    portfolioEntry.Sell(sellTransaction);
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("SellCoin", sqliteDatabaseHandlerException);
                }
            }

            // can have a better implementation (support on PortfolioDatabaseManager level for multiple sells

            /// <summary>
            /// performs the specified <paramref name="sellTransactions"/>.
            /// </summary>
            /// <seealso cref="SellCoin(SellTransaction)"/>
            /// <param name="sellTransactions"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="ManagerNotInitializedException"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="SellCoin(SellTransaction)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="SellCoin(SellTransaction)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            /// <seealso cref="SellCoin(SellTransaction)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="SellCoin(SellTransaction)"/>
            /// </exception>
            public void SellCoin(IList<SellTransaction> sellTransactions)
            {
                assertManagerInitialized("SellCoin");

                PortfolioDatabaseManager.Instance.ExecuteAsOneAction(
                    () =>
                    {
                        foreach (SellTransaction sellTransaction in sellTransactions)
                        {
                            SellCoin(sellTransaction);
                        }
                    }
                );
            }

            /// <summary>
            /// returns string representation of portfolio in tabular format,
            /// containing data of <see cref="PortfolioEntry"/>s with specified <paramref name="coinIds"/>.
            /// </summary>
            /// <param name="coinIds"></param>
            /// <returns>
            /// string representation of portfolio in tabular format,
            /// containing data of <see cref="PortfolioEntry"/>s with specified <paramref name="coinIds"/>.
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            public string GetPortfolioEntryDisplayTableString(params long[] coinIds)
            {
                // init portfolio entry table
                PortfolioEntryTable portfolioEntryTable = new PortfolioEntryTable();

                foreach(long coinId in coinIds)
                {
                    // add row corresponding to each portfolio entry associated with specified id
                    PortfolioEntry portfolioEntry = GetPortfolioEntry(coinId);
                    portfolioEntryTable.AddRow(portfolioEntry);
                }

                // return table display string
                string portfolioEntryTableString = portfolioEntryTable.GetTableDisplayString();

                return portfolioEntryTableString;
            }

            /// <summary>
            /// returns <see cref="PortfolioEntry"/> corresponding to <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="PortfolioEntry"/> corresponding to <paramref name="coinId"/>.
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="assertCoinInPortfolio(long)"/>
            /// </exception>
            /// <exception cref="DatabaseCommunicationException">
            /// <seealso cref="GetPortfolioEntry(long)"/>
            /// </exception>
            public PortfolioEntry GetPortfolioEntry(long coinId)
            {
                assertManagerInitialized("GetPortfolioEntry");
                assertCoinInPortfolio(coinId);

                PortfolioEntry portfolioEntry = null;

                try
                {
                    portfolioEntry =
                        PortfolioDatabaseManager.Instance.GetPortfolioEntry(coinId);

                    if(CoinTickerManager.Instance.HasCoinTicker(coinId))
                    {
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);
                        portfolioEntry.SetCoinTicker(coinTicker);
                    }
                }
                catch (SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
                {
                    handleDatabaseHandlerException("GetPortfolioEntry", sqliteDatabaseHandlerException);
                }

                return portfolioEntry;
            }

            /// <summary>
            /// handles an <see cref="SQLiteDatabaseHandlerException"/> that was thrown by
            /// <see cref="PortfolioDatabaseManager"/>, while performing specified
            /// <paramref name="operationName"/>.
            /// </summary>
            /// <param name="operationName"></param>
            /// <param name="sqliteDatabaseHandlerException"></param>
            /// <exception cref="DatabaseCommunicationException">
            /// exception corresponding to <paramref name="sqliteDatabaseHandlerException"/>
            /// and <paramref name="operationName"/>
            /// </exception>
            private static void handleDatabaseHandlerException(
                string operationName,
                SQLiteDatabaseHandlerException sqliteDatabaseHandlerException)
            {
                throw new DatabaseCommunicationException(operationName, sqliteDatabaseHandlerException);
            }

            /// <summary>
            /// asserts that <see cref="PortfolioManager"/> is initialized.
            /// </summary>
            /// <param name="operationName">
            /// name of operation that is attempted to be performed
            /// </param>
            /// <exception cref="ManagerNotInitializedException">
            /// thrown if <see cref="PortfolioManager"/> is not initialized
            /// </exception>
            private static void assertManagerInitialized(string operationName)
            {
                if (instance == null)
                {
                    throw new ManagerNotInitializedException(operationName);
                }
            }

            /// <summary>
            /// asserts that <see cref="PortfolioManager"/> was not initialized.
            /// </summary>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// thrown if <see cref="PortfolioManager"/> was initialized
            /// </exception>
            private static void assertManagerNotInitialized()
            {
                if (instance != null)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            /// <summary>
            /// asserts that a recently performed undoable portfolio action is available.
            /// </summary>
            /// <seealso cref="UndoableLastActionAvailable"/>
            /// <exception cref="UndoableLastActionNotAvailableException">
            /// thrown if a recently performed undoable portfolio action is not available
            /// </exception>
            private void assertUndoableLastActionAvailable()
            {
                if (!this.UndoableLastActionAvailable)
                {
                    throw new UndoableLastActionNotAvailableException();
                }
            }

            /// <summary>
            /// asserts that coin with <paramref name="coinId"/> exists in portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="CoinNotInPortfolioException">
            /// thrown if coin with <paramref name="coinId"/> does not exist in portfolio
            /// </exception>
            private void assertCoinInPortfolio(long coinId)
            {
                if(!IsInPortfolio(coinId))
                {
                    throw new CoinNotInPortfolioException(coinId);
                }
            }

            /// <summary>
            /// asserts that coin with <paramref name="coinId"/> does not exist in portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// thrown if coin with <paramref name="coinId"/> already exists in portfolio
            /// </exception>
            private void assertCoinNotAlreadyInPortfolio(long coinId)
            {
                if(IsInPortfolio(coinId))
                {
                    throw new CoinAlreadyInPortfolioException(coinId);
                }
            }

            /// <summary>
            /// asserts that <paramref name="coinId"/> exists in coin listing repository.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="InvalidCoinIdException">
            /// thrown if <paramref name="coinId"/> does not exist in coin listing repository.
            /// </exception>
            private void assertCoinIdValid(long coinId)
            {
               if(!CoinListingManager.Instance.CoinIdExists(coinId))
                {
                    throw new InvalidCoinIdException(coinId);
                }
            }
        }
    }
}

