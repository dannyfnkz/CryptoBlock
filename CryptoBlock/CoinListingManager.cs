using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;

namespace CryptoBlock
{
    internal class CoinListingManager
    {
        internal class ManagerException : Exception
        {
            internal ManagerException(string message, Exception innerException)
                : base(message, innerException)
            {

            }

            internal ManagerException(string message)
                : base(message)
            {

            }

            internal ManagerException(Exception innerException)
                : base(string.Empty, innerException)
            {

            }

            internal ManagerException() : base()
            {

            }
        }

        internal class RepositoryNotInitializedException : ManagerException
        {
            private string requestedPropertyName;

            internal RepositoryNotInitializedException(string requestedPropertyName)
                : base(formatExceptionMessage(requestedPropertyName))
            {
                this.requestedPropertyName = requestedPropertyName;
            }

            internal string RequestedPropertyName
            {
                get { return requestedPropertyName; }
            }

            private static string formatExceptionMessage(string requestedPropertyName)
            {
                return string.Format(
                    "Coin listing repository must be initialized before the following property / operation is" +
                    " requested: {0}.",
                    requestedPropertyName);
            }
        }

        internal class RepositoryUpdateException : ManagerException
        {
            internal RepositoryUpdateException(Exception innerException)
                : base("Could not update static data respository.", innerException)
            {

            }
        }

        internal class NoSuchCoinNameException : ManagerException
        {
            internal NoSuchCoinNameException(string coinName) : base(formatExceptionMessage(coinName))
            {

            }

            private static string formatExceptionMessage(string coinName)
            {
                return string.Format("Coin name does not exist in database: {0}.", coinName);
            }
        }

        internal class NoSuchCoinSymbolException : ManagerException
        {
            internal NoSuchCoinSymbolException(string coinSymbol) : base(formatExceptionMessage(coinSymbol))
            {

            }

            private static string formatExceptionMessage(string coinSymbol)
            {
                return string.Format("Coin symbol does not exist in database: {0}.", coinSymbol);
            }
        }

        internal class NoSuchCoinIdException : ManagerException
        {
            internal NoSuchCoinIdException(int coinId) : base(formatExceptionMessage(coinId))
            {

            }

            private static string formatExceptionMessage(int coinId)
            {
                return string.Format("Coin id does not exist in database: {0}.", coinId);
            }
        }

        private static readonly CoinListingManager instance = new CoinListingManager();

        internal static CoinListingManager Instance
        {
            get { return instance; }
        }

        private CoinListing[] coinListingArray;
        private bool repositoryInitialized;

        private Dictionary<int, CoinListing> coinIdToCoinListing =
            new Dictionary<int, CoinListing>();

        // name key is in lowercase
        private Dictionary<string, CoinListing> coinNameToCoinListing = 
            new Dictionary<string, CoinListing>();

        // symbol key is in lowercase
        private Dictionary<string, CoinListing> coinSymbolToCoinListing =
            new Dictionary<string, CoinListing>();

        internal bool RepositoryInitialized
        {
            get
            {
                return repositoryInitialized;
            }
        }

        internal int RepositoryCount
        {
            get
            {
                assertRepositoryInitialized("Count");

                return coinListingArray.Length;
            }
        }

        internal void Initialize()
        {
            UpdateRepository();

            repositoryInitialized = true;
        }


        internal bool CoinIdExists(int coinId)
        {
            assertRepositoryInitialized("CoinIdExists");

            return coinIdToCoinListing.ContainsKey(coinId);
        }

        internal bool CoinNameExists(string coinName)
        {
            assertRepositoryInitialized("CoinNameExists");

            string lowercaseCoinMame = coinName.ToLower();

            return coinNameToCoinListing.ContainsKey(lowercaseCoinMame);
        }

        internal int GetCoinIdByName(string coinName)
        {
            assertRepositoryInitialized("GetCoinIdByName");

            string lowercaseCoinName = coinName.ToLower();

            if(CoinNameExists(lowercaseCoinName))
            {
                return coinNameToCoinListing[lowercaseCoinName].Id;
            }
            else
            {
                throw new NoSuchCoinNameException(coinName);
            }
        }

        internal bool CoinSymbolExists(string coinSymbol)
        {
            assertRepositoryInitialized("CoinSymbolExists");

            string lowercaseCoinSymbol = coinSymbol.ToLower();

            return coinSymbolToCoinListing.ContainsKey(lowercaseCoinSymbol);
        }

        internal int GetCoinIdBySymbol(string coinSymbol)
        {
            assertRepositoryInitialized("GetCoinIdBySymbol");

            string lowercaseCoinSymbol = coinSymbol.ToLower();

            if (CoinSymbolExists(lowercaseCoinSymbol))
            {
                return coinSymbolToCoinListing[coinSymbol].Id;
            }
            else
            {
                throw new NoSuchCoinSymbolException(coinSymbol);
            }
        }

        internal CoinListing GetCoinListing(int coinId)
        {
            assertRepositoryInitialized("GetCoinListing");

            if (CoinIdExists(coinId))
            {
                return coinIdToCoinListing[coinId];
            }
            else
            {
                throw new NoSuchCoinIdException(coinId);
            }
        }

        internal void UpdateRepository()
        {
            try
            {
                coinListingArray = RequestHandler.RequestCoinListings();

                // update name-to-StaticCoinData, symbol-to-StaticCoinData dictionaries
                for(int i = 0; i < coinListingArray.Length; i++)
                {
                    CoinListing currentStaticCoinData = coinListingArray[i];

                    int id = currentStaticCoinData.Id;
                    string lowercaseName = currentStaticCoinData.Name.ToLower();
                    string lowercaseSymbol = currentStaticCoinData.Symbol.ToLower();

                    coinIdToCoinListing[id] = currentStaticCoinData;
                    coinNameToCoinListing[lowercaseName] = currentStaticCoinData;
                    coinSymbolToCoinListing[lowercaseSymbol] = currentStaticCoinData;
                }
            }
            catch(RequestHandler.DataRequestException dataRequestException)
            {
                throw new RepositoryUpdateException(dataRequestException);
            }
        }

        private void assertRepositoryInitialized(string requestedPropertyOrOperation)
        {
            if(!repositoryInitialized)
            {
                throw new RepositoryNotInitializedException(requestedPropertyOrOperation);
            }
        }
    }
}
