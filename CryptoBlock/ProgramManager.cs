using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using System.Threading;
using CryptoBlock.CommandHandling;
using CryptoBlock.PortfolioManagement;
using static CryptoBlock.PortfolioManagement.PortfolioManager;

namespace CryptoBlock
{
    internal class ProgramManager
    {
        private readonly EventWaitHandle userHitReturnKeyWaitHandle = new AutoResetEvent(false);

        internal void StartProgram()
        {
            ConsoleIOManager.Instance.RegisterInput = false;

            initializeCoinListingManager();

            try
            {
                initializePortfolioManager();
            }
            catch (DatabaseCommunicationException databaseCommunicationException)
            {
                ConsoleIOManager.Instance.LogError(
                    "An exception occurred while trying to initialize portfolio data file." +
                    " Program cannot be started.");
                ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                ExceptionManager.Instance.LogToErrorFile(databaseCommunicationException);

                ConsoleIOManager.Instance.showPressAnyKeyToContinueDialog();

                return;
            }

            initializeCoinTickerManager(CoinListingManager.Instance.RepositoryCount);
            
            ConsoleIOManager.Instance.RegisterInput = true;
            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            ConsoleIOManager.Instance.EndOfInputKeyRegistered += consoleIOManager_EndOfInputKeyRegistered;

            while (true)
            {
                // wait till user hits return key
                userHitReturnKeyWaitHandle.WaitOne();
               
                // user hit return key

                // read and parse user command
                string userCommand = ConsoleIOManager.Instance.FlushInputBuffer();
                CommandParser.ParseCommand(userCommand);

                // reset return key wait handle
                userHitReturnKeyWaitHandle.Reset();
            }
        }

        private void consoleIOManager_EndOfInputKeyRegistered(string inputLine)
        {
            userHitReturnKeyWaitHandle.Set();
        }

        private void coinDataManager_RepositoryInitialized(CoinTickerManager coinDataManager)
        {
            ConsoleIOManager.Instance.LogNotice("Coin ticker repository initialized successfully.");
        }

        private void initializeCoinListingManager()
        {
            bool coinListingRepositoryInitialized = false;

            while (!coinListingRepositoryInitialized)
            {
                ConsoleIOManager.Instance.LogNotice("Initializing coin listing repository ..");

                try
                {
                    CoinListingManager.Instance.Initialize();
                    coinListingRepositoryInitialized = true;
                }
                catch (CoinListingManager.RepositoryUpdateException repositoryUpdateException)
                {
                    ExceptionManager.Instance.LogToErrorFile(repositoryUpdateException);

                    ConsoleIOManager.Instance.LogError("An error occurred while trying to" +
                        " initialize coin listing repository.");
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                    ConsoleIOManager.Instance.LogNotice("Retrying ..");

                    Thread.Sleep(5000);
                }
            }

            ConsoleIOManager.Instance.LogNotice("Coin listing repository initialized successfully.");
        }

        private void initializeCoinTickerManager(int numberOfCoins)
        {
            CoinTickerManager.Initialize(numberOfCoins);
            CoinTickerManager.Instance.RepositoryInitializedEvent += coinDataManager_RepositoryInitialized;
        }

        private void initializePortfolioManager()
        {
            PortfolioManager.Initialize();
        }
    }
}
