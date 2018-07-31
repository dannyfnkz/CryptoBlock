using CryptoBlock.CMCAPI;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioEntry;

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
                public PortfolioManagerException(string exceptionMessage)
                    : base(exceptionMessage)
                {

                }
                public PortfolioManagerException(string exceptionMessage, Exception innerException)
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
            /// thrown if an an exception occurs while trying to load <see cref="PortfolioManager"/> data from file.
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
            /// thrown if an operation is attempted to be performed on a coin whose coin ID does not
            /// exist in coin listing repository.
            /// </summary>
            public class InvalidCoinIdException : CoinException
            {
                public InvalidCoinIdException(int coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(int coinId)
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

                public CoinAlreadyInPortfolioException(int coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(int coinId)
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
                public CoinNotInPortfolioException(int coinId)
                    : base(coinId, formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format(
                        "Coin with ID '{0}' does not exist in portfolio.",
                        coinId);
                }
            }

            /// <summary>
            /// thrown if an exception occurs while an operation on a specific coin is attempted to be performed.
            /// </summary>
            public abstract class CoinException : PortfolioManagerException
            {
                private readonly int coinId;

                public CoinException(int coinId, string message)
                    : base(message)
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }
            }

            // name of data save file
            private const string DATA_SAVE_FILE_NAME = "portfolio_data";

            // sleep time for file data save thread
            private const int FILE_DATA_SAVE_THREAD_SLEEP_TIME_MILLIS = 10 * 1000;

            // max numerical value allowed for portfolio operations (e.g price, buy amount)
            private const double MAX_NUMERICAL_VALUE_ALLOWED = 1.0E15;

            private static PortfolioManager instance;

            private Task fileDataSaveTask;

            // true if portfolio state had been changed and was not yet saved
            private bool unsavedStateChange = false;

            private bool fileDataSaveThreadRunning;

            [JsonProperty]
            private readonly Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry 
                = new Dictionary<int, PortfolioEntry>();

            private PortfolioManager()
            {
                // subscribe to CoinTickerManager repository update events
                CoinTickerManager.Instance.RepositoryUpdatedEvent += coinTickerManager_RepositoryUpdated;
            }

            /// <summary>
            /// construction used when loading portfolio manager data from save file.
            /// </summary>
            /// <param name="coinIdToPortfolioEntry"></param>
            [JsonConstructor]
            private PortfolioManager(Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry)
                : this()
            {
                this.coinIdToPortfolioEntry = coinIdToPortfolioEntry;
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
            public static void Initialize()
            {
                assertManagerNotInitialized();

                if(FileIOManager.Instance.DataFileExists(DATA_SAVE_FILE_NAME)) // data file available
                {
                    loadDataFromSaveFile();
                }
                else // data file not available
                {
                    // notify user
                    ConsoleIOManager.Instance.LogNotice("Portfolio data file not found.");
                    ConsoleIOManager.Instance.LogNotice("Initializing an empty portfolio.");

                    // initialize an empty portfolio
                    instance = new PortfolioManager();
                }

                // initialize portfolio entries with their corresponding coin tickers 
                initializePortfolioEntryCoinTickers();

                // start data file save thread
                instance.StartFileDataSaveThreadThread();
            }

            /// <summary>
            /// coin IDs associated with <see cref="PortfolioEntry"/>s which exist in portfolio.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            [JsonIgnore]
            public int[] CoinIds
            {
                get
                {
                    assertManagerInitialized("CoinIds");

                    return coinIdToPortfolioEntry.Keys.ToArray();
                }
            }

            /// <summary>
            /// whether a change made to portfolio was not yet saved to data file.
            /// </summary>
            [JsonIgnore] 
            private bool UnsavedStateChange
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get { return unsavedStateChange; }
                [MethodImpl(MethodImplOptions.Synchronized)]
                set { unsavedStateChange = value; }
            }

            /// <summary>
            /// starts the file data save thread.
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StartFileDataSaveThreadThread()
            {
                fileDataSaveThreadRunning = true;

                // init and start file data save task
                fileDataSaveTask = new Task(new Action(fileDataSaveTask_Target));
                fileDataSaveTask.Start();
            }

            /// <summary>
            /// stops the file data save thread.
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StopFileDataSaveThread()
            {
                fileDataSaveThreadRunning = false;
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
            /// <seealso cref="assertCoinIdValid(int)"/>
            /// </exception>
            public bool IsInPortfolio(int coinId)
            {
                assertManagerInitialized("CreatePortfolioEntry");
                assertCoinIdValid(coinId);

                return coinIdToPortfolioEntry.Keys.Contains(coinId);
            }

            /// <summary>
            /// adds a new <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> to portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="InvalidCoinIdException">
            /// <seealso cref="assertCoinIdValid(int)"/>
            /// </exception>
            /// <exception cref="CoinAlreadyInPortfolioException">
            /// <seealso cref="assertCoinNotAlreadyInPortfolio(int)"/>
            /// </exception>
            public void AddCoin(int coinId)
            {
                assertManagerInitialized("CreatePortfolioEntry");
                assertCoinIdValid(coinId);
                assertCoinNotAlreadyInPortfolio(coinId);

                // get CoinTicker corresponding to coinId, if exists
                CoinTicker coinTicker = CoinTickerManager.Instance.HasCoinTicker(coinId) ?
                    CoinTickerManager.Instance.GetCoinTicker(coinId)
                    : null;

                // create a new portfolio entry and update dictionary
                PortfolioEntry portfolioEntry = new PortfolioEntry(coinId, coinTicker);
                coinIdToPortfolioEntry[coinId] = portfolioEntry;

                onPortfolioStateChanged();
            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/> associated with <paramref name="coinId"/> from portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="assertCoinInPortfolio(int)"/>
            /// </exception>
            public void RemoveCoin(int coinId)
            {
                assertManagerInitialized("RemovePortfolioEntry");
                assertCoinInPortfolio(coinId);

                // remove (coinId,portfolioEntry) pair from dictionary
                coinIdToPortfolioEntry.Remove(coinId);

                onPortfolioStateChanged();
            }

            /// <summary>
            /// buys <paramref name="buyAmount"/> of coin
            /// with specified <paramref name="coinId"/> for <paramref name="buyPricePerCoin"/>.
            /// </summary>
            /// <seealso cref="PortfolioEntry.Buy(double, double, long)"/>
            /// <param name="coinId"></param>
            /// <param name="buyAmount"></param>
            /// <param name="buyPricePerCoin"></param>
            /// <param name="unixTimestamp"></param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="PortfolioEntry.Buy(double, double, long)"/>
            /// </exception>
            public void BuyCoin(int coinId, double buyAmount, double buyPricePerCoin, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);

                portfolioEntry.Buy(buyAmount, buyPricePerCoin, unixTimestamp);

                onPortfolioStateChanged();
            }

            /// <summary>
            /// sells <paramref name="sellAmount"/> of coin
            /// with specified <paramref name="coinId"/> for <paramref name="sellPricePerCoin"/>.
            /// </summary>
            /// <seealso cref="PortfolioEntry.Sell(double, double, long)"/>
            /// <param name="coinId"></param>
            /// <param name="sellAmount"></param>
            /// <param name="sellPricePerCoin"></param>
            /// <param name="unixTimestamp">unix timestamp of purchase</param>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="PortfolioEntry.Sell(double, double, long)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            /// <seealso cref="PortfolioEntry.Sell(double, double, long)"/>
            /// </exception>
            public void SellCoin(int coinId, double sellAmount, double sellPricePerCoin, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);

                portfolioEntry.Sell(sellAmount, sellPricePerCoin, unixTimestamp);

                onPortfolioStateChanged();
            }

            /// <summary>
            /// returns holdings of <see cref="PortfolioEntry"/> with specified <paramref name="coinId"/>.
            /// </summary>
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// <param name="coinId"></param>
            /// <returns>
            /// holdings of <see cref="PortfolioEntry"/> with specified <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            public double GetCoinHoldings(int coinId)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                return portfolioEntry.Holdings;
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
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="getPortfolioEntry(int)"/>
            /// </exception>
            public string GetPortfolioEntryDisplayTableString(params int[] coinIds)
            {
                // init portfolio entry table
                PortfolioEntryTable portfolioEntryTable = new PortfolioEntryTable();

                foreach(int coinId in coinIds)
                {
                    // add row corresponding to each portfolio entry associated with specified id
                    PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                    portfolioEntryTable.AddRow(portfolioEntry);
                }

                // return table display string
                string portfolioEntryTableString = portfolioEntryTable.GetTableDisplayString();

                return portfolioEntryTableString;
            }

            /// <summary>
            /// <para>
            /// if <see cref="fileDataSaveThreadRunning"/> is true, checks <see cref="UnsavedStateChange"/>
            /// at regular intervals.
            /// </para>
            /// <para>
            /// if <see cref="UnsavedStateChange"/> is true, saves portfolio data to file,
            /// and sets <see cref="UnsavedStateChange"/> to false.
            /// </para>
            /// </summary>
            private void fileDataSaveTask_Target()
            {
                while(fileDataSaveThreadRunning)
                {
                    if (UnsavedStateChange) // unsaved change to portfolio
                    {
                        try
                        {
                            string jsonString = JsonUtils.SerializeObject(this);
                            FileIOManager.Instance.WriteTextToDataFile(DATA_SAVE_FILE_NAME, jsonString);

                            UnsavedStateChange = false;
                        }
                        catch (Exception exception) // serialization or write to file failed
                        {
                            // notify user about exception
                            ConsoleIOManager.Instance.LogError(
                                "An error occurred while trying to save portfolio data to file.");
                            ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                            // log exception to error log file
                            DataFileSaveException dataFileSaveException = new DataFileSaveException(exception);
                            ExceptionManager.Instance.LogToErrorFile(dataFileSaveException);
                        }
                    }

                    Thread.Sleep(FILE_DATA_SAVE_THREAD_SLEEP_TIME_MILLIS);
                }
            }

            /// <summary>
            /// asserts that <see cref="PortfolioManager"/> is initialized.
            /// </summary>
            /// <param name="operationName">
            /// name of operation that is attempted to be performed
            /// </param>
            /// <exception cref="ManagerNotInitializedException">
            /// thrown if <see cref="PortfolioManager"/> is not initialized.
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
            /// thrown if <see cref="PortfolioManager"/> was not initialized
            /// </exception>
            private static void assertManagerNotInitialized()
            {
                if (instance != null)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            /// <summary>
            /// initializes <see cref="PortfolioEntry"/>s in portfolio with <see cref="CoinTicker"/>s,
            /// if <see cref="CoinTicker"/> corresponding to <see cref="PortfolioEntry"/>'s coin ID is available
            /// in <see cref="CoinTickerManager"/>.
            /// </summary>
            private static void initializePortfolioEntryCoinTickers()
            {
                Dictionary<int, PortfolioEntry>.KeyCollection portfolioEntryCoinIds
                    = instance.coinIdToPortfolioEntry.Keys;

                // update portfolio entries which have corresponding coin tickers available
                foreach (int coinId in portfolioEntryCoinIds)
                {
                    if (CoinTickerManager.Instance.HasCoinTicker(coinId)) // coin ticker available
                    {
                        // get coin ticker
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);

                        // update portfolio entry corresponding to coin id
                        PortfolioEntry portfolioEntry = instance.coinIdToPortfolioEntry[coinId];
                        portfolioEntry.Update(coinTicker);
                    }
                }
            }

            /// <summary>
            /// loads portfolio data from save file.
            /// </summary>
            /// <remarks>
            /// assumes portfolio data save file exists.
            /// </remarks>
            private static void loadDataFromSaveFile()
            {
                try
                {
                    // notify user that loading is about to begin
                    ConsoleIOManager.Instance.LogNotice("Portfolio data file available.");
                    ConsoleIOManager.Instance.LogNotice("Loading portfolio data from file ..");

                    // load data from save file in form of a JSON string
                    string dataFileJsonString = FileIOManager.Instance.ReadTextFromDataFile(DATA_SAVE_FILE_NAME);

                    // parse portfolio manager instance from JSON string
                    instance = JsonUtils.DeserializeObject<PortfolioManager>(dataFileJsonString);

                    // notify user of loading success
                    ConsoleIOManager.Instance.LogNotice("Portfolio data loaded successfully.");
                }
                catch (Exception exception) // error reading from file or deserializing JSON string
                {
                    // notify user of exception
                    ConsoleIOManager.Instance.LogNotice("Could not load data. File might be corrupt.");
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                    // log exception in error log file
                    DataFileLoadException dataFileLoadException = new DataFileLoadException(exception);
                    ExceptionManager.Instance.LogToErrorFile(dataFileLoadException);
                }
            }

            /// <summary>
            /// called when a change has been made to portfolio (e.g adding, buying or selling a coin).
            /// </summary>
            private void onPortfolioStateChanged()
            {
                UnsavedStateChange = true;
            }

            /// <summary>
            /// returns <see cref="PortfolioEntry"/> having <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="PortfolioEntry"/> having <paramref name="coinId"/>.
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNotInPortfolioException">
            /// <seealso cref="assertCoinInPortfolio(int)"/>
            /// </exception>
            private PortfolioEntry getPortfolioEntry(int coinId)
            {
                assertManagerInitialized("getPortfolioEntry");
                assertCoinInPortfolio(coinId);

                PortfolioEntry portfolioEntry = coinIdToPortfolioEntry[coinId];

                return portfolioEntry;
            }

            /// <summary>
            /// <para>
            /// updates <see cref="PortfolioEntry"/>s with IDs contained in <paramref name="updatedCoinIdRange"/>.
            /// </para>
            /// <para>
            /// called when <see cref="CoinTickerManager"/> notified of 
            /// an <see cref="CoinTickerManager.RepositoryUpdatedEvent"/>.
            /// </para> 
            /// </summary>
            /// <seealso cref="updatePortfolioEntries(Range)"/>
            /// <param name="updatedCoinIdRange"></param>
            private void coinTickerManager_RepositoryUpdated(Range updatedCoinIdRange)
            {
                updatePortfolioEntries(updatedCoinIdRange);
            }

            /// <summary>
            /// <para>
            /// updates <see cref="PortfolioEntry"/>s with IDs contained in <paramref name="updatedCoinIdRange"/>.
            /// </para>
            /// </summary>
            /// <seealso cref="PortfolioEntry.Update(CoinTicker)"/>
            /// <param name="updatedCoinIdRange"></param>
            private void updatePortfolioEntries(Range updatedCoinIdRange)
            {
                // for each PortfolioEntry in portfolio,
                // update entry if its coin id is included in the update range
                foreach (int coinId in coinIdToPortfolioEntry.Keys)
                {
                    if (updatedCoinIdRange.IsWithinRange(coinId))
                    {
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);
                        coinIdToPortfolioEntry[coinId].Update(coinTicker);
                    }
                }
            }

            /// <summary>
            /// asserts that coin with <paramref name="coinId"/> exists in portfolio.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="CoinNotInPortfolioException">
            /// thrown if coin with <paramref name="coinId"/> does not exist in portfolio
            /// </exception>
            private void assertCoinInPortfolio(int coinId)
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
            private void assertCoinNotAlreadyInPortfolio(int coinId)
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
            private void assertCoinIdValid(int coinId)
            {
               if(!CoinListingManager.Instance.CoinIdExists(coinId))
                {
                    throw new InvalidCoinIdException(coinId);
                }
            }
        }
    }
}

