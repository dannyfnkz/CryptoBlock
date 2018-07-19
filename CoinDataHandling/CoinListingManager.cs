using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class CoinListingManager
        {
            public class ManagerException : Exception
            {
                public ManagerException(string message, Exception innerException)
                    : base(message, innerException)
                {

                }

                internal ManagerException(string message)
                    : base(message)
                {

                }

                public ManagerException(Exception innerException)
                    : base(string.Empty, innerException)
                {

                }

                public ManagerException() : base()
                {

                }
            }

            public class RepositoryNotInitializedException : ManagerException
            {
                private string requestedPropertyName;

                public RepositoryNotInitializedException(string requestedPropertyName)
                    : base(formatExceptionMessage(requestedPropertyName))
                {
                    this.requestedPropertyName = requestedPropertyName;
                }

                public string RequestedPropertyName
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

            public class RepositoryUpdateException : ManagerException
            {
                public RepositoryUpdateException(Exception innerException)
                    : base("Could not update static data respository.", innerException)
                {

                }
            }

            public class NoSuchCoinNameException : ManagerException
            {
                public NoSuchCoinNameException(string coinName) : base(formatExceptionMessage(coinName))
                {

                }

                private static string formatExceptionMessage(string coinName)
                {
                    return string.Format("Coin name does not exist in database: {0}.", coinName);
                }
            }

            public class NoSuchCoinSymbolException : ManagerException
            {
                public NoSuchCoinSymbolException(string coinSymbol) : base(formatExceptionMessage(coinSymbol))
                {

                }

                private static string formatExceptionMessage(string coinSymbol)
                {
                    return string.Format("Coin symbol does not exist in database: {0}.", coinSymbol);
                }
            }

            public class NoSuchCoinNameOrSymbolException : ManagerException
            {
                public NoSuchCoinNameOrSymbolException(string coinNameOrSymbol) 
                    : base(formatExceptionMessage(coinNameOrSymbol))
                {

                }

                private static string formatExceptionMessage(string coinNameOrSymbol)
                {
                    return string.Format(
                        "Coin name or symbol '{0}' does not exist in coin listing repository.",
                        coinNameOrSymbol);
                }
            }

            public class NoSuchCoinIdException : ManagerException
            {
                public NoSuchCoinIdException(int coinId) : base(formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format("Coin id does not exist in database: {0}.", coinId);
                }
            }

            private static readonly CoinListingManager instance = new CoinListingManager();

            public static CoinListingManager Instance
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

            public bool RepositoryInitialized
            {
                get
                {
                    return repositoryInitialized;
                }
            }

            public int RepositoryCount
            {
                get
                {
                    assertRepositoryInitialized("Count");

                    return coinListingArray.Length;
                }
            }

            public void Initialize()
            {
                UpdateRepository();

                repositoryInitialized = true;
            }


            public bool CoinIdExists(int coinId)
            {
                assertRepositoryInitialized("CoinIdExists");

                return coinIdToCoinListing.ContainsKey(coinId);
            }

            public bool CoinNameExists(string coinName)
            {
                assertRepositoryInitialized("CoinNameExists");

                string lowercaseCoinMame = coinName.ToLower();

                return coinNameToCoinListing.ContainsKey(lowercaseCoinMame);
            }

            public int GetCoinIdByName(string coinName)
            {
                assertRepositoryInitialized("GetCoinIdByName");

                string lowercaseCoinName = coinName.ToLower();

                if (CoinNameExists(lowercaseCoinName))
                {
                    return coinNameToCoinListing[lowercaseCoinName].Id;
                }
                else
                {
                    throw new NoSuchCoinNameException(coinName);
                }
            }

            public bool CoinSymbolExists(string coinSymbol)
            {
                assertRepositoryInitialized("CoinSymbolExists");

                string lowercaseCoinSymbol = coinSymbol.ToLower();

                return coinSymbolToCoinListing.ContainsKey(lowercaseCoinSymbol);
            }

            public int GetCoinIdBySymbol(string coinSymbol)
            {
                assertRepositoryInitialized("GetCoinIdBySymbol");

                string lowercaseCoinSymbol = coinSymbol.ToLower();

                if (CoinSymbolExists(lowercaseCoinSymbol))
                {
                    return coinSymbolToCoinListing[lowercaseCoinSymbol].Id;
                }
                else
                {
                    throw new NoSuchCoinSymbolException(coinSymbol);
                }
            }

            public int GetCoinIdByNameOrSymbol(string coinNameOrSymbol)
            {
                assertRepositoryInitialized("GetCoinIdByNameOrSymbol");

                string lowercaseCoinNameOrSymbol = coinNameOrSymbol.ToLower();

                if(CoinNameExists(lowercaseCoinNameOrSymbol)) // get id by name
                {
                    return coinNameToCoinListing[lowercaseCoinNameOrSymbol].Id;
                }
                else if(CoinSymbolExists(lowercaseCoinNameOrSymbol)) // get id by symbol
                {
                    return coinSymbolToCoinListing[lowercaseCoinNameOrSymbol].Id;
                }
                else // neither name nor symbol exist in listing repository
                {
                    throw new NoSuchCoinNameOrSymbolException(coinNameOrSymbol);
                }
            }

            public string GetCoinNameById(int coinId)
            {
                return GetCoinListing(coinId).Name;
            }

            public string GetCoinSymbolById(int coinId)
            {
                return GetCoinListing(coinId).Symbol;
            }

            public CoinListing GetCoinListing(int coinId)
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

            public void UpdateRepository()
            {
                try
                {
                    coinListingArray = RequestHandler.RequestCoinListings();

                    // update name-to-StaticCoinData, symbol-to-StaticCoinData dictionaries
                    for (int i = 0; i < coinListingArray.Length; i++)
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
                catch (RequestHandler.DataRequestException dataRequestException)
                {
                    throw new RepositoryUpdateException(dataRequestException);
                }
            }

            private void assertRepositoryInitialized(string requestedPropertyOrOperation)
            {
                if (!repositoryInitialized)
                {
                    throw new RepositoryNotInitializedException(requestedPropertyOrOperation);
                }
            }
        }
    }
}

