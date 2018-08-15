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
        /// sells specified amount of specified coin, for a specified price per coin.
        /// </para>
        /// <para>
        /// command syntax: portfolio sell [coin name / symbol] [sell amount] [sell price per coin]
        /// </para>
        /// </summary>
        internal class PortfolioSellCommand : PortfolioTransactionCommand<SellTransaction>
        {
            internal PortfolioSellCommand()
                : base()
            {

            }

            /// <summary>
            /// sells coin corresponding to name / symbol specified in <paramref name="commandArguments"/>[0],
            /// where sell amount is specified in <paramref name="commandArguments"/>[1]
            /// and sell price per coin is specified in <paramref name="commandArguments"/>[2].
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="PortfolioManager.GetCoinHoldings(int)"/>
            /// <seealso cref="PortfolioManager.SellCoin(int, double, double, long)"/>
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

                    // get coin name & symbol
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);
                    string coinSymbol = CoinListingManager.Instance.GetCoinSymbolById(coinId);

                    // current timestamp
                    long unixTimestamp = DateTimeUtils.GetUnixTimestamp();


                    // parse buy transactions
                    SellTransaction[] sellTransactions = tryParseTransactionArray(
                        commandArguments,
                        coinId,
                        unixTimestamp,
                        out bool sellTransactionArrayParseSuccess);

                    if (!sellTransactionArrayParseSuccess)
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

                        return;
                    }

                    // check if there are enough funds for sell transactions
                    PortfolioEntry portfolioEntry = PortfolioManager.Instance.GetPortfolioEntry(coinId);
                    bool sufficientFundsForSellTransactions = sufficientFundsForTransactions(
                        portfolioEntry, sellTransactions);

                    if (!sufficientFundsForSellTransactions) // not enough funds to perform sales
                    {
                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "Not enough funds for requested sell operation(s). {0} holdings: {1} {2}.",
                            coinName,
                            portfolioEntry.Holdings,
                            coinSymbol);
                        return;
                    }

                    // execute sell transactions
                    PortfolioManager.Instance.SellCoin(sellTransactions);

                    // sale(s) performed successfully
                    string successfulSaleNoticeMessage = sellTransactions.Length == 1
                        ? string.Format(
                            "Successfully sold {0} {1} for {2}$ each.",
                            sellTransactions[0].Amount,
                            coinName,
                            sellTransactions[0].PricePerCoin)
                        : string.Format(
                            "{0} Specified sales made successfully.",
                            sellTransactions.Length);

                    ConsoleIOManager.Instance.LogNotice(successfulSaleNoticeMessage);
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

            private bool sufficientFundsForTransactions(
                PortfolioEntry portfolioEntry,
                SellTransaction[] sellTransactions)
            {
                double requiredFunds = 0;

                foreach (SellTransaction sellTransaction in sellTransactions)
                {
                    requiredFunds += sellTransaction.Amount;
                }

                return requiredFunds <= portfolioEntry.Holdings;
            }
        }
    }
}

