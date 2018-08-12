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
            internal enum eType
            {
                Buy, Sell
            }

            [JsonProperty]
            private readonly eType type;

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
                eType type,
                long coinId,
                double amount,
                double pricePerCoin,
                long unixTimestamp)
            {
                this.type = type;
                this.coinId = coinId;
                this.amount = amount;
                this.pricePerCoin = pricePerCoin;
                this.unixTimestamp = unixTimestamp;
            }

            /// <summary>
            /// type of <see cref="Transaction"/>.
            /// </summary>
            [JsonIgnore]
            internal eType Type
            {
                get { return type; }
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