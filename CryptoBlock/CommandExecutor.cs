
using System;
using CryptoBlock.CMCAPI;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.TableDisplay;

namespace CryptoBlock
{
    internal static class CommandExecutor
    {
        internal class CommandExecutionException : Exception
        {
            internal CommandExecutionException(string message, Exception innerException)
                 : base(message, innerException)
            {

            }

            internal CommandExecutionException(string message)
                : base(message)
            {

            }

            internal CommandExecutionException(Exception innerException)
                 : base(string.Empty, innerException)
            {

            }

            internal CommandExecutionException() : base()
            {

            }
        }

        internal class InvalidCoinNameException : CommandExecutionException
        {
            internal InvalidCoinNameException(string coinName) : base(formatExceptionMessage(coinName))
            {

            }

            private static string formatExceptionMessage(string coinName)
            {
                return string.Format("Coin name not found: {0}.", coinName);
            }
        }

        internal class InvalidCoinSymbolException : CommandExecutionException
        {
            internal InvalidCoinSymbolException(string coinSymbol) : base(formatExceptionMessage(coinSymbol))
            {

            }

            private static string formatExceptionMessage(string coinName)
            {
                return string.Format("Coin name not found: {0}.", coinName);
            }
        }

        internal static void ExecuteCommand(Command command)
        {
            // call method corresponding to command type
            if (command.Type == Command.eCommandType.ViewCoinTicker
                || command.Type == Command.eCommandType.ViewCoinListing)
            {
                executeViewCoinCommand(command);
            }
        }

        // common preparatory method for all "view coin" commands
        private static void executeViewCoinCommand(Command command)
        {
            string coinNameOrSymbol = command.Arguments[0];

            if (CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol)
                || CoinListingManager.Instance.CoinSymbolExists(coinNameOrSymbol))
            {
                int coinId;

                // coin name provided as argument
                if (CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol))
                {
                    coinId = CoinListingManager.Instance.GetCoinIdByName(coinNameOrSymbol);
                }
                else // coin symbol provided as argument
                {
                    coinId = CoinListingManager.Instance.GetCoinIdBySymbol(coinNameOrSymbol);
                }

                if (command.Type == Command.eCommandType.ViewCoinTicker)
                {
                    executeViewCoinDataCommand(coinId);
                }
                else if (command.Type == Command.eCommandType.ViewCoinListing)
                {
                    executeViewCoinListingCommand(coinId);
                }
            }
            else // coin  name or symbol is invalid (does not exist in coin listing repository)
            {
                string message = string.Format(
                    "Coin with specified name or symbol not found: {0}.",
                    coinNameOrSymbol);

                ConsoleIOManager.Instance.LogError(message);
            }
        }

        // assumes coinId is valid (exists in coin listing repository)
        private static void executeViewCoinDataCommand(int coinId)
        {
            try
            {
                CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinData(coinId);

                // init coin ticker table
                CoinTickerTable coinTickerTable = new CoinTickerTable();
                coinTickerTable.AddCoinTickerRow(coinTicker);

                // display table
                string coinTickerTableString = coinTickerTable.GetTableString();
                ConsoleIOManager.Instance.LogData(coinTickerTableString);
            }
            // CoinTicker of specified coinId does not exist in coin data repository
            catch (CoinTickerManager.CoinIdNotFoundException coinIdNotFoundException)
            {
                if (!CoinTickerManager.Instance.RepositoryInitialized)
                {
                    ConsoleIOManager.Instance.LogError("Coin data repository is not fully initialized yet." +
                        " Please try again a bit later.");
                }
                else // unexpected exception
                {
                    ConsoleIOManager.Instance.LogError("An unexpected error has occurred.");
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                    ExceptionManager.Instance.LogException(coinIdNotFoundException);
                }
            }
        }

        private static void executeViewCoinListingCommand(int coinId)
        {
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
    }
}