using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using System.Threading;
using CryptoBlock.CommandHandling;
using CryptoBlock.PortfolioManagement;

namespace CryptoBlock
{
    internal class ProgramManager
    {
        private bool userHitReturnKey;

        internal void StartProgram()
        {
            ConsoleIOManager.Instance.RegisterInput = false;

            initializeCoinListingManager();
            initializeCoinTickerManager(CoinListingManager.Instance.RepositoryCount);
            initializePortfolioManager();

            ConsoleIOManager.Instance.RegisterInput = true;
            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            ConsoleIOManager.Instance.EndOfInputKeyRegistered += consoleIOManager_EndOfInputKeyRegistered;

            while (true)
            {
                if(userHitReturnKey)
                {
                    string userCommand = ConsoleIOManager.Instance.FlushInputBuffer();
                    CommandParser.ParseCommand(userCommand);

                    userHitReturnKey = false;
                }
            }
        }

        private void consoleIOManager_EndOfInputKeyRegistered(string inputLine)
        {
            userHitReturnKey = true;
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
                    ExceptionManager.Instance.LogException(repositoryUpdateException);

                    ConsoleIOManager.Instance.LogError("An error occurred while trying to" +
                        " initialize coin listing repository.");
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();
                    ConsoleIOManager.Instance.LogNotice("Retrying ..");

                    Thread.Sleep(5000);
                }
            }

            ConsoleIOManager.Instance.LogNotice("Coin listings repository initialized successfully.");
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
