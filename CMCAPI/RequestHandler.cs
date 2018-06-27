using System;
using System.Net;
using System.Collections.Generic;
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
        public static class RequestHandler
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

                //private static string formatErrorMessage(string message)
                //{
                //    return string.Format("Data request to server failed: {0}{1}", message, Environment.NewLine);
                //}
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

            public class CoinDataRequestInvalidStartIndexException : DataRequestException
            {
                public CoinDataRequestInvalidStartIndexException(int startIndex)
                    : base(formatExceptionMessage(startIndex))
                {

                }
                public CoinDataRequestInvalidStartIndexException(int startIndex, Exception innerException) 
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

            public class CoinDataRequestInvalidNumberOfCoinsException : DataRequestException
            {
                public CoinDataRequestInvalidNumberOfCoinsException(int lo, int hi)
                    : base(formatExceptionMessage(lo, hi))
                {

                }

                private static string formatExceptionMessage(int lo, int hi)
                {
                    return string.Format("Number of coins must be between {0} and {1}.", lo, hi);
                }
            }

            private const string BASE_URL = @"https://api.coinmarketcap.com/v2/";
            private const string COIN_DATA_REQUEST_URL = BASE_URL + @"ticker/";
            private const string LISTINGS_REQUEST_URL = BASE_URL + @"listings/";

            private const int COIN_DATA_REQUEST_MAX_NUMBER_OF_COINS = 100;
            private const string COIN_DATA_REQUEST_STRUCTURE_TYPE = "array";

            private static readonly Dictionary<eSortType, string> sortTypeToString = 
                new Dictionary<eSortType, string>();

            static RequestHandler()
            {
                sortTypeToString[eSortType.Id] = "id";
                sortTypeToString[eSortType.PercentChange24h] = "percent_change_24h";
                sortTypeToString[eSortType.Rank] = "rank";
                sortTypeToString[eSortType.Volume24h] = "volume_24h";
            }

            public static CoinData[] RequestCoinData(
                int startIndex,
                int numberOfCoins,
                eSortType sortType = eSortType.Id)
            {
                if(startIndex < 0)
                {
                    throw new CoinDataRequestInvalidStartIndexException(startIndex);
                }

                if(numberOfCoins <= 0 || numberOfCoins > COIN_DATA_REQUEST_MAX_NUMBER_OF_COINS)
                {
                    throw new CoinDataRequestInvalidNumberOfCoinsException(1, 100);
                }

                string uri = COIN_DATA_REQUEST_URL;
                HttpGetRequestHandler.GetRequestParameter[] requestParameters
                    = HttpGetRequestHandler.GetRequestParameter.ToGetRequestParameterArray(
                        new string[] { "start", "limit", "sort", "structure" },
                        new string[]
                        {
                            startIndex.ToString(),
                            numberOfCoins.ToString(),
                            sortTypeToString[sortType],
                            COIN_DATA_REQUEST_STRUCTURE_TYPE
                        });

                string serverResponseJson = sendDataRequest(uri, requestParameters);

                try
                {
                    CoinData[] coinDataArray = CoinData.ParseArray(serverResponseJson, startIndex, numberOfCoins);

                    return coinDataArray;
                }
                catch(CoinData.InvalidCoinIndexException invalidCoinIndexException)
                {
                    throw new CoinDataRequestInvalidStartIndexException(startIndex, invalidCoinIndexException);
                }
                catch (Data.DataParseException dataParseException)
                {
                    throw new ServerResponseParseException(dataParseException);
                }
            }

            /// <summary>
            /// returns a <see cref="CoinData"/>object encapsulating dynamic server data
            /// corresponding to coin with <paramref name="coinId"/>.
            /// </summary>
            /// <exception cref="InvalidCoinIdException">thrown if <paramref name="coinId"/>was invalid.</exception>
            /// <exception cref="DataRequestFailedException"><see cref="sendDataRequest(string)"/></exception>
            /// <exception cref="DataRequestUnsuccessfulStatusCodeException"><see cref="sendDataRequest(string)"/>
            /// </exception>
            /// <exception cref="InvalidServerResponse">thrown if a <see cref="CoinData"/>object could not
            /// be parsed from server response string.</exception>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="CoinData"/>object corresponding to coin with <paramref name="coinId"/>.
            /// </returns>
            public static CoinData RequestCoinData(int coinId)
            {
                CoinData coinData;

                if (coinId <= 0)
                {
                    throw new InvalidCoinIdException();
                }

                string uri = COIN_DATA_REQUEST_URL + coinId;

                string serverResponse = sendDataRequest(uri);

                try
                {
                    coinData = CoinData.Parse(serverResponse, coinId);
                    return coinData;
                }
                catch (CoinData.InvalidCoinIndexException invalidCoinIndexException)
                {
                    throw new CoinIdDoesNotExistException(coinId, invalidCoinIndexException);
                }
                catch (Data.DataParseException dataParseException)
                {
                    throw new ServerResponseParseException(dataParseException);
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
                    coinListingsArray = CoinListing.ParseStaticCoinDataArray(serverResponse);
                    return coinListingsArray;

                }
                catch (Data.DataParseException dataParseException)
                {
                    throw new ServerResponseParseException(dataParseException);
                }        
            }

            /// <summary>
            /// sends HTTP GET data request with specified <paramref name="uri"/>to server.
            /// returns the server response.
            /// </summary>
            /// <exception cref="DataRequestFailedException">thrown if trying to send data request to server 
            /// failed.</exception>
            /// <exception cref="DataRequestUnsuccessfulStatusCodeException">thrown if response received from server
            /// had an unsuccessful status code.</exception>
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
