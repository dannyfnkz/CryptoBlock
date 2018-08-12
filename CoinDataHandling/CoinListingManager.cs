using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;
using static CryptoBlock.CMCAPI.RequestHandler;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// manages application's <see cref="CoinListing"/> repository.
        /// </summary>
        public class CoinListingManager
        {
            /// <summary>
            /// thrown if an exception occurs while performing a <see cref="CoinListingManager"/> operation.
            /// </summary>
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

            /// <summary>
            /// thrown if <see cref="CoinListingManager"/> is attempted to be initialized after already being
            /// initialized before.
            /// </summary>
            public class ManagerAlreadyInitializedException : ManagerException
            {
                public ManagerAlreadyInitializedException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Coin listing manager has already been initialized.";
                }
            }

            /// <summary>
            /// thrown if an operation on <see cref="CoinListingManager"/> is attempted to be performed before
            /// manager has been initialized.
            /// </summary>
            public class ManagerNotInitializedException : ManagerException
            {
                private string requestedPropertyName;

                public ManagerNotInitializedException(string requestedPropertyName)
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
                        "Coin listing manager must be initialized before the following property / operation is" +
                        " requested / performed: {0}.",
                        requestedPropertyName);
                }
            }

            /// <summary>
            /// thrown in an exception occurs while coin listing repository is being updated.
            /// </summary>
            public class RepositoryUpdateException : ManagerException
            {
                public RepositoryUpdateException(Exception innerException)
                    : base("Could not update coin listing respository.", innerException)
                {

                }
            }

            /// <summary>
            /// thrown if a <see cref="CoinListing"/> with specified coin name was not found
            /// in coin listing repository.
            /// </summary>
            public class CoinNameNotFoundException : ManagerException
            {
                public CoinNameNotFoundException(string coinName) : base(formatExceptionMessage(coinName))
                {

                }

                private static string formatExceptionMessage(string coinName)
                {
                    return string.Format("Coin name does not exist in database: {0}.", coinName);
                }
            }

            /// <summary>
            /// thrown if a <see cref="CoinListing"/> with specified coin symbol was not found
            /// in coin listing repository.
            /// </summary>
            public class CoinSymbolNotFoundException : ManagerException
            {
                public CoinSymbolNotFoundException(string coinSymbol) : base(formatExceptionMessage(coinSymbol))
                {

                }

                private static string formatExceptionMessage(string coinSymbol)
                {
                    return string.Format("Coin symbol does not exist in database: {0}.", coinSymbol);
                }
            }

            /// <summary>
            /// thrown if a <see cref="CoinListing"/> with specified coin name or symbol was not found in coin
            /// listing repository.
            /// </summary>
            public class CoinNameOrSymbolNotFoundException : ManagerException
            {
                public CoinNameOrSymbolNotFoundException(string coinNameOrSymbol) 
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

            /// <summary>
            /// thrown if a <see cref="CoinListing"/> with specified coin ID was not found in
            /// coin listing repository.
            /// </summary>
            public class CoinIdNotFoundException : ManagerException
            {
                public CoinIdNotFoundException(long coinId) : base(formatExceptionMessage(coinId))
                {

                }

                private static string formatExceptionMessage(long coinId)
                {
                    return string.Format("Coin id does not exist in database: {0}.", coinId);
                }
            }

            private static readonly CoinListingManager instance = new CoinListingManager();

            public static CoinListingManager Instance
            {
                get { return instance; }
            }

            // listing repository was initialized
            private bool managerInitialized;

            private Dictionary<long, CoinListing> coinIdToCoinListing =
                new Dictionary<long, CoinListing>();

            // name key is in lowercase
            private Dictionary<string, CoinListing> coinNameToCoinListing =
                new Dictionary<string, CoinListing>();

            // symbol key is in lowercase
            private Dictionary<string, CoinListing> coinSymbolToCoinListing =
                new Dictionary<string, CoinListing>();

            /// <summary>
            /// whether <see cref="CoinListing"/> respository was initialized.
            /// </summary>
            public bool ManagerInitialized
            {
                get
                {
                    return managerInitialized;
                }
            }
            
            /// <summary>
            ///  count of <see cref="CoinListing"/>s in repository.
            /// </summary>
            public int RepositoryCount
            {
                get
                {
                    assertManagerInitialized("Count");

                    return coinIdToCoinListing.Count;
                }
            }

            /// <summary>
            /// initializes <see cref="CoinListingManager"/>.
            /// </summary>
            /// <seealso cref="UpdateRepository"/>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// <seealso cref="assertManagerNotYetInitialized"/>
            /// </exception>
            public void Initialize()
            {
                assertManagerNotYetInitialized();

                UpdateRepository();
                managerInitialized = true;
            }

            /// <summary>
            /// returns whether a <see cref="CoinListing"/> having specified <paramref name="coinId"/>
            /// exists in repository.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// true if a <see cref="CoinListing"/> having specified <paramref name="coinId"/>
            /// exists in repository, else false
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool CoinIdExists(long coinId)
            {
                assertManagerInitialized("CoinIdExists");

                return coinIdToCoinListing.ContainsKey(coinId);
            }

            /// <summary>
            /// returns whether a <see cref="CoinListing"/> having specified <paramref name="coinName"/>
            /// exists in repository.
            /// </summary>
            /// <param name="coinName"></param>
            /// <returns>
            /// true if a <see cref="CoinListing"/> having specified <paramref name="coinName"/>
            /// exists in repository, else false
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool CoinNameExists(string coinName)
            {
                assertManagerInitialized("CoinNameExists");

                string lowercaseCoinMame = coinName.ToLower();

                return coinNameToCoinListing.ContainsKey(lowercaseCoinMame);
            }

            /// <summary>
            /// returns whether a <see cref="CoinListing"/> having specified <paramref name="coinSymbol"/>
            /// exists in repository.
            /// </summary>
            /// <param name="coinSymbol"></param>
            /// <returns>
            /// true if a <see cref="CoinListing"/> having specified <paramref name="coinSymbol"/>
            /// exists in repository, else false
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool CoinSymbolExists(string coinSymbol)
            {
                assertManagerInitialized("CoinSymbolExists");

                string lowercaseCoinSymbol = coinSymbol.ToLower();

                return coinSymbolToCoinListing.ContainsKey(lowercaseCoinSymbol);
            }

            /// <summary>
            /// returns coin id corresponding to <paramref name="coinName"/>.
            /// </summary>
            /// <param name="coinName"></param>
            /// <returns>
            /// coin id corresponding to <paramref name="coinName"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNameNotFoundException">
            /// thrown if a <see cref="CoinListing"/> having <paramref name="coinName"/> does not exist in
            /// repository
            /// </exception>
            public int GetCoinIdByName(string coinName)
            {
                assertManagerInitialized("GetCoinIdByName");

                string lowercaseCoinName = coinName.ToLower();

                if (CoinNameExists(lowercaseCoinName))
                {
                    return coinNameToCoinListing[lowercaseCoinName].Id;
                }
                else
                {
                    throw new CoinNameNotFoundException(coinName);
                }
            }

            /// <summary>
            /// returns coin id corresponding to <paramref name="coinSymbol"/>.
            /// </summary>
            /// <param name="coinSymbol"></param>
            /// <returns>
            /// coin id corresponding to <paramref name="coinSymbol"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinSymbolNotFoundException">
            /// thrown if a <see cref="CoinListing"/> having <paramref name="coinSymbol"/> does not exist in
            /// repository
            /// </exception>
            public int GetCoinIdBySymbol(string coinSymbol)
            {
                assertManagerInitialized("GetCoinIdBySymbol");

                string lowercaseCoinSymbol = coinSymbol.ToLower();

                if (CoinSymbolExists(lowercaseCoinSymbol))
                {
                    return coinSymbolToCoinListing[lowercaseCoinSymbol].Id;
                }
                else
                {
                    throw new CoinSymbolNotFoundException(coinSymbol);
                }
            }

            /// <summary>
            /// returns coin id corresponding to <paramref name="coinNameOrSymbol"/>.
            /// </summary>
            /// <param name="coinNameOrSymbol"></param>
            /// <returns>
            /// coin id corresponding to <paramref name="coinNameOrSymbol"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinNameOrSymbolNotFoundException">
            /// thrown if a <see cref="CoinListing"/> having <paramref name="coinNameOrSymbol"/> does not exist in
            /// repository
            /// </exception>
            public long GetCoinIdByNameOrSymbol(string coinNameOrSymbol)
            {
                assertManagerInitialized("GetCoinIdByNameOrSymbol");

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
                    throw new CoinNameOrSymbolNotFoundException(coinNameOrSymbol);
                }
            }

            public long[] GetCoinIdsByNamesOrSymbols(IList<string> coinNamesOrSymbols)
            {
                assertManagerInitialized("GetCoinIdByNameOrSymbol");

                long[] coinIds = new long[coinNamesOrSymbols.Count];

                for(int i = 0; i < coinNamesOrSymbols.Count; i++)
                {
                    coinIds[i] = GetCoinIdByNameOrSymbol(coinNamesOrSymbols[i]);
                }

                return coinIds;
            }

            /// <summary>
            /// returns coin name corresponding to <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// coin name corresponding to <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdNotFoundException">
            /// <seealso cref="GetCoinListing(long)"/>
            /// </exception>
            public string GetCoinNameById(long coinId)
            {
                assertManagerInitialized("GetCoinNameById");

                return GetCoinListing(coinId).Name;
            }

            /// <summary>
            /// returns coin symbol corresponding to <paramref name="coinId"/>.
            /// </summary>
            /// <param name="coinId"></param>
            /// <returns>
            /// coin symbol corresponding to <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdNotFoundException">
            /// <seealso cref="GetCoinListing(long)"/>
            /// </exception>
            public string GetCoinSymbolById(long coinId)
            {
                assertManagerInitialized("GetCoinSymbolById");

                return GetCoinListing(coinId).Symbol;
            }

            /// <summary>
            /// returns <see cref="CoinListing"/> corresponding to <paramref name="coinId"/>.
            /// </summary>
            /// <seealso cref="CoinIdExists(long)"/>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="CoinListing"/> corresponding to <paramref name="coinId"/>
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdNotFoundException">
            /// thrown if <see cref="CoinLisiting"/> corresponding to <paramref name="coinId"/> does
            /// not exist in repository
            /// </exception>
            public CoinListing GetCoinListing(long coinId)
            {
                assertManagerInitialized("GetCoinListing");

                if (CoinIdExists(coinId))
                {
                    return coinIdToCoinListing[coinId];
                }
                else
                {
                    throw new CoinIdNotFoundException(coinId);
                }
            }

            /// <summary>
            /// returns display string of <see cref="CoinListingTable"/> with
            /// rows corresponding to <paramref name="coinIds"/>. 
            /// </summary>
            /// <param name="coinIds"></param>
            /// <returns>
            /// display string of <see cref="CoinListingTable"/> with
            /// rows corresponding to <paramref name="coinIds"/>.  
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="CoinIdDoesNotExistException">
            /// <seealso cref="GetCoinListing(long)"/>
            /// </exception>
            public string GetCoinListingTableDisplayString(params long[] coinIds)
            {
                assertManagerInitialized("GetCoinListingTableDisplayString");

                // init coin listing table
                CoinListingTable coinListingTable = new CoinListingTable();

                foreach (long coinId in coinIds)
                {
                    // add row corresponding to each coin listing associated with specified id
                    CoinListing coinListing = GetCoinListing(coinId);
                    coinListingTable.AddRow(coinListing);
                }

                // return table display string
                string coinListingTableString = coinListingTable.GetTableDisplayString();

                return coinListingTableString;
            }

            /// <summary>
            /// returns array of coin IDs, corresponding to coin names / symbols in
            /// <paramref name="coinNameOrSymbolArray"/>.
            /// </summary>
            /// <seealso cref="GetCoinIdByNameOrSymbol(string)"/>
            /// <param name="coinNameOrSymbolArray"></param>
            /// <returns>
            /// array of coin IDs, corresponding to coin names / symbols in
            /// <paramref name="coinNameOrSymbolArray"/>.
            /// </returns>
            /// <exception cref="ManagerNotInitializedException">
            /// <seealso cref="GetCoinIdByNameOrSymbol(string)"/>
            /// </exception>
            /// <exception cref="CoinNameOrSymbolNotFoundException">
            /// <seealso cref="GetCoinIdByNameOrSymbol(string)"/>
            /// </exception>
            public long[] FetchCoinIds(string[] coinNameOrSymbolArray)
            {
                long[] coinIds = new long[coinNameOrSymbolArray.Length];

                for (int i = 0; i < coinNameOrSymbolArray.Length; i++)
                {
                    // try fetching coin id corresponding to i'th coin name / symbol
                    string coinNameOrSymbol = coinNameOrSymbolArray[i];
                    coinIds[i] = Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);
                }

                return coinIds;
            }

            /// <summary>
            /// synchroniously updates <see cref="CoinListing"/> repository with data from server.
            /// </summary>
            public void UpdateRepository()
            {
                try
                {
                    CoinListing[] coinListingArray = RequestHandler.RequestCoinListings();

                    // update id-to-CoinListing, name-to-CoinListing, symbol-to-CoinListing dictionaries
                    for (int i = 0; i < coinListingArray.Length; i++)
                    {
                        CoinListing curCoinListing = coinListingArray[i];

                        // get CoinListing fields in lowercase
                        int id = curCoinListing.Id;
                        string lowercaseName = curCoinListing.Name.ToLower();
                        string lowercaseSymbol = curCoinListing.Symbol.ToLower();

                        // update dictionaries
                        coinIdToCoinListing[id] = curCoinListing;
                        coinNameToCoinListing[lowercaseName] = curCoinListing;
                        coinSymbolToCoinListing[lowercaseSymbol] = curCoinListing;
                    }
                }
                catch (DataRequestException dataRequestException) // request unsuccessful
                {
                    throw new RepositoryUpdateException(dataRequestException);
                }
            }

            /// <summary>
            /// asserts that <see cref="CoinListingManager"/> is yet to be initialized.
            /// </summary>
            /// <exception cref="ManagerAlreadyInitializedException">
            /// thrown if <see cref="CoinListingManager"/> has already been initialized
            /// </exception>
            private void assertManagerNotYetInitialized()
            {
                if(managerInitialized)
                {
                    throw new ManagerAlreadyInitializedException();
                }
            }

            /// <summary>
            /// asserts that <see cref="CoinListingManager"/> has been initialized.
            /// </summary>
            /// <param name="requestedPropertyOrOperation">
            /// property or operation that was attempted to be requested / performed
            /// </param>
            /// <exception cref="ManagerNotInitializedException">
            /// thrown if <see cref="CoinListingManager"/> has not yet been initialized
            /// </exception>
            private void assertManagerInitialized(string requestedPropertyOrOperation)
            {
                if (!managerInitialized)
                {
                    throw new ManagerNotInitializedException(requestedPropertyOrOperation);
                }
            }
        }
    }
}

