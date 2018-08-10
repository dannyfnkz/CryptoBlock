using CryptoBlock.CMCAPI;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static CryptoBlock.CMCAPI.RequestHandler;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// manages application's <see cref="CoinTicker"/> repository.
        /// </summary>
        public class CoinTickerManager
        {
            /// <summary>
            /// thrown if an exception occurs while performing a <see cref="CoinTickerManager"/> operation.
            /// </summary>
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

            /// <summary>
            /// thrown if <see cref="CoinTickerManager"/> is attempted to be initialized with a non-positive
            /// number of coins.
            /// </summary>
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

            /// <summary>
            /// thrown if an operation on <see cref="CoinTickerManager"/> is attempted to be performed before
            /// manager has been initialized.
            /// </summary>
            public class ManagerNotInitializedException : CoinTickerManagerException
            {
                private string requestedPropertyName;

                public ManagerNotInitializedException(string requestedPropertyName)
                    : base(formatExceptionMessage(requestedPropertyName))
                {
                    this.requestedPropertyName = requestedPropertyName;
                }

                public string RequestedPropertyName
                {
                    get { return requestedPropertyName; }
                }

                private static string formatExceptionMessage(string requestedPropertyName)
                {
                    return string.Format(
                        "Coin ticker manager must be initialized before the following property / operation is" +
                        " requested / performed: {0}.",
                        requestedPropertyName);
                }
            }

            /// <summary>
            /// thrown if <see cref="CoinTickerManager"/> is attempted to be initialized after already being
            /// initialized before.
            /// </summary>
            public class ManagerAlreadyInitializedException : CoinTickerManagerException
            {
                public ManagerAlreadyInitializedException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Coin ticker manager has already been initialized.";
                }
            }

            /// <summary>
            /// thrown if a <see cref="CoinTicker"/> with specified coin ID was not found in coin ticker repository.
            /// </summary>
            public class CoinIdNotFoundException : CoinTickerManagerException
            {
                private long coinId;

                public CoinIdNotFoundException(long coinId)
                    : base(formatExceptionMessage(coinId))
                {
                    this.coinId = coinId;
                }

                public long CoinId
                {
                    get { return coinId; }
                }

                private static string formatExceptionMessage(long coinId)
                {
                    return string.Format(
                        "Coin with id {0} does not exist in coin ticker repository." +
                        " Note that it might not have been intialized yet.",
                        coinId);
                }
            }

            public event Action<CoinTickerManager> RepositoryInitializedEvent;
            public event Action<Range> RepositoryUpdatedEvent;

            private const int COIN_TICKER_UPDATE_DELAY_TIME_UNTIL_NEXT_RUN = 1000 * 120; // in millis
            private const int COIN_TICKER_UPDATE_DELAY_TIME_AFTER_EXCEPTION = 1000 * 15; // in millis

            private static CoinTickerManager instance;

            private Dictionary<long, CoinTicker> coinIdToCoinTicker = new Dictionary<long, CoinTicker>();
            //   private CoinTicker[] coinDataArray;
            private int numberOfCoinsInRepository;
            private int leastRecentlyUpdatedCoinIndex = 0;

            private Task repositoryUpdateTask;
            private bool repositoryUpdateThreadRunning;
            private bool repositoryInitialized;


            public CoinTickerManager(int numberOfCoinsInRepository)
            {
                this.numberOfCoinsInRepository = numberOfCoinsInRepository;   
            }

            /// <summary>
            /// returns number of coins in repository.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public int NumberOfCoinsInRepository
            {
                get
                {
                    assertManagerInitialized("NumberOfCoinsInRepository");

                    return numberOfCoinsInRepository;
                }
            }

            /// <summary>
            /// returns whether <see cref="CoinTicker"/> repository was initialized.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool RepositoryInitialized
            {
                get
                {
                    assertManagerInitialized("RepositoryInitialized");

                    return repositoryInitialized;
                }
            }

            /// <summary>
            /// returns whether <see cref="CoinTicker"/> repository update thread is running.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool RepositoryUpdateThreadRunning
            {
                get
                {
                    assertManagerInitialized("RepositoryUpdateThreadRunning");

                    return repositoryUpdateThreadRunning;
                }
            }

            /// <summary>
            /// returns whether <see cref="CoinTickerManager"/> was initialized.
            /// </summary>
            private static bool ManagerInitialized
            {
                get { return instance != null; }
            }

            /// <summary>
            /// global <see cref="CoinTickerManager"/> instance.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public static CoinTickerManager Instance
            {
                get
                {
                    assertManagerInitialized("Instance");

                    return instance;
                }
            }

            /// <summary>
            /// initializes <see cref="CoinTickerManager"/> with specified <paramref name="numberOfCoins"/>,
            /// and starts repository update thread.
            /// </summary>
            /// <param name="numberOfCoins"></param>
            /// <returns></returns>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// <seealso cref="assertManagerNotInitialized"/>
            /// </exception>
            /// <exception cref="InvalidNumberOfCoinsException">
            /// <seealso cref="assertValidNumberOfCoins(int)"/>
            /// </exception>
            public static CoinTickerManager Initialize(int numberOfCoins)
            {
                assertManagerNotInitialized();

                assertValidNumberOfCoins(numberOfCoins);

                instance = new CoinTickerManager(numberOfCoins);

                instance.StartUpdateThread();

                return instance;
            }

            /// <summary>
            /// returns whether <see cref="CoinTicker"/> with specified <paramref name="coinId"/> exists in
            /// repository.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// true if <see cref="CoinTicker"/> with specified <paramref name="coinId"/> exists in
            /// repository,
            /// else false
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool HasCoinTicker(long coinId)
            {
                assertManagerInitialized("HasCoinTicker");

                return coinIdToCoinTicker.ContainsKey(coinId);
            }

            /// <summary>
            /// returns <see cref="CoinTicker"/> having specified <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="CoinTicker"/> having specified <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdNotFoundException">
            /// <seealso cref="assertCoinTickerExists(long)"/>
            /// </exception>
            public CoinTicker GetCoinTicker(long coinId)
            {
                assertManagerInitialized("GetCoinTicker");
                assertCoinTickerExists(coinId);

                return coinIdToCoinTicker[coinId];
            }

            /// <summary>
            /// returns <see cref="CoinTicker"/>s having corresponding <paramref name="coinIds"/>.
            /// </summary>
            /// <param name="coinIds"></param>
            /// <returns>
            /// <see cref="CoinTicker"/> having corresponding <paramref name="coinIds"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdNotFoundException">
            /// <seealso cref="GetCoinTicker(int)"/>
            /// </exception>
            public CoinTicker[] GetCoinTickers(params int[] coinIds)
            {
                assertManagerInitialized("GetCoinTickers");

                CoinTicker[] coinTickers = new CoinTicker[coinIds.Length];

                for(int i = 0; i < coinIds.Length; i++)
                {
                    coinTickers[i] = GetCoinTicker(coinIds[i]);
                }

                return coinTickers;
            }

            /// <summary>
            /// returns string representation of <see cref="CoinTicker"/> repository in tabular format,
            /// containing data of <see cref="CoinTicker"/>s with specified <paramref name="coinIds"/>.
            /// </summary>
            /// <param name="coinIds"></param>
            /// <returns>
            /// string representation of <see cref="CoinTicker"/> repository in tabular format,
            /// containing data of <see cref="CoinTicker"/>s with specified <paramref name="coinIds"/>.
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized"/>
            /// </exception>
            public string GetCoinTickerDisplayTableString(params int[] coinIds)
            {
                assertManagerInitialized("GetCoinTickerDisplayTableString");

                // init coin ticker table
                CoinTickerTable coinTickerTable = new CoinTickerTable();

                foreach (int coinId in coinIds)
                {
                    // add row corresponding to each coin ticker associated with specified id
                    CoinTicker coinTicker = GetCoinTicker(coinId);
                    coinTickerTable.AddRow(coinTicker);
                }

                // return table display string
                string coinTickerTableString = coinTickerTable.GetTableDisplayString();

                return coinTickerTableString;
            }

            /// <summary>
            /// starts the <see cref="CoinTicker"/> repository update thread.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StartUpdateThread()
            {
                assertManagerInitialized("StartUpdateThread");

                repositoryUpdateThreadRunning = true;

                repositoryUpdateTask = new Task(new Action(coinTickerUpdateTask_Target));
                repositoryUpdateTask.Start();
            }

            /// <summary>
            /// stops the <see cref="CoinTicker"/> repository update thread.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StopUpdateThread()
            {
                assertManagerInitialized("StopUpdateThread");

                repositoryUpdateThreadRunning = false;
            }

            /// <summary>
            /// asserts that <paramref name="numberOfCoins"/> specified for an operation is valid.
            /// </summary>
            /// <exception cref="InvalidNumberOfCoinsException">
            /// thrown if <paramref name="numberOfCoins"/> specified for an operation was not valid.
            /// </exception>
            /// <param name="numberOfCoins"></param>
            private static void assertValidNumberOfCoins(int numberOfCoins)
            {
                if (numberOfCoins <= 0)
                {
                    throw new InvalidNumberOfCoinsException(numberOfCoins);
                }
            }

            /// <summary>
            /// asserts that <see cref="CoinTickerManager"/> has been initialized.
            /// </summary>
            /// <exception cref="ManagerNotInitializedException">
            /// thrown if <see cref="CoinTickerManager"/> is yet to be initialized
            /// </exception>
            /// <param name="propertyOrOperationName"></param>
            private static void assertManagerInitialized(string propertyOrOperationName)
            {
                if (!ManagerInitialized)
                {
                    throw new ManagerNotInitializedException(propertyOrOperationName);
                }
            }

            /// <summary>
            /// asserts that <see cref="CoinTickerManager"/> has not yet been initialized.
            /// </summary>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// thrown if <see cref="CoinTickerManager"/> has already been initialized
            /// </exception>
            private static void assertManagerNotInitialized()
            {
                if (ManagerInitialized)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            /// <summary>
            /// asserts that <see cref="CoinTicker"/> corresponding to <paramref name="coinId"/>
            /// exists in coin ticker repository.
            /// </summary>
            /// <param name="coinId"></param>
            /// <exception cref="CoinIdNotFoundException">
            /// thrown if <see cref="CoinTicker"/> corresponding to <paramref name="coinId"/>
            /// does not exist in coin ticker repository.
            /// </exception>
            private void assertCoinTickerExists(long coinId)
            {
                if (!coinIdToCoinTicker.ContainsKey(coinId))
                {
                    throw new CoinIdNotFoundException(coinId);
                }
            }

            /// <summary>
            /// if <see cref="coinTickerUpdateThreadRunning"/> is true,
            /// updates <see cref="CoinTickerRequestMaxNumberOfCoins"/> least recently updated
            /// <see cref="CoinTicker"/>s in repository.
            /// </summary>
            /// <seealso cref="RequestHandler.RequestCoinTicker(int, int, eSortType)"/>
            /// <seealso cref="handleCoinTickerRepositoryUpdateException(DataRequestException)"/>
            private void coinTickerUpdateTask_Target()
            {
                while (repositoryUpdateThreadRunning)
                {
                    try
                    {
                        // fetch data of current coin section
                        CoinTicker[] currentCoinTickerSection = RequestHandler.RequestCoinTicker(
                            leastRecentlyUpdatedCoinIndex,
                            CoinTickerRequestMaxNumberOfCoins);

                        // update appropriate section in coin data array with newly fetched data
                        int currentCoinDataSectionSize = Math.Min(
                            NumberOfCoinsInRepository - leastRecentlyUpdatedCoinIndex,
                            currentCoinTickerSection.Length);

                        // overwrite corresponding old CoinTickers in coinId-to-CoinTicker dictionary
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
                            else // notify user that update run is complete
                            {
                                ConsoleIOManager.Instance.LogNotice("Coin ticker repository updated.");
                            }

                            // wait until next update run
                            Task.Delay(COIN_TICKER_UPDATE_DELAY_TIME_UNTIL_NEXT_RUN).Wait();
                        }
                    }
                    // exception while requesting CoinTicker data from server
                    catch (DataRequestException dataRequestException) 
                    {
                        handleCoinTickerRepositoryUpdateException(dataRequestException);

                        try
                        {
                            Task.Delay(COIN_TICKER_UPDATE_DELAY_TIME_AFTER_EXCEPTION).Wait();
                        }
                        catch(AggregateException) // thrown by Task.Delay(int).wait()
                        {

                        }
                    }
                    catch(AggregateException) // thrown by Task.Delay(int).wait()
                    {

                    }
                }
            }

            /// <summary>
            /// raises <see cref="RepositoryUpdatedEvent"/>.
            /// </summary>
            /// <param name="updateCoinIdRange"></param>
            private void onRepositoryUpdated(Range updateCoinIdRange)
            {
                if(RepositoryUpdatedEvent != null)
                {
                    RepositoryUpdatedEvent.Invoke(updateCoinIdRange);
                }
            }

            /// <summary>
            /// raises <see cref="RepositoryInitializedEvent"/>.
            /// </summary>
            private void onRepositoryInitialized()
            {
                if (RepositoryInitializedEvent != null)
                {
                    RepositoryInitializedEvent.Invoke(this);
                }
            }

            /// <summary>
            /// handles a <paramref name="dataRequestException"/> which occurred during coin ticker repository
            /// update.
            /// </summary>
            /// <param name="dataRequestException"></param>
            private void handleCoinTickerRepositoryUpdateException(DataRequestException dataRequestException)
            {
                // notify user of excpetion
                ConsoleIOManager.Instance.LogError(
                    "An exception occurred while trying to update coin ticker repository.");
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                // log exception to file
                ExceptionManager.Instance.LogToErrorFile(dataRequestException);
            }
        }
    }

}
