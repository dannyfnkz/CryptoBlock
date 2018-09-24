using CryptoBlock.CMCAPI;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.Utils;
using CryptoBlock.Utils.IO.SQLite.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// represents a portfolio entry for a single coin.
        /// </summary>
        public class PortfolioEntry
        {
            /// <summary>
            /// thrown if an exception occurs while performing an operation on <see cref="PortfolioEntry"/>.
            /// </summary>
            public class PortfolioEntryException : Exception
            {
                private readonly long coinId;

                public PortfolioEntryException(long coinId, string message)
                    : base(message)
                {
                    this.coinId = coinId;
                }

                public long CoinId
                {
                    get { return coinId; }
                }
            }

            /// <summary>
            /// thrown if price is specified for a buy / sell operation is not valid.
            /// </summary>
            public class InvalidPriceException : PortfolioEntryException
            {
                private readonly double price;

                public InvalidPriceException(long coinId, double price)
                    : base(coinId, formatExceptionMessage(price))
                {
                    this.price = price;
                }

                public double Price
                {
                    get { return price; }
                }

                private static string formatExceptionMessage(double price)
                {
                    return string.Format("Price must be larger than 0. Price was: {0}.", price);
                }
            }

            /// <summary>
            /// thrown if there was a mismatch between coin id of given <see cref="CoinTicker"/>
            /// and coin id of <see cref="PortfolioEntry"/>.
            /// </summary>
            public class PortfolioAndTickerCoinIdMismatchException : MismatchException
            {
                public PortfolioAndTickerCoinIdMismatchException()
                    : base("this.CoinId", "coinTicker.Id")
                {

                }
            }

            /// <summary>
            /// thrown if there were not enough funds to perform the specified operation.
            /// </summary>
            public class InsufficientFundsException : PortfolioEntryException
            {
                public InsufficientFundsException(long coinId, double holdings)
                    : base(coinId, formatExceptionMessage(holdings))
                {

                }

                private static string formatExceptionMessage(double funds)
                {
                    return string.Format("Not enough funds for requested operation. Hodlings: {0}.", funds);
                }
            }

            private readonly long id;

            [JsonProperty]
            private readonly long coinId;
            [JsonIgnore]
            private CoinTicker coinTicker;

            // list of transactions performed on this entry
            [JsonProperty]
            private List<Transaction> transactionHistory = new List<Transaction>();

            [JsonProperty]
            private double holdings;

            [JsonProperty]
            private double? averageBuyPrice;

            [JsonIgnore]
            private double? profitPercentageUsd;

            public PortfolioEntry(
                long id,
                long coinId,
                double holdings,
                double? averageBuyPrice,
                CoinTicker coinTicker = null)
            {
                this.id = id;
                this.coinId = coinId;
                this.holdings = holdings;
                this.averageBuyPrice = averageBuyPrice;
                this.coinTicker = coinTicker;

                setProfitPercentageUsd();
            }

            public long Id
            {
                get { return id; }
            }

            /// <summary>
            /// coin id associated with portfolio entry.
            /// </summary>
            [JsonIgnore]
            public long CoinId
            {
                get { return coinId; }
            }

            /// <summary>
            /// amount of coin currently held.
            /// </summary>
            [JsonIgnore]
            public double Holdings
            {
                get { return holdings; }
            }

            /// <summary>
            /// whether <see cref="Holdings"/> is 0.
            /// </summary>
            [JsonIgnore]
            public bool HasNoHoldings
            {
                get { return holdings == 0.0; }
            }

            /// <summary>
            /// <para>
            /// average coin buy price, taking into account all buys and sells in transaction history.
            /// </para>
            /// <para>
            /// null if <see cref="HasNoHoldings"/> = true.
            /// </para>
            /// </summary>
            [JsonIgnore]
            public double? AverageBuyPriceUsd
            {
                get { return averageBuyPrice; }
            }

            /// <summary>
            /// <para>
            /// percentage of profit (in relation to current price) from buying coin at <see cref="AverageBuyPrice"/>.
            /// </para>
            /// <para>
            /// null if <see cref="HasNoHoldings"/> = true or data regarding price (USD) is not available.
            /// </para>
            /// </summary>
            [JsonIgnore]
            public double? ProfitPercentageUsd
            {
                get { return profitPercentageUsd; }
            }

            /// <summary>
            /// performs a buy transaction.
            /// </summary>
            /// <seealso cref="addTransaction(Transaction)"/>
            /// <param name="buyAmount"></param>
            /// <param name="buyPricePerCoin"></param>
            /// <param name="unixTimestamp">unix timestamp when purchase was made</param>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="addTransaction(Transaction)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="addTransaction(Transaction)"/>
            /// </exception>
            public void Buy(Transaction buyTransaction)
            {
                addTransaction(buyTransaction);
            }

            /// <summary>
            /// performs a sale transaction.
            /// </summary>
            /// <seealso cref="addTransaction(Transaction)"/>
            /// <param name="sellTransaction"></param>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="addTransaction(Transaction)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            /// <seealso cref="addTransaction(Transaction)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="addTransaction(Transaction)"/>
            /// </exception>
            public void Sell(SellTransaction sellTransaction)
            {
                addTransaction(sellTransaction);
            }

            /// <summary>
            /// creates a new <see cref="Transaction"/> with specified
            /// <paramref name="transactionType"/>, <paramref name="amount"/>,
            /// <paramref name="pricePerCoin"/> and
            /// <paramref name="unixTimestamp"/>, updates <see cref="PortfolioEntry"/> accordingly
            /// and adds <see cref="Transaction"/> to its transaction history.
            /// </summary>
            /// <seealso cref="handleTransaction(Transaction)"/>
            /// <param name="transactionType"></param>
            /// <param name="holdings"></param>
            /// <param name="pricePerCoin"></param>
            /// <param name="unixTimestamp"></param>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="handleTransaction(Transaction)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            ///  <seealso cref="handleTransaction(Transaction)"/>
            /// </exception>
            /// <exception cref="SQLiteDatabaseHandlerException">
            /// <seealso cref="PortfolioDatabaseManager.UpdatePortfolioEntry(PortfolioEntry)"/>
            /// </exception>
            private void addTransaction(Transaction transaction)
            {
                PortfolioDatabaseManager.Instance.ExecuteAsOneAction(() =>
                {
                    // add to transaction history stored in database
                    PortfolioDatabaseManager.Instance.AddTransaction(transaction, this);

                    // handel transaction 
                    handleTransaction(transaction);

                    // update PortfolioEntry row in database
                    PortfolioDatabaseManager.Instance.UpdatePortfolioEntry(this);
                }
                );
            }

            /// <summary>
            /// adds <paramref name="transaction"/> to transaction history and updates
            /// <see cref="AverageBuyPriceUsd"/> and <see cref="Holdings"/> accordingly.
            /// </summary>
            /// <param name="transaction"></param>
            /// <exception cref="InvalidPriceException">
            /// <seealso cref="assertValidPrice(double)"/>
            /// </exception>
            /// <exception cref="InsufficientFundsException">
            /// thrown if there are not enough funds to perform sell <paramref name="transaction"/>.
            /// </exception>
            private void handleTransaction(Transaction transaction)
            {
                assertValidPrice(transaction.PricePerCoin);

                double newHoldings;
                double? newAverageBuyPrice;

                if (transaction.TransactionType == Transaction.eTransactionType.Buy) // buy transaction
                {
                    newHoldings = this.holdings + transaction.Amount;

                    if (HasNoHoldings) // no holdings, so average buy price is transaction price
                    {
                        newAverageBuyPrice = transaction.PricePerCoin;
                    }
                    else // has holdings, calculate new average buy price
                    {
                        newAverageBuyPrice = calcAverageBuyPrice(
                            this.holdings,
                            this.averageBuyPrice.Value,
                            transaction.Amount,
                            transaction.PricePerCoin);
                    }

                }
                else // sell transaction
                {
                    newHoldings = this.holdings - transaction.Amount;

                    if(newHoldings < 0) // less funds than requested sell amount
                    {
                        throw new InsufficientFundsException(this.coinId, this.holdings);
                    }

                    // if after selling holdings == 0, average buy price is undefined
                    // otherwise, it remains unchanged
                    newAverageBuyPrice = HasNoHoldings ? null : this.averageBuyPrice;
                }

                // update holdings and average buy price
                this.holdings = newHoldings;
                this.averageBuyPrice = newAverageBuyPrice;

                // update profit percentage (USD)
                setProfitPercentageUsd();
            }

            /// <summary>
            /// calculates a new average buy price, based on specified parameters.
            /// </summary>
            /// <param name="currentHolding"></param>
            /// <param name="currentAverageBuyPrice"></param>
            /// <param name="additionalBuyAmount"></param>
            /// <param name="additionalBuyAmountAverageBuyPrice"></param>
            /// <returns>
            /// new average buy price, based on specified parameters
            /// </returns>
            private static double calcAverageBuyPrice(
                double currentHolding,
                double currentAverageBuyPrice,
                double additionalBuyAmount,
                double additionalBuyAmountAverageBuyPrice)
            {
                double newHolding = currentHolding + additionalBuyAmount;

                double newAverageBuyPrice =
                    ((currentHolding * currentAverageBuyPrice)
                    + (additionalBuyAmount * additionalBuyAmountAverageBuyPrice))
                    / newHolding;

                return newAverageBuyPrice;
            }

            /// <summary>
            /// sets <see cref="profitPercentageUsd"/> according to current coin price (USD) and
            /// <see cref="AverageBuyPriceUsd"/>.
            /// </summary>
            private void setProfitPercentageUsd()
            {
                // current USD price of coin is not available
                if (this.coinTicker == null || !this.coinTicker.PriceUsd.HasValue)
                {
                    this.profitPercentageUsd = null;
                }
                else // current USD price of coin is available
                {
                    if(HasNoHoldings) // no holdings, so profit percentage is undefined
                    {
                        this.profitPercentageUsd = null;
                    }
                    else // has holdings
                    {
                        double diff = coinTicker.PriceUsd.Value - averageBuyPrice.Value;
                        this.profitPercentageUsd = (diff / averageBuyPrice) * 100.0;
                    }
                }
            }

            /// <summary>
            /// asserts that coin id associated with <paramref name="coinTicker"/> matches
            /// <see cref="CoinId"/>.
            /// </summary>
            /// <param name="coinTicker"></param>
            /// <exception cref="PortfolioAndTickerCoinIdMismatchException">
            /// thrown if coin id associated with <paramref name="coinTicker"/> does not
            /// match <see cref="CoinId"/>
            /// </exception>
            private void assertCoinTickerIdMatchesCoinId(CoinTicker coinTicker)
            {
                if (this.CoinId != coinTicker.Id)
                {
                    throw new PortfolioAndTickerCoinIdMismatchException();
                }
            }

            /// <summary>
            /// asserts that <paramref name="price"/> is valid.
            /// </summary>
            /// <param name="price"></param>
            /// <exception cref="InvalidPriceException">
            /// thrown if <paramref name="price"/> is not valid
            /// </exception>
            private void assertValidPrice(double price)
            {
                if(price <= 0.0)
                {
                    throw new InvalidPriceException(this.coinId, price);
                }
            }

            /// <summary>
            /// asserts that <paramref name="coinTicker"/> is not null.
            /// </summary>
            /// <param name="coinTicker"></param>
            /// <exception cref="ArgumentNullException">
            /// thrown if <paramref name="coinTicker"/> is null
            /// </exception>
            private void assertCoinTickerNotNull(CoinTicker coinTicker)
            {
                if(coinTicker == null)
                {
                    throw new ArgumentNullException("coinTicker");
                }
            }
        }
    }
}

