using System;
using System.Net;

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
            public class InvalidServerResponse : DataRequestException
            {
                public InvalidServerResponse(string dataParseExceptionMessage, Exception innerException)
                    : base(
                          string.Format("Server response could not be parsed: {0}", dataParseExceptionMessage),
                          innerException)
                {

                }

                public InvalidServerResponse(Exception innerException) : base(null, innerException)
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

            private const string BASE_URL = @"https://api.coinmarketcap.com/v2/";
            private const string COIN_DATA_REQUEST_URL = BASE_URL + @"ticker/";
            private const string LISTINGS_REQUEST_URL = BASE_URL + @"listings/";

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
                    coinData = CoinData.Parse(serverResponse);
                    return coinData;
                }
                catch (DataParseException dataParseException)
                {
                    throw new InvalidServerResponse(dataParseException);
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
                catch (DataParseException dataParseException)
                {
                    throw new InvalidServerResponse(dataParseException);
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
            private static string sendDataRequest(string uri)
            {
                string serverResponse = null;

                HttpGetRequestHandler httpGetRequestHandler = new HttpGetRequestHandler(uri);
                httpGetRequestHandler.SendRequest();

                // server responded with a successful status code
                if (!httpGetRequestHandler.RequestFailed && httpGetRequestHandler.SuccessfulStatusCode)
                {
                    serverResponse = httpGetRequestHandler.Response;
                }
                // an error occurred while trying to send request, or server returned an unsuccessful status code
                else if (httpGetRequestHandler.RequestFailed)
                {
                    throw new DataRequestFailedException(httpGetRequestHandler.Exception);
                }
                else // server returned an unsuccessful status code
                {
                    throw new DataRequestUnsuccessfulStatusCodeException(httpGetRequestHandler.StatusCode);
                }

                return serverResponse;
            }
        }
    }

}
