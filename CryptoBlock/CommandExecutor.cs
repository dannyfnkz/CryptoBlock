﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;
using CryptoBlock.Utils;

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
            if (command.Type == Command.eCommandType.ViewCoinData
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

                if(command.Type == Command.eCommandType.ViewCoinData)
                {
                    executeViewCoinDataCommand(coinId);
                }
                else if(command.Type == Command.eCommandType.ViewCoinListing)
                {
                    executeViewCoinListingCommand(coinId);
                }
            }
            else // coin  name or symbol is invalid (does not exist in coin listing repository)
            {
                string message = string.Format(
                    "Coin with specified name or symbol not found: {0}.",
                    coinNameOrSymbol);

                ConsoleUtils.LogLine(message);
            }
        }

        private static void executeViewCoinDataCommand(int coinId)
        {
            try
            {
                CoinData coinData = RequestHandler.RequestCoinData(coinId);

                Console.WriteLine(CoinData.GetTableColumnHeaderString());
                Console.WriteLine(coinData.ToTableRowString());
            }
            catch(RequestHandler.DataRequestException dataRequestException)
            {
                ExceptionManager.Instance.PrintGenericServerDataFetchExceptionMessage();

                // log exception in error log file
                ExceptionManager.Instance.LogException(dataRequestException);
            }

            // some padding
            Console.WriteLine();
        }

        private static void executeViewCoinListingCommand(int coinId)
        {
            // fetching CoinListing guaranteed to be successful as repository is initialized
            // & coin id is associated with an existing coin name / symbol
            CoinListing coinListing = CoinListingManager.Instance.GetCoinListing(coinId);

            Console.WriteLine(CoinListing.GetTableColumnHeaderString());
            Console.WriteLine(coinListing.ToTableRowString());
        }
    }
}