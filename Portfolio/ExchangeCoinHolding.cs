using CryptoBlock.PortfolioManagement.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class ExchangeCoinHolding
        {
            private readonly long coinId;
            private readonly string exchangeName;
            private double amount;
            private double averageBuyPrice;

            public ExchangeCoinHolding(
                long coinId,
                string exchangeName,
                double amount,
                double averageBuyPrice)
            {
                this.coinId = coinId;
                this.exchangeName = exchangeName;
                this.amount = amount;
                this.averageBuyPrice = averageBuyPrice;
            }

            public long CoinId
            {
                get { return coinId; }
            }

            public string ExchangeName
            {
                get { return exchangeName; }
            }

            public double Amount
            {
                get { return amount; }
            }

            public double AverageBuyPrice
            {
                get { return averageBuyPrice; }
            }

            internal void HandleTransaction(Transaction transaction)
            {
                if (transaction.TransactionType == Transaction.eTransactionType.Buy)
                {
                    this.averageBuyPrice =
                        ((this.AverageBuyPrice * this.Amount) +
                        (transaction.PricePerCoin * transaction.Amount))
                        / (this.Amount + transaction.Amount);
                    this.amount += transaction.Amount;
                }
                else
                {
                    this.amount -= transaction.Amount;
                }
            }        
        }
    }
}