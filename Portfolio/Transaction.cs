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
        /// <summary>
        /// represents a coin transaction.
        /// </summary>
        internal class Transaction
        {
            /// <summary>
            /// type of <see cref= "Transaction"/>.
            /// </summary>
            internal enum eType
            {
                Buy, Sell
            }

            [JsonProperty]
            private eType type;
            [JsonProperty]
            private double amount;
            [JsonProperty]
            private double pricePerCoin;
            [JsonProperty]
            private long unixTimestamp;

            [JsonConstructor]
            internal Transaction(eType type, double amount, double pricePerCoin, long unixTimestamp)
            {
                this.type = type;
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
            
            /// <summary>
            /// amount of coin.
            /// </summary>
            [JsonIgnore]
            internal double Amount
            {
                get { return amount; }
            }

            /// <summary>
            /// price per coin.
            /// </summary>
            [JsonIgnore]
            internal double PricePerCoin
            {
                get { return pricePerCoin; }
            }

            /// <summary>
            /// <see cref="Transaction"/> timestamp.
            /// </summary>
            [JsonIgnore]
            internal long UnixTimestamp
            {
                get { return unixTimestamp; }
            }
        }
    }
}