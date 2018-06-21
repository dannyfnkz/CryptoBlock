using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;

namespace CryptoBlock
{
    internal class StaticCoinDataManager
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

        private StaticCoinData[] staticCoinData;
        private bool repositoryInitialized;
        private Dictionary<string, StaticCoinData> nameToStaticCoinData = 
            new Dictionary<string, StaticCoinData>();
        private Dictionary<string, StaticCoinData> symbolToStaticCoinData =
            new Dictionary<string, StaticCoinData>();

        public static readonly StaticCoinDataManager Instance = new StaticCoinDataManager();

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
            return nameToStaticCoinData.ContainsKey(coinName);
        }

        internal int GetCoinIdByName(string coinName)
        {
            if(CoinNameExists(coinName))
            {
                return nameToStaticCoinData[coinName].Id;
            }
            else
            {
                throw new NoSuchCoinNameException(coinName);
            }
        }

        internal bool CoinSymbolExists(string coinSymbol)
        {
            return symbolToStaticCoinData.ContainsKey(coinSymbol);
        }

        internal int GetCoinIdBySymbol(string coinSymbol)
        {
            if (CoinSymbolExists(coinSymbol))
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
                staticCoinData = RequestHandler.RequestStaticCoinData();

                // update name-to-StaticCoinData, symbol-to-StaticCoinData dictionaries
                for(int i = 0; i < staticCoinData.Length; i++)
                {
                    StaticCoinData currentStaticCoinData = staticCoinData[i];
                    nameToStaticCoinData[currentStaticCoinData.Name] = currentStaticCoinData;
                    symbolToStaticCoinData[currentStaticCoinData.Symbol] = currentStaticCoinData;
                }
            }
            catch(RequestHandler.DataRequestException dataRequestException)
            {
                throw new RepositoryUpdateException(dataRequestException);
            }
        }
    }
}
