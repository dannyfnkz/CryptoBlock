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
        internal void StartProgram()
        {
            initializeCoinListingRepository();
            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            while(true)
            {
                string userCommand = Console.ReadLine();

                // padding
                Console.WriteLine();

                CommandParser.ParseCommand(userCommand);
            }
        }

        private void initializeCoinListingRepository()
        {
            bool coinListingRepositoryInitialized = false;

            while (!coinListingRepositoryInitialized)
            {
                ConsoleUtils.LogLine("Initializing coin listing repository ..");

                try
                {
                    CoinListingManager.Instance.InitializeRepository();
                    coinListingRepositoryInitialized = true;
                }
                catch (CoinListingManager.RepositoryUpdateException repositoryUpdateException)
                {
                    ExceptionManager.Instance.LogException(repositoryUpdateException);

                    ExceptionManager.Instance.PrintGenericCoinLisitingRepositoryInitializationExceptionMessage();
                    ConsoleUtils.LogLine("Retrying ..");
                    Console.WriteLine();

                    Thread.Sleep(5000);
                }
            }

            ConsoleUtils.LogLine("Coin listings repository initialized successfully.");

            // some padding
            Console.WriteLine();
        }
    }
}
