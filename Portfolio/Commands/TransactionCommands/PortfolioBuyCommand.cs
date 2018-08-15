using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands.TransactionCommands
    {
        /// <summary>
        /// <para>
        /// buys specified amount of specified coin, for a specified price per coin.
        /// </para>
        /// <para>
        /// command syntax:
        /// portfolio buy [coin name / symbol] ([buy amount] [buy price per coin]),
        /// ([buy amount] [buy price per coin]), ...
        /// </para>
        /// </summary>
        internal class PortfolioBuyCommand : PortfolioTransactionCommand<BuyTransaction>
        {
            internal PortfolioBuyCommand()
            {

            }

            /// <summary>
            /// buys coin corresponding to name / symbol specified in <paramref name="commandArguments"/>[0],
            /// where buy amount is specified in <paramref name="commandArguments"/>[1]
            /// and buy price per coin is specified in <paramref name="commandArguments"/>[2].
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="ConsoleIOManager.ShowConfirmationDialog(string)"/>
            /// <seealso cref="PortfolioManager.BuyCoin(int, double, double, long)"/>
            /// <param name="commandArguments"></param>
            public override void ExecuteCommand(string[] commandArguments)
            {
                bool commandArgumentsValid = base.CheckCommandArgumentConstraints(commandArguments);

                if (!commandArgumentsValid)
                {
                    return;
                }

                try
                {
                    // price coin name or symbol from command argument 0
                    string coinNameOrSymbol = commandArguments[0];

                    // get coin id by name or symbol
                    long coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                    // get coin name
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                    // current timestamp
                    long unixTimestamp = DateTimeUtils.GetUnixTimestamp();

                    // parse buy transactions
                    BuyTransaction[] buyTransactions = tryParseTransactionArray(
                        commandArguments,
                        coinId,
                        unixTimestamp,
                        out bool buyOperationsParseSuccess);

                    if (!buyOperationsParseSuccess)
                    {
                        return;
                    }

                    // check if portfolio has an entry with specified id
                    if (!PortfolioManager.Instance.IsInPortfolio(coinId))
                    {
                        // portfolio has no entry with specified id
                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's no entry in portfolio manager for '{0}'.",
                            coinName);

                        // ask user if they want to create a new portfolio entry
                        string promptMessage = "Create new entry?";
                        bool createNewPortfolioEntry =
                            ConsoleIOManager.Instance.ShowConfirmationDialog(promptMessage);

                        if (createNewPortfolioEntry) // user chose to create a new portfolio entry
                        {
                            // create a new entry before proceeding to execute buy command 
                            PortfolioManager.Instance.AddCoin(coinId);

                            ConsoleIOManager.Instance.LogNoticeFormat(
                                false,
                                "'{0}' successfully added to portfolio.",
                                coinName);
                        }
                        else // user chose not to create a new portfolio entry
                        {
                            ConsoleIOManager.Instance.LogNotice("Purchase cancelled.");
                            return;
                        }
                    }

                    // execute buy command
                    PortfolioManager.Instance.BuyCoin(buyTransactions);

                    // purchase performed successfully
                    string successfulPurchaseNoticeMessage = buyTransactions.Length == 1
                        ? string.Format(
                            "Successfully purchased {0} {1} for {2}$ each.",
                            buyTransactions[0].Amount,
                            coinName,
                            buyTransactions[0].PricePerCoin)
                        : string.Format(
                            "{0} Specified purchases made successfully.",
                            buyTransactions.Length);

                    ConsoleIOManager.Instance.LogNotice(successfulPurchaseNoticeMessage);
                }
                catch (CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    // coin with specified name / symbol not found in listing repository
                    ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    base.HandleDatabaseCommunicationException(databaseCommunicationException);
                }
            }
        }
    }
}

