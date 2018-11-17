using CryptoBlock.CMCAPI;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            /// thrown if <see cref="ExchangeHolding"/> in exchange having <see cref="ExchangeName"/>
            /// is not sufficient for the requested operation.
            /// </summary>
            public class InsufficientFundsException : PortfolioEntryException
            {
                private readonly string exchangeName;
                private readonly double exchangeHolding;

                public InsufficientFundsException(long coinId, string exchangeName, double exchangeHolding)
                    : base(coinId, formatExceptionMessage(exchangeName, exchangeHolding))
                {
                    this.exchangeName = exchangeName;
                    this.exchangeHolding = exchangeHolding;
                }

                public string ExchangeName
                {
                    get { return ExchangeName; }
                }

                public double ExchangeHolding
                {
                    get { return exchangeHolding; }
                }

                private static string formatExceptionMessage(string exchangeName, double exchangeHolding)
                {
                    return string.Format(
                        "Not enough funds in specified exchange '{0}' for requested operation. " +
                        "Exchange holding: {1}.",
                        exchangeName,
                        exchangeHolding);
                }
            }

            private const string NOT_AVAILABLE_DATA_FIELD_STRING = "N/A";

            private readonly long id;
            private readonly long coinId;
            private Dictionary<string, ExchangeCoinHolding> exchangeNameToExchangeCoinHolding
                = new Dictionary<string, ExchangeCoinHolding>();
            private CoinTicker coinTicker;

            private double? averageCoinBuyPrice;
            private double coinHoldings;
            private double? profitPercentageUsd;

            internal PortfolioEntry(
                long id,
                long coinId,
                IList<ExchangeCoinHolding> exchangeCoinHoldings,
                CoinTicker coinTicker = null)
            {
                this.id = id;
                this.coinId = coinId;
                this.coinTicker = coinTicker;

                initializeExchangeNameToExchangeCoinHoldingDictionary(exchangeCoinHoldings);
                updateDynamicData();
            }

            /// <summary>
            /// <see cref="PortfolioEntry"/> ID, as set in database.
            /// </summary>
            public long Id
            {
                get { return id; }
            }

            /// <summary>
            /// coin id associated with portfolio entry.
            /// </summary>
            public long CoinId
            {
                get { return coinId; }
            }

            /// <summary>
            /// total amount of coin currently held in all exchanges.
            /// </summary>
            public double CoinHoldings
            {
                get { return coinHoldings; }
            }

            /// <summary>
            /// whether <see cref="HasNoCoinHoldings"/> is 0.
            /// </summary>
            public bool HasNoCoinHoldings
            {
                get { return coinHoldings == 0.0; }
            }

            /// <summary>
            /// <para>
            /// average coin buy price across all exchanges.
            /// </para>
            /// <para>
            /// null if <see cref="HasNoHoldings"/> = true.
            /// </para>
            /// </summary>
            public double? AverageCoinBuyPrice
            {
                get { return averageCoinBuyPrice; }
            }

            /// <summary>
            /// <para>
            /// percentage of profit (in relation to current price) from buying coin at
            /// <see cref="AverageBuyPrice"/>.
            /// </para>
            /// <para>
            /// null if <see cref="HasNoHoldings"/> = true or data regarding price (USD) is not available.
            /// </para>
            /// </summary>
            public double? ProfitPercentageUsd
            {
                get { return profitPercentageUsd; }
            }

            /// <summary>
            /// returns amount of coins of <see cref="CoinId"/> stored in exchange having
            /// specified <paramref name="exchangeName"/>.
            /// </summary>
            /// <param name="exchangeName"></param>
            /// <returns>
            /// amount of coins of <see cref="CoinId"/> stored in exchange having
            /// specified <paramref name="exchangeName"/>
            /// </returns>
            internal double GetCoinHoldings(string exchangeName)
            {
                double coinHoldings;

                if(this.exchangeNameToExchangeCoinHolding.ContainsKey(exchangeName)) 
                {
                    // specified exchange has coin holdings
                    ExchangeCoinHolding exchangeCoinHolding = 
                        this.exchangeNameToExchangeCoinHolding[exchangeName];
                    coinHoldings = exchangeCoinHolding.Amount;
                }
                else
                {
                    // specified exchange has no coin holdings 
                    coinHoldings = 0.0;
                }

                return coinHoldings;
            }

            /// <summary>
            /// handles specified <see cref="BuyTransaction"/>
            /// </summary>
            /// <seealso cref="addTransaction(Transaction)"/>
            /// <param name="buyTransaction"></param>
            internal void Buy(Transaction buyTransaction)
            {
                addTransaction(buyTransaction);
            }

            /// <summary>
            /// handles specified <paramref name="sellTransaction"/>.
            /// </summary>
            /// <seealso cref="addTransaction(Transaction)"/>
            /// <param name="sellTransaction"></param>
            internal void Sell(SellTransaction sellTransaction)
            {
                addTransaction(sellTransaction);
            }

            /// <summary>
            /// sets specified <paramref name="coinTicker"/> and updates relevant data fields
            /// accordingly.
            /// </summary>
            /// <seealso cref="updateProfitPercentageUsd"/>
            /// <param name="coinTicker"></param>
            internal void SetCoinTicker(CoinTicker coinTicker)
            {
                this.coinTicker = coinTicker;
                updateProfitPercentageUsd();
            }

            /// <summary>
            /// returns a detailed <see cref="String"/> representation of this
            /// <see cref="PortfolioEntry"/>.
            /// </summary>
            /// <seealso cref="getDataFieldstring{T}(T?)"/>
            /// <returns>
            /// detailed <see cref="String"/> representation of this
            /// <see cref="PortfolioEntry"/>
            /// </returns>
            internal String GetDetailedString()
            {
                StringBuilder detailedStringBuilder = new StringBuilder();

                // append data fields
                // append coin name
                string coinName = CoinListingManager.Instance.GetCoinNameById(this.CoinId);
                detailedStringBuilder.AppendFormatLine("Name: {0}", coinName);

                // append coin symbol
                string coinSymbol = CoinListingManager.Instance.GetCoinSymbolById(this.coinId);
                detailedStringBuilder.AppendFormatLine("Symbol: {0}", coinSymbol);

                // append coin price (USD)
                string coinPriceUsdString = getDataFieldstring(this.coinTicker.PriceUsd);
                detailedStringBuilder.AppendFormatLine("Price (USD): {0}", coinPriceUsdString);

                // append % of 24 hour coin price change
                string coinPricePercentChange24hUsdString =
                    getDataFieldstring(this.coinTicker.PricePercentChange24hUsd);
                detailedStringBuilder.AppendFormatLine(
                    "Price change % (24h): {0}",
                    coinPricePercentChange24hUsdString);

                // append coin holdings
                // append total coin holdings
                detailedStringBuilder.AppendFormatLine(
                    "Total Holdings: {0} {1}",
                    this.CoinHoldings,
                    coinSymbol);

                // append coin holding for each exchange
                string[] exchangeNameArray = this.exchangeNameToExchangeCoinHolding.Keys.ToArray();
                Array.Sort(exchangeNameArray);
                foreach(string exchangeName in exchangeNameArray)
                {
                    double exchangeCoinAmount = 
                        this.exchangeNameToExchangeCoinHolding[exchangeName].Amount;
                    detailedStringBuilder.AppendFormatTabbedLine(
                        "{0} Holdings: {1} {2}",
                        exchangeName,
                        exchangeCoinAmount,
                        coinSymbol);
                }

                // append coin % of profit (USD)
                string coinProfitPercentageUsd =
                    getDataFieldstring(this.ProfitPercentageUsd);
                detailedStringBuilder.AppendFormatLine(
                    "Profit % (USD): {0}%",
                    coinProfitPercentageUsd);

                return detailedStringBuilder.ToString();
            }

            /// <summary>
            /// returns a <see cref="String"/> representation of data field represented by
            /// specified <paramref name="nullable"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="nullable"></param>
            /// <returns>
            /// <see cref="String"/> representation of data field represented by
            /// specified <paramref name="nullable"/>.
            /// </returns>
            private string getDataFieldstring<T>(Nullable<T> nullable) where T : struct
            {
                return nullable.HasValue
                    ? nullable.Value.ToString()
                    : NOT_AVAILABLE_DATA_FIELD_STRING;
            }
                
            /// <summary>
            /// associates specified <paramref name="transaction"/> with this <see cref="PortfolioEntry"/>.
            /// </summary>
            /// <param name="transaction"></param>
            private void addTransaction(Transaction transaction)
            {
                PortfolioDatabaseManager.Instance.ExecuteAsOneAction(
                    () =>
                    {
                        // add to transaction history stored in database
                        PortfolioDatabaseManager.Instance.AddTransaction(transaction, this.Id);

                        // handel transaction 
                        ExchangeCoinHolding affectedExchangeCoinHolding = 
                            handleTransaction(transaction, out bool newExchangeCoinHoldingAdded);

                        if(newExchangeCoinHoldingAdded)
                        {
                            PortfolioDatabaseManager.Instance.AddExchangeCoinHolding(
                                affectedExchangeCoinHolding,
                                this.Id);
                        }
                        else
                        {
                            PortfolioDatabaseManager.Instance.UpdateExchangeCoinHolding(
                                affectedExchangeCoinHolding,
                                this.Id);
                        }
                    }
                );
            }

            /// <summary>
            /// handles specified <paramref name="transaction"/> and returns
            /// <see cref="ExchangeCoinHolding"/> associated with it.
            /// </summary>
            /// <param name="transaction"></param>
            /// <param name="newExchangeCoinHoldingAdded">
            /// whether a new <see cref="ExchangeCoinHolding"/> was created in the process of
            /// handling specified <paramref name="transaction"/>.
            /// </param>
            /// <returns>
            /// <see cref="ExchangeCoinHolding"/> associated with handled <paramref name="transaction"/>
            /// </returns>
            private ExchangeCoinHolding handleTransaction(
                Transaction transaction, 
                out bool newExchangeCoinHoldingAdded)
            {
                assertValidPrice(transaction.PricePerCoin);

                ExchangeCoinHolding affectedExchangeCoinHolding;
                    
                if (transaction.TransactionType == Transaction.eTransactionType.Buy) // buy transaction
                {
                    if(this.exchangeNameToExchangeCoinHolding.ContainsKey(transaction.ExchangeName))
                    {
                        affectedExchangeCoinHolding = 
                            this.exchangeNameToExchangeCoinHolding[transaction.ExchangeName];
                        affectedExchangeCoinHolding.HandleTransaction(transaction);

                        newExchangeCoinHoldingAdded = false;
                    }
                    else
                    {
                        affectedExchangeCoinHolding = new ExchangeCoinHolding(
                            transaction.CoinId,
                            transaction.ExchangeName,
                            transaction.Amount,
                            transaction.PricePerCoin);
                        this.exchangeNameToExchangeCoinHolding.Add(
                            affectedExchangeCoinHolding.ExchangeName,
                            affectedExchangeCoinHolding);

                        newExchangeCoinHoldingAdded = true;
                    }
                }
                else // sell transaction
                {
                    newExchangeCoinHoldingAdded = false;

                    if (this.exchangeNameToExchangeCoinHolding.ContainsKey(transaction.ExchangeName))
                    {
                        affectedExchangeCoinHolding =
                            this.exchangeNameToExchangeCoinHolding[transaction.ExchangeName];

                        if(affectedExchangeCoinHolding.Amount >= transaction.Amount)
                        {
                            affectedExchangeCoinHolding.HandleTransaction(transaction);
                        }
                        else
                        {
                            throw new InsufficientFundsException(
                                this.CoinId,
                                transaction.ExchangeName,
                                affectedExchangeCoinHolding.Amount);
                        }
                    }
                    else
                    {
                        throw new InsufficientFundsException(
                            this.CoinId,
                            transaction.ExchangeName,
                            0.0);
                    }
                }

                updateDynamicData();

                return affectedExchangeCoinHolding;
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
            /// initializes <see cref="exchangeNameToExchangeCoinHolding"/>.
            /// </summary>
            /// <param name="exchangeCoinHoldings"></param>
            private void initializeExchangeNameToExchangeCoinHoldingDictionary(
                IList<ExchangeCoinHolding> exchangeCoinHoldings)
            {
                foreach (ExchangeCoinHolding exchangeCoinHolding in exchangeCoinHoldings)
                {
                    this.exchangeNameToExchangeCoinHolding.Add(
                        exchangeCoinHolding.ExchangeName,
                        exchangeCoinHolding);
                }
            }

            /// <summary>
            /// updates dynamic data fields.
            /// </summary>
            /// <seealso cref="updateCoinHoldings"/>
            /// <seealso cref="updateAverageCoinBuyPrice"/>
            /// <seealso cref="updateProfitPercentageUsd"/>
            private void updateDynamicData()
            {
                updateCoinHoldings();
                updateAverageCoinBuyPrice();
                updateProfitPercentageUsd();
            }

            /// <summary>
            /// updates <see cref="CoinHoldings"/> based on <see cref="ExchangeCoinHolding.Amount"/>
            /// of each <see cref="ExchangeCoinHolding"/>.
            /// </summary>
            private void updateCoinHoldings()
            {
                this.coinHoldings = 0;

                foreach (ExchangeCoinHolding exchangeCoinHolding in
                    this.exchangeNameToExchangeCoinHolding.Values)
                {
                    this.coinHoldings += exchangeCoinHolding.Amount;
                }
            }

            /// <summary>
            /// updates <see cref="AverageCoinBuyPrice"/> based on 
            /// <see cref="ExchangeCoinHolding.AverageBuyPrice"/> of each <see cref="ExchangeCoinHolding"/>.
            /// </summary>
            private void updateAverageCoinBuyPrice()
            {
                if(!this.HasNoCoinHoldings)
                {
                    double sumOfAverageBuyPrices = 0;

                    foreach (ExchangeCoinHolding exchangeCoinHolding in
                        this.exchangeNameToExchangeCoinHolding.Values)
                    {
                        sumOfAverageBuyPrices += 
                            exchangeCoinHolding.Amount * exchangeCoinHolding.AverageBuyPrice;
                    }

                    this.averageCoinBuyPrice = sumOfAverageBuyPrices / this.CoinHoldings;
                }         
            }

            /// <summary>
            /// sets <see cref="profitPercentageUsd"/> according to current coin price (USD) and
            /// <see cref="AverageBuyPriceUsd"/>.
            /// </summary>
            private void updateProfitPercentageUsd()
            {
                // current USD price of coin is not available
                if (this.coinTicker == null || !this.coinTicker.PriceUsd.HasValue)
                {
                    this.profitPercentageUsd = null;
                }
                else // current USD price of coin is available
                {
                    if(!HasNoCoinHoldings) // has holdings
                    {
                        double diff = coinTicker.PriceUsd.Value - this.AverageCoinBuyPrice.Value;
                        this.profitPercentageUsd = (diff / this.AverageCoinBuyPrice.Value) * 100.0;
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

