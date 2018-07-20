using CryptoBlock.CMCAPI;
using CryptoBlock.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class PortfolioEntry
        {
            public class PortfolioEntryException : Exception
            {
                private int coinId;

                public PortfolioEntryException(int coinId, string message)
                    : base(message)
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }
            }

            public class InvalidPriceException : PortfolioEntryException
            {
                private double price;

                public InvalidPriceException(int coinId, double price)
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
                    return string.Format("Price must be larget than 0. Price was: {0}.", price);
                }
            }

            public class PortfolioAndTickerCoinIdMismatchException : MismatchException
            {
                public PortfolioAndTickerCoinIdMismatchException()
                    : base("this.CoinId", "coinTicker.Id")
                {

                }
            }

            public class InsufficientFundsException : PortfolioEntryException
            {
                public InsufficientFundsException(int coinId, double holdings)
                    : base(coinId, formatExceptionMessage(holdings))
                {

                }

                private static string formatExceptionMessage(double funds)
                {
                    return string.Format("Not enough funds for requested operation. Hodlings: {0}.", funds);
                }
            }

            private const string NULL_VALUE_TABLE_DISPLAY_STRING = "N/A";

            [JsonProperty]
            private readonly int coinId;
            [JsonIgnore]
            private CoinTicker coinTicker;
            [JsonProperty]
            private List<Transaction> transactionHistory = new List<Transaction>();
            [JsonProperty]
            private double holdings;
            [JsonProperty]
            private double? averageBuyPrice;
            [JsonIgnore]
            private double? profitPercentageUsd;

            // assumes coinId is valid (exists in coin listing manager)
            [JsonConstructor]
            public PortfolioEntry(
                int coinId,
                CoinTicker coinTicker = null)
            {
                this.coinId = coinId;
               
                if (coinTicker != null)
                {
                    Update(coinTicker);
                }
            }

            [JsonIgnore]
            public int CoinId
            {
                get { return coinId; }
            }

            [JsonIgnore]
            public double Holdings
            {
                get { return holdings; }
            }

            [JsonIgnore]
            public bool HasNoHoldings
            {
                get { return holdings == 0.0; }
            }

            [JsonIgnore]
            public double? AverageBuyPrice
            {
                get { return averageBuyPrice; }
            }

            [JsonIgnore]
            public double? ProfitPercentageUsd
            {
                get { return profitPercentageUsd; }
            }

            public static PortfolioEntry DeserializeJSON(string JSONString)
            {
                PortfolioEntry portfolioEntry = JsonConvert.DeserializeObject<PortfolioEntry>(JSONString);

                return portfolioEntry;
            }

            public void Buy(double amount, double buyPrice, long unixTimestamp)
            {
                addTransaction(Transaction.eType.Buy, amount, buyPrice, unixTimestamp);
            }

            public void Sell(double amount, double sellPrice, long unixTimestamp)
            {
                addTransaction(Transaction.eType.Sell, amount, sellPrice, unixTimestamp);
            }

            // assumes coinTicker != null
            public void Update(CoinTicker coinTicker)
            {
                // assert that coinTicker and this portfolio entry have a matching coin id
                assertCoinTickerIdMatchesCoinId(coinTicker);

                this.coinTicker = coinTicker;

                updateProfitPercentageUsd();
            }

            private void addTransaction(
                Transaction.eType transactionType,
                double holdings,
                double price,
                long unixTimestamp)
            {
                Transaction transaction = new Transaction(transactionType, holdings, price, unixTimestamp);
                addTransaction(transaction);
            }

            private void assertCoinTickerIdMatchesCoinId(CoinTicker coinTicker)
            {
                if(this.coinId != coinTicker.Id)
                {
                    throw new PortfolioAndTickerCoinIdMismatchException();
                }
            }

            private void addTransaction(Transaction transaction)
            {
                assertValidPrice(transaction.Price);

                double newHoldings;
                double? newAverageBuyPrice;

                if (transaction.Type == Transaction.eType.Buy) // buy transaction
                {
                    newHoldings = this.holdings + transaction.Amount;
                    if (HasNoHoldings) // no holdings, so average buy price is current transaction price
                    {
                        newAverageBuyPrice = transaction.Price;
                    }
                    else // has holdings, calculate new average buy price
                    {
                        newAverageBuyPrice =
                            ((this.averageBuyPrice.Value * this.holdings) + (transaction.Amount * transaction.Price)) 
                            / newHoldings;
                    }

                }
                else // sell transaction
                {
                    newHoldings = this.holdings - transaction.Amount;

                    if(newHoldings < 0) // less funds than requested sell amount
                    {
                        throw new InsufficientFundsException(this.coinId, this.holdings);
                    }

                    newAverageBuyPrice = this.averageBuyPrice;
                }

                // update holdings and average buy price
                this.holdings = newHoldings;
                this.averageBuyPrice = newAverageBuyPrice;

                // update profit percentage (USD)
                updateProfitPercentageUsd();

                // add transaction to history
                this.transactionHistory.Add(transaction);
            }

            private void updateProfitPercentageUsd()
            {
                // current USD price of coin is not available
                if (coinTicker == null || !coinTicker.PriceUsd.HasValue)
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

            private void assertValidPrice(double price)
            {
                if(price <= 0.0)
                {
                    throw new InvalidPriceException(this.coinId, price);
                }
            }
        }
    }
}

