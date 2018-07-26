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
        [System.ComponentModel.DesignerCategory("Code")]
        public class TimeoutWebClient : WebClient
        {
            private const int DEFAULT_CONNECTION_TIMEOUT_MILLIS = 10 * 1000;

            private int timeoutMillis;

            public TimeoutWebClient(int timeoutMillis = DEFAULT_CONNECTION_TIMEOUT_MILLIS)
            {
                this.timeoutMillis = timeoutMillis;
            }

            public int TimeoutMillis
            {
                get { return timeoutMillis; }
                set
                {
                    if (timeoutMillis < 0)
                    {
                        throw new ArgumentException(
                            "Timeout vaule must be a non-negative integer.", "TimeoutMillis");
                    }

                    timeoutMillis = value;
                }
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest webRequest = base.GetWebRequest(uri);
                webRequest.Timeout = timeoutMillis;
                ((HttpWebRequest)webRequest).ReadWriteTimeout = timeoutMillis;
                return webRequest;
            }
        }
    }
}

