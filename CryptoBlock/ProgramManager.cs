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
        private void initializeStaticCoinDataRepository()
        {          
            bool staticCoinDataRepositoryInitialized = false;
            
            while(!staticCoinDataRepositoryInitialized)
            {
                ConsoleUtils.LogLine("Initializing coin listings repository ..");

                try
                {                    
                    CoinListingManager.Instance.InitializeRepository();
                    staticCoinDataRepositoryInitialized = true;
                }
                catch (CoinListingManager.RepositoryUpdateException repositoryUpdateException)
                {
                    ExceptionManager.Instance.LogException(repositoryUpdateException);

                    ConsoleUtils.LogLine("An error occurred while trying to initialize Coin listings repository");
                    ConsoleUtils.LogLine("Retrying ..");
                    Console.WriteLine();
                    Thread.Sleep(5000);
                }
            }
            
            ConsoleUtils.LogLine("Coin listings repository initialized successfully.");

            // some padding
            Console.WriteLine();
        }

        internal void StartProgram()
        {
            initializeStaticCoinDataRepository();
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
    }
}
