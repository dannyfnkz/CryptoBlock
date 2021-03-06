﻿using CryptoBlock.IOManagement;
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
        /// represents a <see cref="PortfolioTransactionCommand{T}"/> which performs a purchase of specified 
        /// amount of specified coin, for a specified price per coin.
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
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="ConsoleIOManager.ShowConfirmationDialog(string)"/>
            /// <seealso cref="PortfolioManager.BuyCoin(int, double, double, long)"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

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
                        out bool buyTransactionsParseSuccess);

                    bool createNewPortfolioEntry = false;

                    if (buyTransactionsParseSuccess)
                    {
                        bool executeBuyTransactions = false;

                        // check if portfolio has an entry with specified id
                        if (!PortfolioManager.Instance.IsInPortfolio(coinId))
                        {
                            // portfolio has no entry with specified id
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                ConsoleIOManager.eOutputReportType.CommandExecution,
                                "There's no entry in portfolio manager for '{0}'.",
                                coinName);

                            // ask user if they want to create a new portfolio entry
                            string promptMessage = "Create new entry?";
                            createNewPortfolioEntry =
                                ConsoleIOManager.Instance.ShowConfirmationDialog(
                                    promptMessage,
                                    ConsoleIOManager.eOutputReportType.CommandExecution);

                            if (createNewPortfolioEntry) // user chose to create a new portfolio entry
                            {
                                executeBuyTransactions = true;
                            }
                            else // user chose not to create a new portfolio entry
                            {
                                ConsoleIOManager.Instance.LogNotice(
                                    "Purchase cancelled.",
                                    ConsoleIOManager.eOutputReportType.CommandExecution);
                                executeBuyTransactions = false;
                            }
                        }
                        else // portfolio already has an entry with specified id
                        {
                            executeBuyTransactions = true;
                        }

                        if(executeBuyTransactions)
                        {
                            if(createNewPortfolioEntry) // perform coin (add + buy) action
                            {
                                PortfolioManager.Instance.AddAndBuyCoin(coinId, buyTransactions);

                                // log coin add notice
                                ConsoleIOManager.Instance.LogNoticeFormat(
                                    false,
                                    ConsoleIOManager.eOutputReportType.CommandExecution,
                                    "'{0}' successfully added to portfolio.",
                                    coinName);
                            }
                            else // perform coin buy action
                            {
                                PortfolioManager.Instance.BuyCoin(buyTransactions);
                            }

                            // purchase performed successfully
                            string successfulPurchaseNoticeMessage = buyTransactions.Length == 1
                                ? string.Format(
                                    "Successfully purchased {0} {1} for {2}$ each, stored in exchange " +
                                    "'{3}'.",
                                    buyTransactions[0].Amount,
                                    coinName,
                                    buyTransactions[0].PricePerCoin,
                                    buyTransactions[0].ExchangeName)
                                : string.Format(
                                    "{0} Specified purchases made successfully.",
                                    buyTransactions.Length);

                            ConsoleIOManager.Instance.LogNotice(
                                successfulPurchaseNoticeMessage,
                                ConsoleIOManager.eOutputReportType.CommandExecution);

                            commandExecutedSuccessfuly = true;
                        }
                        else // !executeBuyTransactions
                        {
                            commandExecutedSuccessfuly = false;
                        }
                    }
                    else // buy transaction parse not successful
                    {
                        commandExecutedSuccessfuly = false;
                    }
                }
                catch (CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    // coin with specified name / symbol not found in listing repository
                    ConsoleIOManager.Instance.LogError(
                        coinNameOrSymbolNotFoundException.Message,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = false;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    PortfolioCommand.HandleDatabaseCommunicationException(databaseCommunicationException);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}

