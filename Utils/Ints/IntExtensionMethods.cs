using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Ints
    {
        public static class IntExtensionMethods
        {
            private const char INVISIBLE_CHARACTER = '\u0001';

            public static int getNumberOfDigits(this int i)
            {
                int numberOfDigitsCount = 1;

                while(i >= 10)
                {
                    ++numberOfDigitsCount;
                    i /= 10;
                }

                return numberOfDigitsCount;
            }

            public static string getSignSymbolString(this int i)
            {
                return i < 0 ? "-" : string.Empty;
            }
        }
    }
}

