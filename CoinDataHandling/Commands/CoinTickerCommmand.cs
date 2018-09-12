using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ServerDataManagement.Commands
    {
        /// <summary>
        /// <para>
        /// prints <see cref="CoinTicker"/> data in tabular format.
        /// </para>
        /// <para>
        /// syntax: ticker [coin0 name/symbol] ?[coin1 name/symbol] ?[coin2 name/symbol] ...
        /// </para>
        /// </summary>
        internal class CoinTickerCommmand : ServerDataCommand
        {
            private const string PREFIX = "ticker";

            internal CoinTickerCommmand()
                : base(PREFIX)
            {

            }

            /// <summary>
            /// prints <see cref="CoinTicker"/> data corresponding to coin name / symbols
            /// contained in <paramref name="commandArguments"/> in tabular format.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.FetchCoinIds(string[])"/>
            /// <seealso cref="CoinTickerManager.GetCoinTickerDisplayTableString(int[])"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                try
                {
                    // fetch coin ids corresponding to coin names / symbols
                    long[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

                    // only coin ids which corresponding ticker entry in ticker manager
                    // has been initialized are displayed
                    List<int> coinIdsWithInitalizedTicker = new List<int>();
                    List<string> coinNamesWithoutInitalizedTicker = new List<string>();

                    // get coin ids with initialized ticker data
                    foreach (int coinId in coinIds)
                    {
                        if (CoinTickerManager.Instance.HasCoinTicker(coinId))
                        {
                            coinIdsWithInitalizedTicker.Add(coinId);
                        }
                        else
                        {
                            string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);
                            coinNamesWithoutInitalizedTicker.Add(coinName);
                        }
                    }

                    if (coinIdsWithInitalizedTicker.Count > 0)
                    {
                        // print coin listing display table containing coin listings corresponding
                        // to fetched coin ids
                        string coinTickerTableString =
                            CoinTickerManager.Instance.GetCoinTickerDisplayTableString(
                                coinIdsWithInitalizedTicker.ToArray());
                        ConsoleIOManager.Instance.PrintData(
                            coinTickerTableString,
                            ConsoleIOManager.eOutputReportType.CommandExecution);
                    }

                    // if data for coin ids with uninitialized tickers was requested, 
                    // display an appropriate message to user
                    if (coinNamesWithoutInitalizedTicker.Count > 0)
                    {
                        string errorMessage = StringUtils.Append(
                            "Coin ticker data for the following coin(s) was not yet initialized: ",
                            ", ",
                            coinNamesWithoutInitalizedTicker.ToArray())
                            + ".";
                        ConsoleIOManager.Instance.LogError(
                            errorMessage,
                            ConsoleIOManager.eOutputReportType.CommandExecution);
                    }

                    commandExecutedSuccessfuly = true;
                }
                catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    ConsoleIOManager.Instance.LogError(
                        coinNameOrSymbolNotFoundException.Message,
                        ConsoleIOManager.eOutputReportType.CommandExecution);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}

