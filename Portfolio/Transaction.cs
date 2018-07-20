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
        internal class Transaction
        {
            internal enum eType
            {
                Buy, Sell
            }

            [JsonProperty]
            private eType type;
            [JsonProperty]
            private double amount;
            [JsonProperty]
            private double price;
            [JsonProperty]
            private long unixTimestamp;

            [JsonConstructor]
            internal Transaction(eType type, double amount, double price, long unixTimestamp)
            {
                this.type = type;
                this.amount = amount;
                this.price = price;
                this.unixTimestamp = unixTimestamp;
            }

            [JsonIgnore]
            internal eType Type
            {
                get { return type; }
            }

            [JsonIgnore]
            internal double Amount
            {
                get { return amount; }
            }

            [JsonIgnore]
            internal double Price
            {
                get { return price; }
            }

            [JsonIgnore]
            internal long UnixTimestamp
            {
                get { return unixTimestamp; }
            }
        }
    }
}