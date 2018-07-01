using System.Threading;

namespace CryptoBlock
{
    internal class ProgramManager
    {
        private bool inputAvailable;

        internal void StartProgram()
        {
            ConsoleIOManager.Instance.RegisterInput = false;

            initializeCoinListingManager();

            initializeCoinDataManager(CoinListingManager.Instance.RepositoryCount);

            ConsoleIOManager.Instance.RegisterInput = true;
            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            ConsoleIOManager.Instance.EndOfInputKeyRegistered += consoleIOManager_EndOfInputKeyRegistered;

            while (true)
            {
         //       ConsoleIOManager.Instance.ReadKeyIfAvailable();

                if(inputAvailable)
                {
                    // some padding
   ////                 ConsoleIOManager.Instance.PrintNewLine();

                    string userCommand = ConsoleIOManager.Instance.FlushInputBuffer();
                    CommandParser.ParseCommand(userCommand);

                    inputAvailable = false;

                    // some padding
       ////             ConsoleIOManager.Instance.PrintNewLine();
                }
            }
        }

        private void consoleIOManager_EndOfInputKeyRegistered(string inputLine)
        {
            inputAvailable = true;

            // some padding after user input line
  ////          ConsoleIOManager.Instance.PrintNewLine();
        }

        private void initializeCoinDataManager(int numberOfCoins)
        {
            CoinTickerManager.Initialize(numberOfCoins);
            CoinTickerManager.Instance.RepositoryInitializedEvent += coinDataManager_RepositoryInitialized;
            CoinTickerManager.Instance.StartCoinTickerUpdateThread();
        }

        private void coinDataManager_RepositoryInitialized(CoinTickerManager coinDataManager)
        {
            ConsoleIOManager.Instance.LogNotice("Coin ticker repository initialized successfully.");

            // some padding
  ////          ConsoleIOManager.Instance.PrintNewLine();
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

            // some padding
   ////         ConsoleIOManager.Instance.PrintNewLine();
        }
    }
}
