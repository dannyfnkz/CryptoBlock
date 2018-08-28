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
            private class InsufficientFundsForSellTransactionsException : Exception
            {
                private readonly long coinId;
                private readonly double coinHoldings;

                internal InsufficientFundsForSellTransactionsException(long coinId, double coinHoldings)
                {
                    this.coinId = coinId;
                    this.coinHoldings = coinHoldings;
                }

                internal long CoinId
                {
                    get { return coinId; }
                }

                internal double CoinHoldings
                {
                    get { return coinHoldings; }
                }
            }

            internal PortfolioSellCommand()
                : base()
            {

            }

            /// <summary>
            /// sells coin corresponding to name / symbol specified in <paramref name="commandArguments"/>[0],
            /// where sell amount is specified in <paramref name="commandArguments"/>[1]
            /// and sell price per coin is specified in <paramref name="commandArguments"/>[2].
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="PortfolioManager.GetCoinHoldings(int)"/>
            /// <seealso cref="PortfolioManager.SellCoin(int, double, double, long)"/>
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

                    if (sellTransactionArrayParseSuccess)
                    {

                        // check if there are enough funds for sell transactions
                        PortfolioEntry portfolioEntry = PortfolioManager.Instance.GetPortfolioEntry(coinId);
                        bool sufficientFundsForSellTransactions = sufficientFundsForTransactions(
                            portfolioEntry, sellTransactions);

                        if (!sufficientFundsForSellTransactions) // not enough funds to perform sales
                        {
                            throw new InsufficientFundsForSellTransactionsException(
                                coinId,
                                portfolioEntry.Holdings);
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

                        commandExecutedSuccessfuly = true;
                    }
                    else // !sellTransactionArrayParseSuccess
                    {
                        commandExecutedSuccessfuly = false;
                    }
                }
                catch (CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    // coin with specified name / symbol not found in listing repository
                    ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);

                    commandExecutedSuccessfuly = false;
                }
                catch(CoinNotInPortfolioException coinNotInPortfolioException)
                {
                    // portfolio has no entry with specified id
                    string coinName = CoinListingManager.Instance.GetCoinNameById(
                        coinNotInPortfolioException.CoinId);

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        "There's no entry in portfolio manager for '{0}'.",
                        coinName);

                    commandExecutedSuccessfuly = false;
                }
                catch(
                InsufficientFundsForSellTransactionsException insufficientFundsForSellTransactionsException)
                {
                    string coinName = CoinListingManager.Instance.GetCoinNameById(
                        insufficientFundsForSellTransactionsException.CoinId);
                    string coinSymbol = CoinListingManager.Instance.GetCoinSymbolById(
                        insufficientFundsForSellTransactionsException.CoinId);

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        "Not enough funds for requested sell operation(s). {0} holdings: {1} {2}.",
                        coinName,
                        insufficientFundsForSellTransactionsException.CoinHoldings,
                        coinSymbol);

                    commandExecutedSuccessfuly = false;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    PortfolioCommandUtils.HandleDatabaseCommunicationException(databaseCommunicationException);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
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

