using CryptoBlock.Utils;
using System;
using System.Net;
using System.Net.Http;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// handles a single asynchronous HTTP GET request.
        /// <remarks>
        /// each <see cref="HttpGetRequestHandler"/>is intended for a single HTTP request. 
        /// to send an additional request, instantiate a new <see cref="HttpGetRequestHandler"/>object.
        /// </remarks>
        /// </summary>
        internal class HttpGetRequestHandler
        {
            /// <summary>
            /// thrown if an error occurres while trying to call one of <see cref="HttpGetRequestHandler"/>'s methods.
            /// </summary>
            internal class HttpGetRequestHandlerException : Exception
            {
            }

            /// <summary>
            /// thrown when trying to access one of <see cref="HttpGetRequestHandler"/>'s properties
            /// before <see cref="SendRequest()"/> was called. 
            /// </summary>
            internal class RequestNotSentYetException : Exception
            {
                internal RequestNotSentYetException() : base("Call SendRequest() first.")
                {
                }
            }

            /// <summary>
            /// thrown when trying to call <see cref="SendRequest()"/>again after it was already called.
            /// </summary>
            internal class RequestAlreadySentException : Exception
            {
                internal RequestAlreadySentException()
                    : base("Instantiate a new HttpGetRequestHandler object to send a new request.")
                {
                }
            }

            /// <summary>
            /// <exception cref="RequestFailedException">thrown if <see cref="SendRequest"/> was called but request
            /// failed to be sent, or server failed to respond.</exception>
            /// </summary>
            internal class RequestFailedException : Exception
            {
                internal RequestFailedException()
                    : base("Request failed to be sent to server.")
                {

                }
            }

            private readonly string uri;
            private bool requestSent;
            private bool requestFailed;
            private bool successfulStatusCode;
            private string response;
            private HttpStatusCode statusCode;
            private Exception exception;
            private string exceptionMessage;

            internal HttpGetRequestHandler(string uri)
            {
                this.uri = uri;
            }

            internal string Uri { get { return uri; } }

            internal bool RequestSent { get { return requestSent; } }

            internal bool RequestFailed { get { return requestFailed; } }

            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSentSuccessfully()"/></exception>
            /// <exception cref="RequestFailedException"><see cref="assertRequestSentSuccessfully()"/></exception>
            internal bool SuccessfulStatusCode
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return successfulStatusCode;
                }
            }

            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSentSuccessfully()"/></exception>
            /// <exception cref="RequestFailedException"><see cref="assertRequestSentSuccessfully()"/></exception>
            internal string Response
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return response;
                }
            }

            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSentSuccessfully()"/></exception>
            /// <exception cref="RequestFailedException"><see cref="assertRequestSentSuccessfully()"/></exception>
            internal HttpStatusCode StatusCode
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return statusCode;
                }
            }

            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSent()"/>
            internal Exception Exception
            {
                get
                {
                    assertRequestSent();
                    return exception;
                }
            }

            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSent()"/>
            internal string ExceptionMessage
            {
                get
                {
                    assertRequestSent();
                    return exceptionMessage;
                }
            }

            /// <summary>
            /// asserts that <see cref="SendRequest()"/>was called, and server returned response with
            /// a successful status code.
            /// </summary>
            /// <exception cref="RequestNotSentYetException">thrown if <see cref="SendRequest"/>was not yet
            /// called.</exception>
            /// <exception cref="RequestFailedException">thrown if <see cref="SendRequest"/> was called but request
            /// failed to be sent, or server failed to respond..</exception>
            private void assertRequestSentSuccessfully()
            {
                if (!requestSent)
                {
                    throw new RequestNotSentYetException();
                }
                else if (requestFailed)
                {
                    throw new RequestFailedException();
                }
            }

            /// <summary>
            /// asserts that <see cref="SendRequest()"/>was called.
            /// </summary>
            /// <exception cref="RequestNotSentYetException">thrown if <see cref="SendRequest"/>was not yet
            /// called.</exception>
            private void assertRequestSent()
            {
                if (!requestSent)
                {
                    throw new RequestNotSentYetException();
                }
            }

            /// <summary>
            /// sends HTTP GET request to server asynchronously.
            /// </summary>
            /// <exception cref="RequestAlreadySentException">thrown if <see cref="SendRequest"/>was already called.
            /// </exception>
            internal void SendRequest()
            {
                if (requestSent)
                {
                    throw new RequestAlreadySentException();
                }

                requestSent = true;

                using (
                    HttpClient client = new HttpClient(
                        new HttpClientHandler
                        {
                            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                        }))
                {
                    client.BaseAddress = new Uri(uri);

                    try
                    {
                        HttpResponseMessage response = client.GetAsync(string.Empty).Result;

                        statusCode = response.StatusCode;

                        if (response.IsSuccessStatusCode)
                        {
                            this.response = response.Content.ReadAsStringAsync().Result;
                            successfulStatusCode = true;
                        }
                        else
                        {
                            successfulStatusCode = false;
                        }
                    }

                    catch (AggregateException aggregateException)
                    {
                        requestFailed = true;
                        exception = ExceptionUtils.ToException("HTTP GET request failed.", aggregateException);
                        exceptionMessage = exception.Message;
                    }
                }
            }
        }
    }
}