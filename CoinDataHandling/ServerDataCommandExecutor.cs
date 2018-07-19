using CryptoBlock.CMCAPI;
using CryptoBlock.CommandHandling;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class ServerDataCommandExecutor : CommandExecutor
        {
            private abstract class ServerDataCommand : Command
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;

                internal ServerDataCommand(string prefix)
                    : base(prefix, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }
            }

            private class CoinListingCommand : ServerDataCommand
            {
                private const string PREFIX = "listing";

                internal CoinListingCommand()
                    : base(PREFIX)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // fetching CoinListing guaranteed to be successful as repository is initialized
                        // & coin id is associated with an existing coin name / symbol
                        CoinListing coinListing = CoinListingManager.Instance.GetCoinListing(coinId);

                        // init coin listing table
                        CoinListingTable coinListingTable = new CoinListingTable();
                        coinListingTable.AddCoinListingRow(coinListing);

                        // display table
                        string coinListingTableString = coinListingTable.GetTableString();
                        ConsoleIOManager.Instance.PrintData(coinListingTableString);
                    }
                    catch (CoinListingManager.NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                }
            }

            private class CoinTickerCommmand : ServerDataCommand
            {
                private const string PREFIX = "ticker";

                internal CoinTickerCommmand()
                    : base(PREFIX)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // fetching coinTicker from repository might return null in case 
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);

                        // init coin listing table
                        CoinTickerTable coinTickerTable = new CoinTickerTable();
                        coinTickerTable.AddCoinTickerRow(coinTicker);

                        // display table
                        string coinTickerTableString = coinTickerTable.GetTableString();
                        ConsoleIOManager.Instance.PrintData(coinTickerTableString);
                    }
                    catch (CoinListingManager.NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }

                    // coin id associated with given coin name / symbol does not exist in ticker repository
                    catch (CoinTickerManager.CoinIdNotFoundException coinIdNotFoundException)
                    {
                        // coin ticker repository not initialized yet
                        if (!CoinTickerManager.Instance.RepositoryInitialized)
                        {
                            ConsoleIOManager.Instance.LogError("Coin ticker repository is not fully" +
                                " initialized yet. Please try again a bit later.");
                        }
                        else
                        {
                            // coin ticker repository initialized and coin id not found - 
                            // this means an error occurred during ticker repository update thread run
                            // while trying to fetch ticker data for the coin id associated with the specified
                            // name / symbol
                            ConsoleIOManager.Instance.LogError("An unexpected error has occurred.");
                            ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                            ExceptionManager.Instance.LogException(coinIdNotFoundException);
                        }
                    }
                }
            }

            private const string COMMAND_TYPE = "ServerData";


            public ServerDataCommandExecutor()
            {
                // populate commandPrefixToCommmand dictionary with (prefix, command) pairs
                AddPrefixToCommandPair(new CoinTickerCommmand());
                AddPrefixToCommandPair(new CoinListingCommand());
            }

            protected override string GetCommandType()
            {
                return COMMAND_TYPE;
            }
        }
    }
}
