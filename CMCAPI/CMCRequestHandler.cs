using System;
using System.Net;
using System.Collections.Generic;
using static CryptoBlock.CMCAPI.CoinListing;
using static CryptoBlock.CMCAPI.CoinTicker;
using static CryptoBlock.Utils.InternetUtils.HttpGetRequestHandler;
using CryptoBlock.Utils.InternetUtils;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// provides methods for requesting data from coinmarketcap.com.
        /// </summary>
        /// <remarks>
        /// uses coinmarkcap public API Version 2.
        /// </remarks>
        public static class CMCRequestHandler
        {
            public enum eSortType
            {
                Id, Rank, Volume24h, PercentChange24h
            }

            /// <summary>
            /// thrown if error occurs while trying to request data from server. 
            /// </summary>
            public class DataRequestException : Exception
            {
                public DataRequestException(string message, Exception innerException) 
                    : base(message, innerException)
                {

                }

                public DataRequestException(string message) : base(message)
                {

                }

                public DataRequestException(Exception innerException) : base(null, innerException)
                {

                }
            }

            /// <summary>
            /// thrown if sending data request to server failed.
            /// </summary>
            public class DataRequestFailedException : DataRequestException
            {
                public DataRequestFailedException(Exception innerException) 
                    : base("Data request to server failed.", innerException)
                {

                }
            }

            /// <summary>
            /// thrown if sending data request to server resulted in a server response with unsuccessful 
            /// status code.
            /// </summary>
            public class DataRequestUnsuccessfulStatusCodeException : DataRequestException
            {
                public DataRequestUnsuccessfulStatusCodeException(HttpStatusCode httpStatusCode)
                    : base(formatErrorMessage(httpStatusCode))
                {

                }

                private static string formatErrorMessage(HttpStatusCode statusCode)
                {
                    return string.Format("Status code was: {0}", statusCode.ToString());
                }
            }

            /// <summary>
            /// thrown if server response string could not be parsed.
            /// </summary>
            public class ServerResponseParseException : DataRequestException
            {
                public ServerResponseParseException(Exception innerException)
                    : base(innerException)
                {

                }
            }

            /// <summary>
            /// thrown if coin id provided as argument was invalid (<c>coinId <= 0</c>)
            /// </summary>
            public class InvalidCoinIdException : DataRequestException
            {
                public InvalidCoinIdException() : base("Coin ID must be a positive integer.")
                {

                }
            }

            public class CoinIdDoesNotExistException : DataRequestException
            {
                public CoinIdDoesNotExistException(int coinId)
                    : base(formatExceptionMessage(coinId))
                {

                }

                public CoinIdDoesNotExistException(int coinId, Exception innerException)
                    : base(formatExceptionMessage(coinId), innerException)
                {

                }

                private static string formatExceptionMessage(int coinId)
                {
                    return string.Format("Coin id does not exist in database: {0}.", coinId);
                }
            }

            /// <summary>
            /// thrown if an invalid start index was specified for a <see cref="CoinTicker"/> data request.
            /// </summary>
            public class CoinTickerRequestInvalidStartIndexException : DataRequestException
            {
                public CoinTickerRequestInvalidStartIndexException(int startIndex)
                    : base(formatExceptionMessage(startIndex))
                {

                }
                public CoinTickerRequestInvalidStartIndexException(int startIndex, Exception innerException) 
                    : base(formatExceptionMessage(startIndex), innerException)
                {

                }

                private static string formatExceptionMessage(int startIndex)
                {
                    return string.Format(
                        "Start index must be a non-negative integer and not larger than" +
                        " total number of coins in database: {0}.",
                        startIndex);
                }
            }

            /// <summary>
            /// thrown if an invalid number of coins was specified for a <see cref="CoinTicker"/> data request.
            /// </summary>
            public class CoinTickerRequestInvalidNumberOfCoinsException : DataRequestException
            {
                public CoinTickerRequestInvalidNumberOfCoinsException(int lo, int hi)
                    : base(formatExceptionMessage(lo, hi))
                {

                }

                private static string formatExceptionMessage(int lo, int hi)
                {
                    return string.Format("Number of coins must be between {0} and {1}.", lo, hi);
                }
            }

            // base url for CMC server requests
            private const string BASE_URL = @"https://api.coinmarketcap.com/v2/";

            // url for coin ticker requests
            private const string TICKER_REQUEST_URL = BASE_URL + @"ticker/";

            // url for coin listing requests
            private const string LISTINGS_REQUEST_URL = BASE_URL + @"listings/";

            // max number of coins which can be requested in a coin ticker request
            private const int COIN_TICKER_REQUEST_MAX_NUMBER_OF_COINS = 100;

            // type of structure to store coin ticker data in server response
            private const string COIN_TICKER_REQUEST_STRUCTURE_TYPE = "array";

            private static readonly Dictionary<eSortType, string> sortTypeToString =
                new Dictionary<eSortType, string>()
            {
                {eSortType.Id, "id" },
                {eSortType.PercentChange24h, "percent_change_24h" },
                {eSortType.Rank, "rank" },
                {eSortType.Volume24h, "volume_24h" }
            };

            /// <summary>
            /// maximum number of coin tickers which can be returned in a single request.
            /// </summary>
            public static int CoinTickerRequestMaxNumberOfCoins
            {
                get { return COIN_TICKER_REQUEST_MAX_NUMBER_OF_COINS; }
            }

            /// <summary>
            /// requests from server <see cref="CoinTickers"/> with indices in range
            /// [<paramref name="startIndex"/>, (<paramref name="startIndex"/> + <paramref name="numberOfCoins"/>)].
            /// </summary>
            /// <param name="startIndex"></param>
            /// <param name="numberOfCoins"></param>
            /// <param name="sortType"></param>
            /// <returns>
            /// <see cref="CoinTicker"/> array of length <paramref name="numberOfCoins"/> containing
            /// <see cref="CoinTicker"/>s of indices in range
            /// [[<paramref name="startIndex"/>, (<paramref name="startIndex"/> + <paramref name="numberOfCoins"/>)]
            /// </returns>
            /// <exception cref="CoinTickerRequestInvalidStartIndexException">
            /// thrown if <paramref name="startIndex"/> < 0
            /// </exception>
            /// <exception cref="CoinTickerRequestInvalidNumberOfCoinsException">
            /// thrown if <paramref name="numberOfCoins"/> <= 0 ||
            /// <paramref name="numberOfCoins"/> > COIN_TICKER_REQUEST_MAX_NUMBER_OF_COINS
            /// </exception>
            /// <exception cref="CoinTickerRequestInvalidStartIndexException">
            /// thrown if <paramref name="startIndex"/> does not exist in server.
            /// </exception>
            /// <exception cref="ServerResponseParseException">
            /// thrown if server response was invalid.
            /// </exception>
            public static CoinTicker[] RequestCoinTicker(
                int startIndex,
                int numberOfCoins,
                eSortType sortType = eSortType.Id)
            {
                if(startIndex < 0) // invalid start index
                {
                    throw new CoinTickerRequestInvalidStartIndexException(startIndex);
                }

                // invalid number of coins
                if(numberOfCoins <= 0 || numberOfCoins > COIN_TICKER_REQUEST_MAX_NUMBER_OF_COINS)
                {
                    throw new CoinTickerRequestInvalidNumberOfCoinsException(1, 100);
                }
                
                // prepare request
                string uri = TICKER_REQUEST_URL;

                GetRequestParameter[] requestParameters = new GetRequestParameter[]
                {
                    new GetRequestParameter("start", startIndex.ToString()),
                    new GetRequestParameter("limit", numberOfCoins.ToString()),
                    new GetRequestParameter("sort", sortTypeToString[sortType].ToString()),
                    new GetRequestParameter("structure",  COIN_TICKER_REQUEST_STRUCTURE_TYPE)
                };

                string serverResponseJson = sendDataRequest(uri, requestParameters);

                try
                {
                    CoinTicker[] coinDataArray = CoinTicker.ParseArray(serverResponseJson, startIndex, numberOfCoins);

                    return coinDataArray;
                }
                catch(CoinIndexNotFoundException coinIndexNotFoundException)
                {
                    throw new CoinTickerRequestInvalidStartIndexException(startIndex, coinIndexNotFoundException);
                }
                catch (CoinTickerJsonParseException coinTickerJsonParseException)
                {
                    throw new ServerResponseParseException(coinTickerJsonParseException);
                }
            }

            /// <summary>
            /// returns a <see cref="CoinTicker"/>object encapsulating dynamic server data
            /// corresponding to coin with <paramref name="coinId"/>.
            /// </summary>
            /// <exception cref="InvalidCoinIdException">thrown if <paramref name="coinId"/>was invalid.</exception>
            /// <exception cref="DataRequestFailedException"><see cref="sendDataRequest(string)"/></exception>
            /// <exception cref="DataRequestUnsuccessfulStatusCodeException"><see cref="sendDataRequest(string)"/>
            /// </exception>
            /// <exception cref="InvalidServerResponse">thrown if a <see cref="CoinTicker"/>object could not
            /// be parsed from server response string.</exception>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="CoinTicker"/>object corresponding to coin with <paramref name="coinId"/>.
            /// </returns>
            public static CoinTicker RequestCoinTicker(int coinId)
            {
                CoinTicker CoinTicker;

                if (coinId <= 0)
                {
                    throw new InvalidCoinIdException();
                }

                string uri = TICKER_REQUEST_URL + coinId;

                string serverResponse = sendDataRequest(uri);

                try
                {
                    CoinTicker = CoinTicker.Parse(serverResponse, coinId);
                    return CoinTicker;
                }
                catch (CoinIndexNotFoundException coinIndexNotFoundException)
                {
                    throw new CoinIdDoesNotExistException(coinId, coinIndexNotFoundException);
                }
                catch (CoinTickerJsonParseException coinTickerJsonParseException)
                {
                    throw new ServerResponseParseException(coinTickerJsonParseException);
                }
            }

            /// <summary>
            /// return a <see cref="StaticCoinData"/>array containing data of all coins in database,
            /// each element encapsulating static server data corresponding to a specific coin.
            /// </summary>
            /// <exception cref="DataRequestFailedException"><see cref="sendDataRequest(string)"/></exception>
            /// <exception cref="DataRequestUnsuccessfulStatusCodeException"><see cref="sendDataRequest(string)"/>
            /// </exception>
            /// <exception cref="InvalidServerResponse">thrown if a <see cref="StaticCoinData"/>object could not
            /// be parsed from server response string.</exception>
            /// <returns>
            /// <see cref="StaticCoinData"/>array containing data of all coins in database,
            /// each element encapsulating static server data corresponding to a specific coin.
            /// </returns>
            public static CoinListing[] RequestCoinListings()
            {
                CoinListing[] coinListingsArray;

                string serverResponse = sendDataRequest(LISTINGS_REQUEST_URL);

                try
                {
                    coinListingsArray = CoinListing.ParseCoinListingArray(serverResponse);
                    return coinListingsArray;

                }
                catch (CoinListingJsonParseException coinListingJsonParseException)
                {
                    throw new ServerResponseParseException(coinListingJsonParseException);
                }        
            }

            /// <summary>
            /// sends HTTP GET data request with specified <paramref name="uri"/>to server.
            /// returns the server response.
            /// </summary>
            /// <exception cref="DataRequestFailedException">thrown if trying to send data request to server 
            /// failed.</exception>
            /// <param name="uri"></param>
            /// <returns>
            /// server response string.
            /// </returns>
            private static string sendDataRequest(
                string uri,
                HttpGetRequestHandler.GetRequestParameter[] requestParameters = null)
            {
                string serverResponse = null;

                HttpGetRequestHandler httpGetRequestHandler = new HttpGetRequestHandler(uri, requestParameters);
                httpGetRequestHandler.SendRequest();

                // server responded with a successful status code
                if (!httpGetRequestHandler.RequestFailed)
                {
                    serverResponse = httpGetRequestHandler.Response;
                }
                else // an error occurred while trying to send request
                {
                    throw new DataRequestFailedException(httpGetRequestHandler.Exception);
                }

                return serverResponse;
            }
        }
    }

}
