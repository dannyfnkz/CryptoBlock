namespace CryptoBlock
{
    namespace PortfolioManagement.Transactions
    {
        public class BuyTransaction : Transaction
        {
            public BuyTransaction(long coinId, double amount, double pricePerCoin, long unixTimestamp)
                : base(eType.Buy, coinId, amount, pricePerCoin, unixTimestamp)
            {

            }
        }
    }
}
