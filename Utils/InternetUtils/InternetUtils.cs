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
        /// contains internet utility methods.
        /// </summary>
        public static class InternetUtils
        {
            // stable web host used to test internet connectivity
            private const string STABLE_WEB_HOST_URL = @"http://clients3.google.com/generate_204";

            // timeout for internet request
            private const int CONNECTION_TIMEOUT_MILLIS = 10 * 1000;

            /// <summary>
            /// returns whether if device is connecte to internet
            /// </summary>
            /// <returns>
            /// true if device is connected to internet,
            /// else false
            /// </returns>
            public static bool IsConnectedToInternet()
            {
                bool connectedToInternet;

                try
                {
                    // init a connection and send request to a stable web host
                    using (TimeoutWebClient timeoutWebClient = new TimeoutWebClient(CONNECTION_TIMEOUT_MILLIS))
                    using (timeoutWebClient.OpenRead(STABLE_WEB_HOST_URL))
                    {
                        // connection succeeded
                        connectedToInternet = true;
                    }
                }
                catch // connection failed
                {
                    connectedToInternet = false;
                }

                return connectedToInternet;
            }  
        }
    }
}
