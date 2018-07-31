using CryptoBlock.CommandHandling;
using CryptoBlock.IOManagement;
using CryptoBlock.Utils;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// handles executing server data commands.
        /// </summary>
        public class ServerDataCommandExecutor : CommandExecutor
        {
            /// <summary>
            /// represents an executable server data command.
            /// </summary>
            private abstract class ServerDataCommand : Command
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 20;

                internal ServerDataCommand(string prefix)
                    : base(prefix, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }
            }

            /// <summary>
            /// <para>
            /// prints <see cref="CoinListing"/> data in tabular format.
            /// </para>
            /// <para>
            /// syntax: listing [coin0 name/symbol] ?[coin1 name/symbol] ?[coin2 name/symbol] ...
            /// </para>
            /// </summary>
            private class CoinListingCommand : ServerDataCommand
            {
                private const string PREFIX = "listing";

                internal CoinListingCommand()
                    : base(PREFIX)
                {

                }

                /// <summary>
                /// prints <see cref="CoinListing"/> data corresponding to coin name / symbols
                /// contained in <paramref name="commandArguments"/> in tabular format.
                /// </summary>
                /// <seealso cref="CoinListingManager.FetchCoinIds(string[])"/>
                /// <seealso cref="CoinListingManager.GetCoinListingTableDisplayString(int[])"/>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    try
                    {
                        // fetch coin ids corresponding to coin names / symbols
                        int[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

                        // print coin listing display table containing coin listings corresponding
                        // to fetched coin ids
                        string coinListingTableString =
                            CoinListingManager.Instance.GetCoinListingTableDisplayString(coinIds);
                        ConsoleIOManager.Instance.PrintData(coinListingTableString);
                    }
                    catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                    {
                        ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    }
                }
            }

            /// <summary>
            /// <para>
            /// prints <see cref="CoinTicker"/> data in tabular format.
            /// </para>
            /// <para>
            /// syntax: ticker [coin0 name/symbol] ?[coin1 name/symbol] ?[coin2 name/symbol] ...
            /// </para>
            /// </summary>
            private class CoinTickerCommmand : ServerDataCommand
            {
                private const string PREFIX = "ticker";

                internal CoinTickerCommmand()
                    : base(PREFIX)
                {

                }

                /// <summary>
                /// prints <see cref="CoinTicker"/> data corresponding to coin name / symbols
                /// contained in <paramref name="commandArguments"/> in tabular format.
                /// </summary>
                /// <seealso cref="CoinListingManager.FetchCoinIds(string[])"/>
                /// <seealso cref="CoinTickerManager.GetCoinTickerDisplayTableString(int[])"/>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    try
                    {
                        // fetch coin ids corresponding to coin names / symbols
                        int[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

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
                            ConsoleIOManager.Instance.PrintData(coinTickerTableString);
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
                            ConsoleIOManager.Instance.LogError(errorMessage);
                        }
                    }
                    catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                    {
                        ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    }
                }
            }

            private const string COMMAND_TYPE = "ServerData";

            public ServerDataCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(new CoinTickerCommmand());
                AddCommandPrefixToCommandPair(new CoinListingCommand());
            }

            /// <summary>
            /// returns <see cref="ServerDataCommandExecutor"/> command type.
            /// </summary>
            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}
