namespace CryptoBlock
{
    namespace PortfolioManagement.Transactions
    {
        /// <summary>
        /// <summary>
        /// represents a <see cref="Transaction"/> which performs a sale
        /// of specified coin.
        /// </summary>
        public class SellTransaction : Transaction
        {
            public SellTransaction(
                long coinId, 
                double amount, 
                double pricePerCoin,
                string exchangeName,
                long unixTimestamp)
                : base(eTransactionType.Sell, coinId, amount, pricePerCoin, exchangeName, unixTimestamp)
            {

            }
        }
    }
}