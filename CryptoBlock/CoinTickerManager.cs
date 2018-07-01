using CryptoBlock.CMCAPI;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    internal class CoinTickerManager
    {
        internal class CoinTickerManagerException : Exception
        {
            internal CoinTickerManagerException()
                : base()
            {

            }

            internal CoinTickerManagerException(string exceptionMessage)
                : base(exceptionMessage)
            {

            }

            internal CoinTickerManagerException(string exceptionMessage, Exception innerException)
                : base(exceptionMessage, innerException)
            {

            }
        }

        internal class InvalidNumberOfCoinsException : CoinTickerManagerException
        {
            int numberOfCoins;

            internal InvalidNumberOfCoinsException(int numberOfCoins)
                : base(formatExceptionMessage())
            {
                this.numberOfCoins = numberOfCoins;
            }

            internal int NumberOfCoins
            {
                get { return numberOfCoins; }
            }

            private static string formatExceptionMessage()
            {
                return "Number of coins must be a positive integer.";
            }
        }

        internal class ManagerNotInitializedException : CoinTickerManagerException
        {

        }

        internal class ManagerAlreadyInitializedException : CoinTickerManagerException
        {
            internal ManagerAlreadyInitializedException()
                : base(formatExceptionMessage())
            {

            }

            private static string formatExceptionMessage()
            {
                return "Coin ticker manager was already initialized.";
            }
        }

        internal class CoinIdNotFoundException : CoinTickerManagerException
        {
            private int coinId;

            internal CoinIdNotFoundException(int coinId)
                : base(formatExceptionMessage(coinId))
            {
                this.coinId = coinId;
            }

            internal int CoinId
            {
                get { return coinId; }
            }

            private static string formatExceptionMessage(int coinId)
            {
                return string.Format(
                    "Coin with id {0} does not exist in repository." +
                    " Note that it might not have been intialized yet.",
                    coinId);
            }
        }

        internal event Action<CoinTickerManager> RepositoryInitializedEvent;

        private const int COIN_TICKER_UPDATE_THREAD_SLEEP_TIME = 1000 * 120; // in millis

        private static CoinTickerManager instance;


        private Dictionary<int, CoinTicker> coinIdToCoinTicker = new Dictionary<int, CoinTicker>();
        //   private CoinTicker[] coinDataArray;
        private int numberOfCoinsInRepository;
        private int leastRecentlyUpdatedCoinIndex = 0;
        
        private Task coinTickerUpdateTask;
        private bool coinTickerUpdateThreadRunning;
        private bool repositoryInitialized;

        internal CoinTickerManager(int numberOfCoinsInRepository)
        {
            this.numberOfCoinsInRepository = numberOfCoinsInRepository;
            coinTickerUpdateTask = new Task(new Action(updateCoinTickerRepository));
        }

        internal int NumberOfCoinsInRepository
        {
            get { return numberOfCoinsInRepository; }
        }

        internal bool RepositoryInitialized
        {
            get { return repositoryInitialized; }
        }

        internal static CoinTickerManager Initialize(int numberOfCoins)
        {
            assertManagerNotInitialized();

            assertValidNumberOfCoins(numberOfCoins);

            instance = new CoinTickerManager(numberOfCoins);

            return instance;
        }

        internal static CoinTickerManager Instance
        {
            get
            {
                assertManagerInitialized();
                return instance;
            }
        }

        internal bool CoinIdExists(int coinId)
        {
            return coinIdToCoinTicker.ContainsKey(coinId);
        }

        internal CoinTicker GetCoinData(int coinId)
        {
            assertCoinIdExists(coinId);
            return coinIdToCoinTicker[coinId];
        }

        internal void StartCoinTickerUpdateThread()
        {
            coinTickerUpdateThreadRunning = true;
            coinTickerUpdateTask.Start();
        }
        
        internal void StopCoinTickerUpdateThread()
        {
            coinTickerUpdateThreadRunning = false;
        }

        private static void assertValidNumberOfCoins(int numberOfCoins)
        {
            if(numberOfCoins <= 0)
            {
                throw new InvalidNumberOfCoinsException(numberOfCoins);
            }
        }

        private static void assertManagerInitialized()
        {
            if(instance == null)
            {
                throw new ManagerNotInitializedException();
            }
        }

        private static void assertManagerNotInitialized()
        {
            if(instance != null)
            {
                throw new ManagerAlreadyInitializedException();
            }
        }

        private void assertCoinIdExists(int coinId)
        {
            if(!coinIdToCoinTicker.ContainsKey(coinId))
            {
                throw new CoinIdNotFoundException(coinId);
            }
        }

        private void updateCoinTickerRepository()
        {
            while(coinTickerUpdateThreadRunning)
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

                    foreach(CoinTicker coinTicker in currentCoinTickerSection)
                    {
                        coinIdToCoinTicker[coinTicker.Id] = coinTicker;
                    }
                    //Array.Copy(
                    //    currentCoinDataSection,
                    //    0, coinDataArray,
                    //    leastRecentlyUpdatedCoinIndex,
                    //    currentCoinDataSectionSize);

                    // set start index of next coin section (0 if current update run is complete)
                    leastRecentlyUpdatedCoinIndex += currentCoinTickerSection.Length;

                    if (leastRecentlyUpdatedCoinIndex > NumberOfCoinsInRepository - 1)
                    {
                        leastRecentlyUpdatedCoinIndex = 0;
                    }

                    if (leastRecentlyUpdatedCoinIndex == 0) // current update run is complete
                    {
                        if(!repositoryInitialized)
                        {
                            repositoryInitialized = true;
                            if(RepositoryInitializedEvent != null)
                            {
                                RepositoryInitializedEvent.Invoke(this);
                            }     
                        }

                        Thread.Sleep(COIN_TICKER_UPDATE_THREAD_SLEEP_TIME);
                    }
                }
                catch(RequestHandler.DataRequestException dataRequestException)
                {
                    handleCoinTickerUpdateException(dataRequestException);
                }
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
