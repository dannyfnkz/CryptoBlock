using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Ints
    {
        /// <summary>
        /// contains extension methods for <see cref="int"/>
        /// </summary>
        public static class IntExtensionMethods
        {
            private const char INVISIBLE_CHARACTER = '\u0001';

            /// <summary>
            /// returns the number of digits in <paramref name="i"/>.
            /// </summary>
            /// <param name="i"></param>
            /// <returns>
            /// number of digits in <paramref name="i"/>
            /// </returns>
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

            /// <summary>
            /// returns the string representation of the sign of <paramref name="i"/>.
            /// </summary>
            /// <param name="i"></param>
            /// <returns>
            /// string representation of the sign of <paramref name="i"/>
            /// </returns>
            public static string getSignSymbolString(this int i)
            {
                return i < 0 ? "-" : string.Empty;
            }
        }
    }
}

