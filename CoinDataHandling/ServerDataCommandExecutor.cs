using CryptoBlock.CommandHandling;
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
                private const int MAX_NUMBER_OF_ARGUMENTS = 20;

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
                            CoinListingManager.Instance.GetCoinListingDisplayTableString(coinIds);
                        ConsoleIOManager.Instance.PrintData(coinListingTableString);
                    }
                    catch (CoinListingManager.NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
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
                                "Ticker Data for the following coin(s) was not yet initialized: ",
                                ", ",
                                coinNamesWithoutInitalizedTicker.ToArray())
                                + ".";
                            ConsoleIOManager.Instance.LogError(errorMessage);
                        }
                    }
                    catch (CoinListingManager.NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }

                    //// coin id associated with given coin name / symbol does not exist in ticker repository
                    //catch (CoinTickerManager.CoinIdNotFoundException coinIdNotFoundException)
                    //{
                    //    // coin ticker repository not initialized yet
                    //    if (!CoinTickerManager.Instance.RepositoryInitialized)
                    //    {
                    //        ConsoleIOManager.Instance.LogError("Coin ticker repository is not fully" +
                    //            " initialized yet. Please try again a bit later.");
                    //    }
                    //    else
                    //    {
                    //        // coin ticker repository initialized and coin id not found - 
                    //        // this means an error occurred during ticker repository update thread run
                    //        // while trying to fetch ticker data for the coin id associated with the specified
                    //        // name / symbol
                    //        ConsoleIOManager.Instance.LogError("An unexpected error has occurred.");
                    //        ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage();

                    //        ExceptionManager.Instance.LogException(coinIdNotFoundException);
                    //    }
                    //}
                }
            }

            private const string COMMAND_TYPE = "ServerData";

            public ServerDataCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(new CoinTickerCommmand());
                AddCommandPrefixToCommandPair(new CoinListingCommand());
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}
