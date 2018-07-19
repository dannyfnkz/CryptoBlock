using CryptoBlock.CMCAPI;
using CryptoBlock.ServerDataManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class PortfolioManager
        {
            public class PortfolioManagerException : Exception
            {
                public PortfolioManagerException(string exceptionMessage)
                    : base(exceptionMessage)
                {

                }
            }

            public class CoinIdAlreadyInPortfolioException : PortfolioManagerException
            {
                private readonly int coinId;

                public CoinIdAlreadyInPortfolioException(int coinId)
                    : base(formatExceptionMessage(coinId))
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format(
                        "Coin ID '{0}' already exists in portfolio.",
                        coinId);
                }
            }

            public class CoinIdNotInPortfolioException : PortfolioManagerException
            {
                private readonly int coinId;

                public CoinIdNotInPortfolioException(int coinId)
                    : base(formatExceptionMessage(coinId))
                {
                    this.coinId = coinId;
                }

                public int CoinId
                {
                    get { return coinId; }
                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format(
                        "Coin ID '{0}' does not exist in portfolio manager.",
                        coinId);
                }
            }

            private const double MAX_NUMERICAL_VALUE_ALLOWED = 1.0E15;

            private static readonly PortfolioManager instance = new PortfolioManager();

            private readonly Dictionary<int, PortfolioEntry> coinIdToPortfolioEntry 
                = new Dictionary<int, PortfolioEntry>();

            public PortfolioManager()
            {
                // subscribe to coin ticker manager repository update events
                CoinTickerManager.Instance.RepositoryUpdatedEvent += coinTickerManager_RepositoryUpdated;
            }

            public static double MaxNumericalValueAllowed
            {
                get { return MAX_NUMERICAL_VALUE_ALLOWED; }
            }

            public static PortfolioManager Instance
            {
                get { return instance; }
            }

            //public PortfolioEntry GetPortfolioEntry(int coinId)
            //{
            //    assertCoinIdInPortfolio(coinId);

            //    return coinIdToPortfolioEntry[coinId];    
            //}

            public bool IsInPortfolio(int coinId)
            {
                return coinIdToPortfolioEntry.Keys.Contains(coinId);
            }

            // assumes coinId is valid
            public void CreatePortfolioEntry(int coinId)
            {
                assertCoinIdNotAlreadyInPortfolio(coinId);

                // get CoinTicker corresponding to coinId (null if not available in ticker repository)
                CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);

                // create a new portfolio entry and update dictionary
                PortfolioEntry portfolioEntry = new PortfolioEntry(coinId, coinTicker);
                coinIdToPortfolioEntry[coinId] = portfolioEntry;
            }

            public void RemovePortfolioEntry(int coinId)
            {
                assertCoinIdInPortfolio(coinId);

                coinIdToPortfolioEntry.Remove(coinId);
            }

            public void BuyCoin(int coinId, double buyAmount, double buyPrice, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                portfolioEntry.Buy(buyAmount, buyPrice, unixTimestamp);
            }

            // throws PortfolioEntry.InsufficientFundsException
            public void SellCoin(int coinId, double sellAmount, double sellPrice, long unixTimestamp)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                portfolioEntry.Sell(sellAmount, sellPrice, unixTimestamp);
            }

            public double GetCoinHoldings(int coinId)
            {
                PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                return portfolioEntry.Holdings;
            }

            public string GetPortfolioEntryDisplayTableString(params int[] coinIds)
            {
                // init portfolio entry table
                PortfolioEntryTable portfolioEntryTable = new PortfolioEntryTable();

                foreach(int coinId in coinIds)
                {
                    // add row corresponding to each portfolio entry associated with specified id
                    PortfolioEntry portfolioEntry = getPortfolioEntry(coinId);
                    portfolioEntryTable.AddPortfolioEntryRow(portfolioEntry);
                }

                // return table display string
                string portfolioEntryTableString = portfolioEntryTable.GetTableDisplayString();

                return portfolioEntryTableString;
            }

            private PortfolioEntry getPortfolioEntry(int coinId)
            {
                assertCoinIdInPortfolio(coinId);

                PortfolioEntry portfolioEntry = coinIdToPortfolioEntry[coinId];
                return portfolioEntry;
            }

            private void coinTickerManager_RepositoryUpdated(Range updatedCoinIdRange)
            {
                // for each portfolio entry,
                // update entry if its coin id is included in the update range
                foreach (int coinId in coinIdToPortfolioEntry.Keys)
                {
                    if (updatedCoinIdRange.IsWithinRange(coinId))
                    {
                        CoinTicker coinTicker = CoinTickerManager.Instance.GetCoinTicker(coinId);
                        coinIdToPortfolioEntry[coinId].Update(coinTicker);
                    }
                }
            }

            private void assertCoinIdInPortfolio(int coinId)
            {
                if(!IsInPortfolio(coinId))
                {
                    throw new CoinIdNotInPortfolioException(coinId);
                }
            }

            private void assertCoinIdNotAlreadyInPortfolio(int coinId)
            {
                if(IsInPortfolio(coinId))
                {
                    throw new CoinIdAlreadyInPortfolioException(coinId);
                }
            }
        }
    }
}

