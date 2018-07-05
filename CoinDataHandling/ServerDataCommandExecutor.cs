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
                protected class CoinNameOrSymbolNotFoundException : CommandExecutionException
                {
                    internal CoinNameOrSymbolNotFoundException(string coinNameOrSymbol)
                        : base(formatExceptionMessage(coinNameOrSymbol))
                    {

                    }

                    private static string formatExceptionMessage(string coinNameOrSymbol)
                    {
                        return string.Format(
                            "Coin with specified name or symbol not found: {0}.",
                            coinNameOrSymbol);
                    }
                }

                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;

                internal ServerDataCommand(string prefix)
                    : base(prefix, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                protected int GetCoinIdByNameOrSymbol(string coinNameOrSymbol)
                {
                    int coinId;

                    if (CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol)
                        || CoinListingManager.Instance.CoinSymbolExists(coinNameOrSymbol))
                    {
                        // coin id found
                        // coin name provided as argument
                        if (CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol))
                        {
                            coinId = CoinListingManager.Instance.GetCoinIdByName(coinNameOrSymbol);
                        }
                        else // coin symbol provided as argument
                        {
                            coinId = CoinListingManager.Instance.GetCoinIdBySymbol(coinNameOrSymbol);
                        }

                        return coinId;
                    }
                    else // coin id corresponding to name or symbol not found
                    {
                        throw new CoinNameOrSymbolNotFoundException(coinNameOrSymbol);
                    }
                }
            }

            private class CoinListingCommand : ServerDataCommand
            {
                internal CoinListingCommand()
                    : base("listing")
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if(invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        int coinId = GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // fetching CoinListing guaranteed to be successful as repository is initialized
                        // & coin id is associated with an existing coin name / symbol
                        CoinListing coinListing = CoinListingManager.Instance.GetCoinListing(coinId);

                        // init coin listing table
                        CoinListingTable coinListingTable = new CoinListingTable();
                        coinListingTable.AddCoinListingRow(coinListing);

                        // display table
                        string coinListingTableString = coinListingTable.GetTableString();
                        ConsoleIOManager.Instance.LogData(coinListingTableString);
                    }
                    catch (CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                    {
                        // coin with specified name / symbol not found
                        ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    }
                }
            }

            private class CoinTickerCommmand : ServerDataCommand
            {
                internal CoinTickerCommmand()
                    : base("ticker")
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
                        int coinId = GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // fetching coinTicker from repository might return null in case 
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);

                        // init coin listing table
                        CoinTickerTable coinTickerTable = new CoinTickerTable();
                        coinTickerTable.AddCoinTickerRow(coinTicker);

                        // display table
                        string coinTickerTableString = coinTickerTable.GetTableString();
                        ConsoleIOManager.Instance.LogData(coinTickerTableString);
                    }
                    catch(CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                    {
                        // coin with specified name / symbol not found
                        ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    }

                    // coin id not found in ticker repository
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

            public class InvalidServerDataCommandException : InvalidCommandSyntaxException
            {
                public InvalidServerDataCommandException()
                    : base("ServerData")
                {

                }

            }

            private static readonly Dictionary<string, Command> commandPrefixToCommmand
                = new Dictionary<string, Command>
                {
                { "ticker", new CoinTickerCommmand() },
                { "listing", new CoinListingCommand() }
                };

            public override bool IsValidCommand(string userInputLowercase)
            {
                // check if user input starts with a recognized command prefix
                foreach(string commandPrefix in commandPrefixToCommmand.Keys)
                {
                    if(userInputLowercase.StartsWith(commandPrefix))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override string GetCommandPrefix(string userInputLowercase)
            {
                // if user input starts with a recognized prefix, get prefix
                string prefix = StringUtils.GetPrefixIfStartsWith(
                    userInputLowercase,
                    commandPrefixToCommmand.Keys.ToArray());

                if (prefix != null) // valid command
                {
                    return prefix;
                }
                else // invalid command
                {
                    throw new InvalidServerDataCommandException();
                }
            }
            
            public override void ExecuteCommand(string commandPrefix, string[] commandArguments)
            {
                if(!IsValidCommand(commandPrefix))
                {
                    throw new InvalidServerDataCommandException();
                }

                // get matching command
                Command command = commandPrefixToCommmand[commandPrefix];

                command.ExecuteCommand(commandArguments);
            }
        }
    }
}
