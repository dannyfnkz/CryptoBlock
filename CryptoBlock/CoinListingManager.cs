using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;

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

        internal class RepositoryNotInitializedException : ManagerException
        {
            internal RepositoryNotInitializedException() :
                base(formatExceptionMessage())
            {

            }

            private static string formatExceptionMessage()
            {
                return string.Format("Coin Listing repository has not been initialized.");
            }
        }

        private CoinListing[] staticCoinData;
        private bool repositoryInitialized;

        private Dictionary<int, CoinListing> idToCoinListing =
            new Dictionary<int, CoinListing>();

        // name key is in lowercase
        private Dictionary<string, CoinListing> nameToCoinListing = 
            new Dictionary<string, CoinListing>();

        // symbol key is in lowercase
        private Dictionary<string, CoinListing> symbolToCoinListing =
            new Dictionary<string, CoinListing>();

        public static readonly CoinListingManager Instance = new CoinListingManager();

        internal bool RepositoryInitialized
        {
            get
            {
                return repositoryInitialized;
            }
        }

        internal void InitializeRepository()
        {
            UpdateRepository();

            repositoryInitialized = true;
        }


        internal bool CoinIdExists(int coinId)
        {
            assertRepositoryInitialized();

            return idToCoinListing.ContainsKey(coinId);
        }

        internal bool CoinNameExists(string coinName)
        {
            assertRepositoryInitialized();

            string lowercaseCoinMame = coinName.ToLower();

            return nameToCoinListing.ContainsKey(lowercaseCoinMame);
        }

        internal int GetCoinIdByName(string coinName)
        {
            assertRepositoryInitialized();

            string lowercaseCoinName = coinName.ToLower();

            if(CoinNameExists(lowercaseCoinName))
            {
                return nameToCoinListing[lowercaseCoinName].Id;
            }
            else
            {
                throw new NoSuchCoinNameException(coinName);
            }
        }

        internal bool CoinSymbolExists(string coinSymbol)
        {
            assertRepositoryInitialized();

            string lowercaseCoinSymbol = coinSymbol.ToLower();

            return symbolToCoinListing.ContainsKey(lowercaseCoinSymbol);
        }

        internal int GetCoinIdBySymbol(string coinSymbol)
        {
            assertRepositoryInitialized();

            string lowercaseCoinSymbol = coinSymbol.ToLower();

            if (CoinSymbolExists(lowercaseCoinSymbol))
            {
                return symbolToCoinListing[coinSymbol].Id;
            }
            else
            {
                throw new NoSuchCoinSymbolException(coinSymbol);
            }
        }

        internal CoinListing GetCoinListing(int coinId)
        {
            assertRepositoryInitialized();

            if(CoinIdExists(coinId))
            {
                return idToCoinListing[coinId];
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
                staticCoinData = RequestHandler.RequestCoinListings();

                // update name-to-StaticCoinData, symbol-to-StaticCoinData dictionaries
                for(int i = 0; i < staticCoinData.Length; i++)
                {
                    CoinListing currentStaticCoinData = staticCoinData[i];

                    int id = currentStaticCoinData.Id;
                    string lowercaseName = currentStaticCoinData.Name.ToLower();
                    string lowercaseSymbol = currentStaticCoinData.Symbol.ToLower();

                    idToCoinListing[id] = currentStaticCoinData;
                    nameToCoinListing[lowercaseName] = currentStaticCoinData;
                    symbolToCoinListing[lowercaseSymbol] = currentStaticCoinData;
                }
            }
            catch(RequestHandler.DataRequestException dataRequestException)
            {
                throw new RepositoryUpdateException(dataRequestException);
            }
        }

        private void assertRepositoryInitialized()
        {
            if(!repositoryInitialized)
            {
                throw new RepositoryNotInitializedException();
            }
        }
    }
}
