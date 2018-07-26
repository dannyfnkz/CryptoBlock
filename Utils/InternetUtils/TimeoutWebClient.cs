using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.InternetUtils
    {
        /// <summary>
        /// <see cref="System.Net.WebClient"/> with a configurable request timeout.
        /// </summary>
        // required to make TimeoutWebClient open in code editor
        // as WebClient inherits from System.ComponentModel.Component which is configured to use designer view
        [System.ComponentModel.DesignerCategory("Code")]
        public class TimeoutWebClient : WebClient
        {
            // default request timeout
            private const int DEFAULT_REQUEST_TIMEOUT_MILLIS = 10 * 1000;

            // request timeout in milliseconds
            private int requestTimeoutMillis;

            public TimeoutWebClient(int requestTimeoutMillis = DEFAULT_REQUEST_TIMEOUT_MILLIS)
            {
                this.requestTimeoutMillis = requestTimeoutMillis;
            }

            /// <summary>
            /// request timeout value in milliseconds.
            /// </summary>
            public int RequestTimeoutMillis
            {
                get { return requestTimeoutMillis; }
                set
                {
                    if (requestTimeoutMillis < 0)
                    {
                        throw new ArgumentException(
                            "Request timeout value must be a non-negative integer.", "TimeoutMillis");
                    }

                    requestTimeoutMillis = value;
                }
            }

            /// <summary>
            /// overridden in order to configure request timeout.
            /// </summary>
            /// <seealso cref="System.Net.WebClient.GetWebRequest(Uri)"/>
            /// <seealso cref="System.Net.WebRequest"/>
            /// <param name="uri"></param>
            /// <returns></returns>
            protected override WebRequest GetWebRequest(Uri uri)
            {
                // get web client web request
                WebRequest webRequest = base.GetWebRequest(uri);

                // set request timeout
                webRequest.Timeout = requestTimeoutMillis;
                ((HttpWebRequest)webRequest).ReadWriteTimeout = requestTimeoutMillis;

                return webRequest;
            }
        }
    }
}

