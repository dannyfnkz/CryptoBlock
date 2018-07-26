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
        public static class InternetUtils
        {
            private const string STABLE_WEB_HOST_URL = @"http://clients3.google.com/generate_204";
            private const int CONNECTION_TIMEOUT_MILLIS = 10 * 1000;

            public static bool IsConnectedToInternet()
            {
                bool connectedToInternet;

                try
                {
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
