using Newtonsoft.Json;

namespace CryptoBlock
{
    namespace PortfolioManagement.Transactions
    {
        /// <summary>
        /// represents a coin transaction.
        /// </summary>
        public class Transaction
        {
            /// <summary>
            /// type of <see cref= "Transaction"/>.
            /// </summary>
            internal enum eTransactionType
            {
                Buy, Sell
            }

            [JsonProperty]
            private readonly eTransactionType transactionType;

            [JsonProperty]
            private readonly long coinId;

            [JsonProperty]
            private double amount;

            [JsonProperty]
            private readonly double pricePerCoin;

            [JsonProperty]
            private readonly long unixTimestamp;

            [JsonConstructor]
            internal Transaction(
                eTransactionType transactionType,
                long coinId,
                double amount,
                double pricePerCoin,
                long unixTimestamp)
            {
                this.transactionType = transactionType;
                this.coinId = coinId;
                this.amount = amount;
                this.pricePerCoin = pricePerCoin;
                this.unixTimestamp = unixTimestamp;
            }

            /// <summary>
            /// type of <see cref="Transaction"/>.
            /// </summary>
            [JsonIgnore]
            internal eTransactionType TransactionType
            {
                get { return transactionType; }
            }

            public long CoinId
            {
                get { return coinId; }
            }
            
            /// <summary>
            /// amount of coin.
            /// </summary>
            [JsonIgnore]
            public double Amount
            {
                get { return amount; }
            }

            /// <summary>
            /// price per coin.
            /// </summary>
            [JsonIgnore]
            public double PricePerCoin
            {
                get { return pricePerCoin; }
            }

            /// <summary>
            /// <see cref="Transaction"/> timestamp.
            /// </summary>
            [JsonIgnore]
            public long UnixTimestamp
            {
                get { return unixTimestamp; }
            }
        }
    }
}