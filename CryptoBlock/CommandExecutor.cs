using System;
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

        internal static void ExecuteViewDataCommand(string coinNameOrSymbol)
        {
            if(CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol)
                || CoinListingManager.Instance.CoinSymbolExists(coinNameOrSymbol))
            {
                int coinId;

                if (CoinListingManager.Instance.CoinNameExists(coinNameOrSymbol))
                {
                    coinId = CoinListingManager.Instance.GetCoinIdByName(coinNameOrSymbol);
                }
                else // coin symbol provided as argument
                {
                    coinId = CoinListingManager.Instance.GetCoinIdBySymbol(coinNameOrSymbol);
                }

                CoinData coinData = RequestHandler.RequestCoinData(coinId);
                Console.WriteLine(CoinData.TableColumnHeaderString());
                Console.WriteLine(coinData.ToTableRowString());
            }
            else
            {
                string message = string.Format(
                    "Coin with specified name or symbol not found: {0}.",
                    coinNameOrSymbol);
                ConsoleUtils.LogLine(message);
            }
        }
    }
}
