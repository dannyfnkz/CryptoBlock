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

            private eType type;
            private double amount;
            private double price;
            private long unixTimestamp;

            internal Transaction(eType type, double amount, double price, long unixTimestamp)
            {
                this.type = type;
                this.amount = amount;
                this.price = price;
                this.unixTimestamp = unixTimestamp;
            }

            internal eType Type
            {
                get { return type; }
            }

            internal double Amount
            {
                get { return amount; }
            }

            internal double Price
            {
                get { return price; }
            }

            internal long UnixTimestamp
            {
                get { return unixTimestamp; }
            }
        }
    }
}