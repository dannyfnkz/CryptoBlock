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

        private CoinListing[] staticCoinData;
        private bool repositoryInitialized;

        // name key is in lowercase
        private Dictionary<string, CoinListing> nameToStaticCoinData = 
            new Dictionary<string, CoinListing>();

        // symbol key is in lowercase
        private Dictionary<string, CoinListing> symbolToStaticCoinData =
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

        internal bool CoinNameExists(string coinName)
        {
            string lowercaseCoinMame = coinName.ToLower();
            return nameToStaticCoinData.ContainsKey(lowercaseCoinMame);
        }

        internal int GetCoinIdByName(string coinName)
        {
            string lowercaseCoinName = coinName.ToLower();

            if(CoinNameExists(lowercaseCoinName))
            {
                return nameToStaticCoinData[lowercaseCoinName].Id;
            }
            else
            {
                throw new NoSuchCoinNameException(coinName);
            }
        }

        internal bool CoinSymbolExists(string coinSymbol)
        {
            string lowercaseCoinSymbol = coinSymbol.ToLower();
            return symbolToStaticCoinData.ContainsKey(lowercaseCoinSymbol);
        }

        internal int GetCoinIdBySymbol(string coinSymbol)
        {
            string lowercaseCoinSymbol = coinSymbol.ToLower();

            if (CoinSymbolExists(lowercaseCoinSymbol))
            {
                return symbolToStaticCoinData[coinSymbol].Id;
            }
            else
            {
                throw new NoSuchCoinSymbolException(coinSymbol);
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
                    string lowercaseName = currentStaticCoinData.Name.ToLower();
                    string lowercaseSymbol = currentStaticCoinData.Symbol.ToLower();

                    nameToStaticCoinData[lowercaseName] = currentStaticCoinData;
                    symbolToStaticCoinData[lowercaseSymbol] = currentStaticCoinData;
                }
            }
            catch(RequestHandler.DataRequestException dataRequestException)
            {
                throw new RepositoryUpdateException(dataRequestException);
            }
        }
    }
}
