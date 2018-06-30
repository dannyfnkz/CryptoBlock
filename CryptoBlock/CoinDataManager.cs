using CryptoBlock.CMCAPI;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBlock
{
    internal class CoinDataManager
    {
        internal class CoinDataManagerException : Exception
        {
            internal CoinDataManagerException()
                : base()
            {

            }

            internal CoinDataManagerException(string exceptionMessage)
                : base(exceptionMessage)
            {

            }

            internal CoinDataManagerException(string exceptionMessage, Exception innerException)
                : base(exceptionMessage, innerException)
            {

            }
        }

        internal class InvalidNumberOfCoinsException : CoinDataManagerException
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

        internal class ManagerNotInitializedException : CoinDataManagerException
        {

        }

        internal class ManagerAlreadyInitializedException : CoinDataManagerException
        {
            internal ManagerAlreadyInitializedException()
                : base(formatExceptionMessage())
            {

            }

            private static string formatExceptionMessage()
            {
                return "Coin data manager was already initialized.";
            }
        }

        internal class CoinIdNotFoundException : CoinDataManagerException
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
                    "Coin with id {0} does not exist in repository. Note that it might not have been intialized yet.",
                    coinId);
            }
        }

        internal event Action<CoinDataManager> RepositoryInitializedEvent;

        private const int COIN_DATA_UPDATE_THREAD_SLEEP_TIME = 1000 * 120; // in millis

        private static CoinDataManager instance;


        private Dictionary<int, CoinData> coinIdToCoinData = new Dictionary<int, CoinData>();
        //   private CoinData[] coinDataArray;
        private int numberOfCoinsInRepository;
        private int leastRecentlyUpdatedCoinIndex = 0;
        
        private Task coinDataUpdateTask;
        private bool coinDataUpdateThreadRunning;
        private bool repositoryInitialized;

        internal CoinDataManager(int numberOfCoinsInRepository)
        {
            //     coinDataArray = new CoinData[numberOfCoins];
            this.numberOfCoinsInRepository = numberOfCoinsInRepository;
            coinDataUpdateTask = new Task(new Action(updateCoinDataRepository));
        }

        internal int NumberOfCoinsInRepository
        {
            get { return numberOfCoinsInRepository; }
        }

        internal bool RepositoryInitialized
        {
            get { return repositoryInitialized; }
        }

        internal static CoinDataManager Initialize(int numberOfCoins)
        {
            assertManagerNotInitialized();

            assertValidNumberOfCoins(numberOfCoins);

            instance = new CoinDataManager(numberOfCoins);

            return instance;
        }

        internal static CoinDataManager Instance
        {
            get
            {
                assertManagerInitialized();
                return instance;
            }
        }

        internal bool CoinIdExists(int coinId)
        {
            return coinIdToCoinData.ContainsKey(coinId);
        }

        internal CoinData GetCoinData(int coinId)
        {
            assertCoinIdExists(coinId);
            return coinIdToCoinData[coinId];
        }

        internal void StartCoinDataUpdateThread()
        {
            coinDataUpdateThreadRunning = true;
            coinDataUpdateTask.Start();
        }
        
        internal void StopCoinDataUpdateThread()
        {
            coinDataUpdateThreadRunning = false;
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
            if(!coinIdToCoinData.ContainsKey(coinId))
            {
                throw new CoinIdNotFoundException(coinId);
            }
        }

        private void updateCoinDataRepository()
        {
            while(coinDataUpdateThreadRunning)
            {
                try
                {
                    // fetch data of current coin section
                    CoinData[] currentCoinDataSection = RequestHandler.RequestCoinData(
                        leastRecentlyUpdatedCoinIndex,
                        RequestHandler.CoinDataRequestMaxNumberOfCoins);

                    // update appropriate section in coin data array with newly fetched data
                    int currentCoinDataSectionSize = Math.Min(
                        NumberOfCoinsInRepository - leastRecentlyUpdatedCoinIndex,
                        currentCoinDataSection.Length);

                    foreach(CoinData coinData in currentCoinDataSection)
                    {
                        coinIdToCoinData[coinData.Id] = coinData;
                    }
                    //Array.Copy(
                    //    currentCoinDataSection,
                    //    0, coinDataArray,
                    //    leastRecentlyUpdatedCoinIndex,
                    //    currentCoinDataSectionSize);

                    // set start index of next coin section (0 if current update run is complete)
                    leastRecentlyUpdatedCoinIndex += currentCoinDataSection.Length;

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

                        Thread.Sleep(COIN_DATA_UPDATE_THREAD_SLEEP_TIME);
                    }
                }
                catch(RequestHandler.DataRequestException dataRequestException)
                {
                    handleCoinDataUpdateException(dataRequestException);
                }
            }
        }

        private void handleCoinDataUpdateException(Exception exception)
        {
            ConsoleIOManager.Instance.LogError("An exception occurred while trying to update coin data repository.");
            ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
            ExceptionManager.Instance.LogException(exception);
        }
    }
}
