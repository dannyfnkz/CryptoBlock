using CryptoBlock.CMCAPI;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class PortfolioManager
        {
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

            public class CoinIdAlreadyInPortfolioException : PortfolioManagerException
            {
                private readonly int coinId;

                public CoinIdAlreadyInPortfolioException(int coinId)
                    : base(formatExceptionMessage(coinId))
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format(
                        "Coin ID '{0}' already exists in portfolio.",
                        coinId);
                }
            }

            public class CoinIdNotInPortfolioException : PortfolioManagerException
            {
                private readonly int coinId;

                public CoinIdNotInPortfolioException(int coinId)
                    : base(formatExceptionMessage(coinId))
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format(
                        "Coin ID '{0}' does not exist in portfolio manager.",
                        coinId);
                }
            }

            private const string DATA_FILE_NAME = "portfolio_data";

            private const int FILE_DATA_SAVE_THREAD_SLEEP_TIME_MILLIS = 10 * 1000;
            private const double MAX_NUMERICAL_VALUE_ALLOWED = 1.0E15;

            private static PortfolioManager instance;

            private Task fileDataSaveTask;
            private bool unsavedStateChange = false;
            private bool fileDataSaveThreadRunning;

            [JsonProperty]
            private readonly Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry 
                = new Dictionary<int, PortfolioEntry>();

            private PortfolioManager()
            {
                // subscribe to coin ticker manager repository update events
                CoinTickerManager.Instance.RepositoryUpdatedEvent += coinTickerManager_RepositoryUpdated;
            }

            [JsonConstructor]
            private PortfolioManager(Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry)
                : this()
            {
                this.coinIdToPortfolioEntry = coinIdToPortfolioEntry;
            }

            public static double MaxNumericalValueAllowed
            {
                get { return MAX_NUMERICAL_VALUE_ALLOWED; }
            }

            public static PortfolioManager Instance
            {
                get { return instance; }
            }

            public static void Initialize()
            {
                if(FileIOManager.Instance.DataFileExists(DATA_FILE_NAME)) // data file available
                {
                    loadDataFromFile();
                }
                else // data file not available
                {
                    instance = new PortfolioManager();
                }

                // initialize portfolio entries with corresponding coin tickers 
                // (available in coin ticker manager)
                initializePortfolioEntryCoinTickers();

                instance.StartFileDataSaveThreadThread();
            }

            [JsonIgnore]
            public int[] CoinIds
            {
                get { return coinIdToPortfolioEntry.Keys.ToArray(); }
            }

            [JsonIgnore] 
            private bool UnsavedStateChange
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get { return unsavedStateChange; }
                [MethodImpl(MethodImplOptions.Synchronized)]
                set { unsavedStateChange = value; }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StartFileDataSaveThreadThread()
            {
                fileDataSaveThreadRunning = true;

                // init and start file data save task
                fileDataSaveTask = new Task(new Action(saveDataToFile));
                fileDataSaveTask.Start();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StopFileDataSaveThreadThread()
            {
                fileDataSaveThreadRunning = false;
            }

            //public PortfolioEntry GetPortfolioEntry(int coinId)
            //{
            //    assertCoinIdInPortfolio(coinId);

            //    return coinIdToPortfolioEntry[coinId];    
            //}

            public bool IsInPortfolio(int coinId)
            {
                return coinIdToPortfolioEntry.Keys.Contains(coinId);
            }

            // assumes coinId is valid
            public void CreatePortfolioEntry(int coinId)
            {
                assertManagerInitialized("CreatePortfolioEntry");
                assertCoinIdNotAlreadyInPortfolio(coinId);

                // get CoinTicker corresponding to coinId, if exists
                CoinTicker coinTicker = CoinTickerManager.Instance.HasCoinTicker(coinId) ?
                    CoinTickerManager.Instance.GetCoinTicker(coinId)
                    : null;

                // create a new portfolio entry and update dictionary
                PortfolioEntry portfolioEntry = new PortfolioEntry(coinId, coinTicker);
                coinIdToPortfolioEntry[coinId] = portfolioEntry;

                onPortfolioStateChanged();
            }

            public void RemovePortfolioEntry(int coinId)
            {
                assertManagerInitialized("RemovePortfolioEntry");
                assertCoinIdInPortfolio(coinId);

                bool portfolioEntryRemoved = coinIdToPortfolioEntry.Remove(coinId);

                if(portfolioEntryRemoved)
                {
                    onPortfolioStateChanged();
                }

                onPortfolioStateChanged();
            }

            public void BuyCoin(int coinId, double buyAmount, double buyPrice, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                portfolioEntry.Buy(buyAmount, buyPrice, unixTimestamp);

                onPortfolioStateChanged();
            }

            // throws PortfolioEntry.InsufficientFundsException
            public void SellCoin(int coinId, double sellAmount, double sellPrice, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                portfolioEntry.Sell(sellAmount, sellPrice, unixTimestamp);

                onPortfolioStateChanged();
            }

            public double GetCoinHoldings(int coinId)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                return portfolioEntry.Holdings;
            }

            public string GetPortfolioEntryDisplayTableString(params int[] coinIds)
            {
                // init portfolio entry table
                PortfolioEntryTable portfolioEntryTable = new PortfolioEntryTable();

                foreach(int coinId in coinIds)
                {
                    // add row corresponding to each portfolio entry associated with specified id
                    PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                    portfolioEntryTable.AddPortfolioEntryRow(portfolioEntry);
                }

                // return table display string
                string portfolioEntryTableString = portfolioEntryTable.GetTableDisplayString();

                return portfolioEntryTableString;
            }

            private void saveDataToFile()
            {
                while(fileDataSaveThreadRunning)
                {
                    if (UnsavedStateChange)
                    {
                        try
                        {
                            string jsonString = JsonConvert.SerializeObject(this);
                            FileIOManager.Instance.WriteTextToDataFile(DATA_FILE_NAME, jsonString);

                            UnsavedStateChange = false;
                        }
                        catch (Exception exception)
                        {
                            ConsoleIOManager.Instance.LogError(
                                "An error occurred while trying to save portfolio data to file.");
                            ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                            ExceptionManager.Instance.LogToErrorFile(exception);
                        }
                    }

                    Thread.Sleep(FILE_DATA_SAVE_THREAD_SLEEP_TIME_MILLIS);
                }
            }

            private static void assertManagerInitialized(string operationName)
            {
                if (instance == null)
                {
                    throw new ManagerNotInitializedException(operationName);
                }
            }

            private static void assertManagerNotInitialized()
            {
                if (instance != null)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            private static void initializePortfolioEntryCoinTickers()
            {
                // update portfolio entries which have corresponding coin tickers available in
                // coin ticker manager
                foreach (int coinId in instance.coinIdToPortfolioEntry.Keys)
                {
                    if (CoinTickerManager.Instance.HasCoinTicker(coinId)) // coin ticker available
                    {
                        // update portfolio entry corresponding to coin id
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);
                        PortfolioEntry portfolioEntry = instance.coinIdToPortfolioEntry[coinId];
                        portfolioEntry.Update(coinTicker);
                    }
                }
            }

            private static void loadDataFromFile()
            {
                try
                {
                    ConsoleIOManager.Instance.LogNotice("Portfolio data file available.");
                    ConsoleIOManager.Instance.LogNotice("Loading portfolio data from file ..");

                    string dataFileText = FileIOManager.Instance.ReadTextFromDataFile(DATA_FILE_NAME);
                    instance = JsonConvert.DeserializeObject<PortfolioManager>(dataFileText);

                    ConsoleIOManager.Instance.LogNotice("Portfolio data loaded successfully.");
                }
                catch (Exception exception)
                {
                    ConsoleIOManager.Instance.LogNotice("Could not load data. File might be corrupt.");
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                    ExceptionManager.Instance.LogToErrorFile(exception);
                }
            }

            private void onPortfolioStateChanged()
            {
                UnsavedStateChange = true;
            }

            private PortfolioEntry getPortfolioEntry(int coinId)
            {
                assertManagerInitialized("getPortfolioEntry");
                assertCoinIdInPortfolio(coinId);

                PortfolioEntry portfolioEntry = coinIdToPortfolioEntry[coinId];
                return portfolioEntry;
            }

            private void coinTickerManager_RepositoryUpdated(Range updatedCoinIdRange)
            {
                // for each portfolio entry,
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

            private void assertCoinIdInPortfolio(int coinId)
            {
                if(!IsInPortfolio(coinId))
                {
                    throw new CoinIdNotInPortfolioException(coinId);
                }
            }

            private void assertCoinIdNotAlreadyInPortfolio(int coinId)
            {
                if(IsInPortfolio(coinId))
                {
                    throw new CoinIdAlreadyInPortfolioException(coinId);
                }
            }
        }
    }
}

