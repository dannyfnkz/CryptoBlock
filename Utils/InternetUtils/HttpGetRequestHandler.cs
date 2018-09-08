using CryptoBlock.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CryptoBlock
{
    namespace Utils.InternetUtils
    {
        /// <summary>
        /// handles a single synchronous HTTP GET request.
        /// </summary>
        /// <remarks>
        /// each <see cref="HttpGetRequestHandler"/>is intended for a single HTTP request. 
        /// to send an additional request, instantiate a new <see cref="HttpGetRequestHandler"/>object.
        /// </remarks>
        public class HttpGetRequestHandler
        {
            public class GetRequestParameter
            {
                /// <summary>
                /// thrown during <see cref="GetRequestParameter"/> array parsing
                /// if parameter names array and parameter value array don't have the same length.
                /// </summary>
                public class ParameterNameValueArrayLengthMismatchException : Exception
                {
                    public ParameterNameValueArrayLengthMismatchException()
                        : base(formatExceptionMessage())
                    {

                    }

                    private static string formatExceptionMessage()
                    {
                        return "Parameter name array and value array must have the same length.";
                    }
                }

                private string name;
                private string value;

                public GetRequestParameter(string name, string value)
                {
                    this.name = name;
                    this.value = value;
                }

                public string Name { get { return name; } }
                public string Value { get { return value; } }

                public static GetRequestParameter[] ToGetRequestParameterArray(
                    string[] parameterNames,
                    string[] parameterValues)
                {
                    if(parameterNames.Length != parameterValues.Length)
                    {
                        throw new ParameterNameValueArrayLengthMismatchException();
                    }

                    GetRequestParameter[] parameters = new GetRequestParameter[parameterNames.Length];

                    for(int i = 0; i < parameters.Length; i++)
                    {
                        parameters[i] = new GetRequestParameter(parameterNames[i], parameterValues[i]);
                    }

                    return parameters;
                }

                /// <summary>
                /// appends <see cref="GetRequestParameter"/> (name,value) 
                /// pair to <paramref name="uriStringBuilder"/>ץ
                /// </summary>
                /// <param name="uriStringBuilder"></param>
                /// <param name="firstParameterInUri">
                /// whether this <see cref="GetRequestParameter"/> is the first parameter in
                /// <paramref name="firstParameterInUri"/>
                /// </param>
                internal void AppendToUri(StringBuilder uriStringBuilder, bool firstParameterInUri)
                {
                    if(firstParameterInUri)
                    {
                        uriStringBuilder.Append("?");
                    }

                    uriStringBuilder.Append(name);
                    uriStringBuilder.Append("=");
                    uriStringBuilder.Append(value);
                }
            }
            /// <summary>
            /// thrown if an error occurres while trying to call one of <see cref="HttpGetRequestHandler"/>'s methods.
            /// </summary>
            public class HttpGetRequestHandlerException : Exception
            {
                public HttpGetRequestHandlerException(string message)
                    : base(message)
                {

                }
            }

            /// <summary>
            /// thrown when trying to access one of <see cref="HttpGetRequestHandler"/>'s properties
            /// before <see cref="SendRequest()"/> was called. 
            /// </summary>
            public class RequestNotSentYetException : HttpGetRequestHandlerException
            {
                public RequestNotSentYetException() : base("Call SendRequest() first.")
                {

                }
            }

            /// <summary>
            /// thrown when trying to call <see cref="SendRequest()"/>again after it was already called.
            /// </summary>
            public class RequestAlreadySentException : HttpGetRequestHandlerException
            {
                public RequestAlreadySentException()
                    : base("Instantiate a new HttpGetRequestHandler object to send a new request.")
                {

                }
            }

            /// <summary>
            /// <exception cref="RequestFailedException">thrown if <see cref="SendRequest"/> was called but request
            /// failed to be sent, or server failed to respond.</exception>
            /// </summary>
            public class RequestFailedException : HttpGetRequestHandlerException
            {
                public RequestFailedException()
                    : base("Request failed to be sent to server.")
                {

                }
            }

            private readonly string uri;
            private readonly GetRequestParameter[] parameters;
            private bool requestSent;
            private bool requestFailed;
            private bool successfulStatusCode;
            private string response;
            private HttpStatusCode statusCode;
            private Exception exception;
            private string exceptionMessage;

            public HttpGetRequestHandler(string uri) : this(uri, new GetRequestParameter[0])
            {

            }

            public HttpGetRequestHandler(string uri, string[] parameterNames, string[] parameterValues)
                : this(uri, GetRequestParameter.ToGetRequestParameterArray(parameterNames, parameterValues))
            {

            }

            public HttpGetRequestHandler(string uri, GetRequestParameter[] parameters)
            {
                this.uri = uri;
                this.parameters = parameters ?? new GetRequestParameter[0];
            }

            public string Uri { get { return uri; } }

            public GetRequestParameter[] Parameters { get { return parameters; } }

            public int ParameterCount { get { return parameters.Length; } }

            /// <summary>
            /// whether request was sent to host.
            /// </summary>
            public bool RequestSent { get { return requestSent; } }

            /// <summary>
            /// whether request was not successfuly sent to host.
            /// </summary>
            public bool RequestFailed { get { return requestFailed; } }

            /// <summary>
            /// whether response received from host had a successful status code.
            /// </summary>
            /// <exception cref="RequestNotSentYetException">
            /// <see cref="assertRequestSentSuccessfully()"/>
            /// </exception>
            /// <exception cref="RequestFailedException">
            /// <see cref="assertRequestSentSuccessfully()"/>
            /// </exception>
            public bool SuccessfulStatusCode
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return successfulStatusCode;
                }
            }

            /// <summary>
            /// response string received from host.
            /// </summary>
            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSentSuccessfully()"/></exception>
            /// <exception cref="RequestFailedException"><see cref="assertRequestSentSuccessfully()"/></exception>
            public string Response
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return response;
                }
            }

            /// <summary>
            /// status code of response received from host.
            /// </summary>
            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSentSuccessfully()"/></exception>
            /// <exception cref="RequestFailedException"><see cref="assertRequestSentSuccessfully()"/></exception>
            public HttpStatusCode StatusCode
            {
                get
                {
                    assertRequestSentSuccessfully();
                    return statusCode;
                }
            }

            /// <summary>
            /// exception thrown while trying to send request to host.
            /// </summary>
            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSent()"/>
            public Exception Exception
            {
                get
                {
                    assertRequestSent();
                    return exception;
                }
            }

            /// <summary>
            /// message of exception thrown while trying to send request to host.
            /// </summary>
            /// <exception cref="RequestNotSentYetException"><see cref="assertRequestSent()"/>
            public string ExceptionMessage
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
            public void SendRequest()
            {
                if (requestSent)
                {
                    throw new RequestAlreadySentException();
                }

                requestSent = true;

                // append get request parameters to uri
                string uriWithAppendedParameters = getUriWithAppendedParameters(uri, parameters);

                using (
                    HttpClient client = new HttpClient(
                        new HttpClientHandler
                        {
                            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                        }))
                {
                    client.BaseAddress = new Uri(uriWithAppendedParameters);

                    try
                    {
                        HttpResponseMessage response = client.GetAsync(string.Empty).Result;

                        this.response = response.Content.ReadAsStringAsync().Result;

                        statusCode = response.StatusCode;

                        successfulStatusCode = response.IsSuccessStatusCode;
                    }

                    catch (AggregateException aggregateException)
                    {
                        requestFailed = true;
                        exception = ExceptionUtils.ToException("HTTP GET request failed.", aggregateException);
                        exceptionMessage = exception.Message;
                    }
                }
            }

            private static string getUriWithAppendedParameters(string uri, GetRequestParameter[] parameters)
            {
                StringBuilder uriStringBuilder = new StringBuilder(uri);

                bool firstParameterInUri = true;

                foreach(GetRequestParameter parameter in parameters)
                {
                    parameter.AppendToUri(uriStringBuilder, firstParameterInUri);

                    if(Array.IndexOf(parameters, parameter) < parameters.Length - 1)
                    {
                        uriStringBuilder.Append("&");
                    }

                    if (firstParameterInUri)
                    {
                        firstParameterInUri = false;
                    }
                }

                return uriStringBuilder.ToString();
            }
        }
    }
}