using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement;
using CryptoBlock.PortfolioManagement.Commands;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        /// <summary>
        /// <para>
        /// prints portfolio data in a tabular form.
        /// </para>
        /// <para>
        /// syntax: portfolio view ?[coin0 name/symbol] ?[coin1 name/symbol] ...
        /// </para>
        /// </summary>
        internal class PortfolioViewCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 10;

            // command sub-prefix
            private const string SUBPREFIX = "view";

            internal PortfolioViewCommand()
                : base(SUBPREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// prints portfolio data corresponding to coin name / symbols
            /// contained in <paramref name="commandArguments"/> (or all coins in portfolio if
            /// <paramref name="commandArguments"/>.Length == 0) in tabular format.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.FetchCoinIds(string[])"/>
            /// <seealso cref="PortfolioManager.GetPortfolioEntryDisplayTableString(int[])"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                try
                {
                    // only coin ids which have a corresponding portfolio entry are displayed
                    List<long> coinIdsWithPortfolioEntry = new List<long>();
                    List<string> coinNamesWithoutPortfolioEntry = new List<string>();

                    if (commandArguments.Length == 0)
                    {
                        // if no command args are provided, display all entries in portfolio
                        long[] allCoinIdsInPortfolio = PortfolioManager.Instance.CoinIds;
                        coinIdsWithPortfolioEntry.AddRange(allCoinIdsInPortfolio);
                    }
                    else // command args are provided
                    {
                        // fetch coin ids corresponding to coin names / symbols
                        long[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

                        // get coin ids with initialized ticker data
                        foreach (int coinId in coinIds)
                        {
                            if (PortfolioManager.Instance.IsInPortfolio(coinId))
                            {
                                coinIdsWithPortfolioEntry.Add(coinId);
                            }
                            else
                            {
                                string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);
                                coinNamesWithoutPortfolioEntry.Add(coinName);
                            }
                        }
                    }

                    if (coinIdsWithPortfolioEntry.Count > 0)
                    {
                        // print coin PortfolioEntry display table containing portfolio entries corresponding
                        // to fetched coin ids
                        string portfolioEntryDisplayTableString =
                            PortfolioManager.Instance.GetPortfolioEntryDisplayTableString(
                                coinIdsWithPortfolioEntry.ToArray());
                        ConsoleIOManager.Instance.PrintData(portfolioEntryDisplayTableString);
                    }
                    else // no PortfolioEntries to display
                    {
                        string noticeMessage = "No portfolio entries to display.";
                        ConsoleIOManager.Instance.LogNotice(noticeMessage);
                    }

                    // if data for coin ids which don't have corresponding porfolio entries was requested, 
                    // display an appropriate message to user
                    if (coinNamesWithoutPortfolioEntry.Count > 0)
                    {
                        string noticeMessage = StringUtils.Append(
                            "Following coin(s) were not in portfolio: ",
                            ", ",
                            coinNamesWithoutPortfolioEntry.ToArray())
                            + ".";
                        ConsoleIOManager.Instance.LogNotice(noticeMessage);
                    }

                    commandExecutedSuccessfuly = true;
                }
                catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    commandExecutedSuccessfuly = false;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    PortfolioCommandUtils.HandleDatabaseCommunicationException(databaseCommunicationException);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}

