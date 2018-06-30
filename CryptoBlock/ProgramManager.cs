using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoBlock.Utils;

namespace CryptoBlock
{
    internal class ProgramManager
    {
        private bool inputAvailable;

        internal void StartProgram()
        {
            ConsoleIOManager.Instance.InputEnabled = false;

            initializeCoinListingManager();

            initializeCoinDataManager(CoinListingManager.Instance.RepositoryCount);

            ConsoleIOManager.Instance.InputEnabled = true;
            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            ConsoleIOManager.Instance.EndOfInputKeyRead += consoleIOManager_LineRead;

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

        private void consoleIOManager_LineRead(string inputLine)
        {
            inputAvailable = true;

            // some padding after user input line
  ////          ConsoleIOManager.Instance.PrintNewLine();
        }

        private void initializeCoinDataManager(int numberOfCoins)
        {
            CoinDataManager.Initialize(numberOfCoins);
            CoinDataManager.Instance.RepositoryInitializedEvent += coinDataManager_RepositoryInitialized;
            CoinDataManager.Instance.StartCoinDataUpdateThread();
        }

        private void coinDataManager_RepositoryInitialized(CoinDataManager coinDataManager)
        {
            ConsoleIOManager.Instance.LogNotice("Coin data repository initialized successfully.");

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
