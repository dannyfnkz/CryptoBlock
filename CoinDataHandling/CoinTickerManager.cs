using CryptoBlock.CMCAPI;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class CoinTickerManager
        {
            public class CoinTickerManagerException : Exception
            {
                public CoinTickerManagerException()
                    : base()
                {

                }

                public CoinTickerManagerException(string exceptionMessage)
                    : base(exceptionMessage)
                {

                }

                public CoinTickerManagerException(string exceptionMessage, Exception innerException)
                    : base(exceptionMessage, innerException)
                {

                }
            }

            public class InvalidNumberOfCoinsException : CoinTickerManagerException
            {
                int numberOfCoins;

                public InvalidNumberOfCoinsException(int numberOfCoins)
                    : base(formatExceptionMessage())
                {
                    this.numberOfCoins = numberOfCoins;
                }

                public int NumberOfCoins
                {
                    get { return numberOfCoins; }
                }

                private static string formatExceptionMessage()
                {
                    return "Number of coins must be a positive integer.";
                }
            }

            public class ManagerNotInitializedException : CoinTickerManagerException
            {

            }

            public class ManagerAlreadyInitializedException : CoinTickerManagerException
            {
                public ManagerAlreadyInitializedException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Coin ticker manager was already initialized.";
                }
            }

            public class CoinIdNotFoundException : CoinTickerManagerException
            {
                private int coinId;

                public CoinIdNotFoundException(int coinId)
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
                        "Coin with id {0} does not exist in coin ticker repository." +
                        " Note that it might not have been intialized yet.",
                        coinId);
                }
            }

            public event Action<CoinTickerManager> RepositoryInitializedEvent;
            public event Action<Range> RepositoryUpdatedEvent;

            private const int COIN_TICKER_UPDATE_THREAD_SLEEP_TIME = 1000 * 120; // in millis

            private static CoinTickerManager instance;


            private Dictionary<int, CoinTicker> coinIdToCoinTicker = new Dictionary<int, CoinTicker>();
            //   private CoinTicker[] coinDataArray;
            private int numberOfCoinsInRepository;
            private int leastRecentlyUpdatedCoinIndex = 0;

            private Task coinTickerUpdateTask;
            private bool coinTickerUpdateThreadRunning;
            private bool repositoryInitialized;

            public CoinTickerManager(int numberOfCoinsInRepository)
            {
                this.numberOfCoinsInRepository = numberOfCoinsInRepository;
                coinTickerUpdateTask = new Task(new Action(updateCoinTickerRepository));
            }

            public int NumberOfCoinsInRepository
            {
                get { return numberOfCoinsInRepository; }
            }

            public bool RepositoryInitialized
            {
                get { return repositoryInitialized; }
            }

            public static CoinTickerManager Initialize(int numberOfCoins)
            {
                assertManagerNotInitialized();

                assertValidNumberOfCoins(numberOfCoins);

                instance = new CoinTickerManager(numberOfCoins);

                return instance;
            }

            public static CoinTickerManager Instance
            {
                get
                {
                    assertManagerInitialized();
                    return instance;
                }
            }

            public bool CoinIdExists(int coinId)
            {
                return coinIdToCoinTicker.ContainsKey(coinId);
            }

            public CoinTicker GetCoinTicker(int coinId)
            {
                assertCoinIdExists(coinId);

                return coinIdToCoinTicker[coinId];
            }

            public void StartCoinTickerUpdateThread()
            {
                coinTickerUpdateThreadRunning = true;
                coinTickerUpdateTask.Start();
            }

            public void StopCoinTickerUpdateThread()
            {
                coinTickerUpdateThreadRunning = false;
            }

            private static void assertValidNumberOfCoins(int numberOfCoins)
            {
                if (numberOfCoins <= 0)
                {
                    throw new InvalidNumberOfCoinsException(numberOfCoins);
                }
            }

            private static void assertManagerInitialized()
            {
                if (instance == null)
                {
                    throw new ManagerNotInitializedException();
                }
            }

            private static void assertManagerNotInitialized()
            {
                if (instance != null)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            private void assertCoinIdExists(int coinId)
            {
                if (!coinIdToCoinTicker.ContainsKey(coinId))
                {
                    throw new CoinIdNotFoundException(coinId);
                }
            }

            private void updateCoinTickerRepository()
            {
                while (coinTickerUpdateThreadRunning)
                {
                    try
                    {
                        // fetch data of current coin section
                        CoinTicker[] currentCoinTickerSection = RequestHandler.RequestCoinTicker(
                            leastRecentlyUpdatedCoinIndex,
                            RequestHandler.CoinDataRequestMaxNumberOfCoins);

                        // update appropriate section in coin data array with newly fetched data
                        int currentCoinDataSectionSize = Math.Min(
                            NumberOfCoinsInRepository - leastRecentlyUpdatedCoinIndex,
                            currentCoinTickerSection.Length);

                        foreach (CoinTicker coinTicker in currentCoinTickerSection)
                        {
                            coinIdToCoinTicker[coinTicker.Id] = coinTicker;
                        }

                        // calculate range of updated coin IDs
                        int lowerBound = leastRecentlyUpdatedCoinIndex;
                        int upperBound = leastRecentlyUpdatedCoinIndex + currentCoinDataSectionSize - 1;
                        Range updatedCoinIdRange = new Range(lowerBound, upperBound);

                        // raise repository update event
                        onRepositoryUpdated(updatedCoinIdRange);

                        // set start index of next coin section (0 if current update run is complete)
                        leastRecentlyUpdatedCoinIndex += currentCoinTickerSection.Length;

                        if (leastRecentlyUpdatedCoinIndex > NumberOfCoinsInRepository - 1)
                        {
                            leastRecentlyUpdatedCoinIndex = 0;
                        }

                        if (leastRecentlyUpdatedCoinIndex == 0) // current update run is complete
                        {
                            // if repository was not yet initialized, raise repository initialized event
                            if (!repositoryInitialized)
                            {
                                repositoryInitialized = true;
                                onRepositoryInitialized();
                            }

                            Thread.Sleep(COIN_TICKER_UPDATE_THREAD_SLEEP_TIME);
                        }
                    }
                    catch (RequestHandler.DataRequestException dataRequestException)
                    {
                        handleCoinTickerUpdateException(dataRequestException);
                    }
                }
            }

            private void onRepositoryUpdated(Range updateCoinIdRange)
            {
                if(RepositoryUpdatedEvent != null)
                {
                    RepositoryUpdatedEvent.Invoke(updateCoinIdRange);
                }
            }

            private void onRepositoryInitialized()
            {
                if (RepositoryInitializedEvent != null)
                {
                    RepositoryInitializedEvent.Invoke(this);
                }
            }

            private void handleCoinTickerUpdateException(Exception exception)
            {
                ConsoleIOManager.Instance.LogError("An exception occurred while trying to update coin data repository.");
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                ExceptionManager.Instance.LogException(exception);
            }
        }
    }

}
