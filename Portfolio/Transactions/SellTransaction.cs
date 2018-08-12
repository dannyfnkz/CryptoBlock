namespace CryptoBlock
{
    namespace PortfolioManagement.Transactions
    {
        public class SellTransaction : Transaction
        {
            public SellTransaction(long coinId, double amount, double pricePerCoin, long unixTimestamp)
                : base(eType.Sell, coinId, amount, pricePerCoin, unixTimestamp)
            {

            }
        }
    }
}