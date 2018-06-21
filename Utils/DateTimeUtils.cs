using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public static class DateTimeUtils
        {
            public static long GetUnixTimestamp()
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            public static string GetCurrentDateTimeString()
            {
                return DateTime.Now.ToString(@"yyyy-MM-dd hh:mm:ss");
            }
        }
    }
}

