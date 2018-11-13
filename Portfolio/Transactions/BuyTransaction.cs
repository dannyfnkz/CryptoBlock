namespace CryptoBlock
{
    namespace PortfolioManagement.Transactions
    {
        /// <summary>
        /// represents a <see cref="Transaction"/> which performs a purchase
        /// of specified coin.
        /// </summary>
        public class BuyTransaction : Transaction
        {
            public BuyTransaction(
                long coinId, 
                double amount,
                double pricePerCoin,
                string exchangeName,
                long unixTimestamp)
                : base(eTransactionType.Buy, coinId, amount, pricePerCoin, exchangeName, unixTimestamp)
            {

            }
        }
    }
}
