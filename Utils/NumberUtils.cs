using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// provides utility methods for dealing with number types.
        /// </summary>
        public static class NumberUtils
        {
            /// <summary>
            /// tries parsing <paramref name="str"/> as double within bounds specified by
            /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>.
            /// returns whether parsing was successful,
            /// with <paramref name="parseResult"/> containing the parsed double.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="parseResult">parsed double, if parsing was successful</param>
            /// <param name="lowerBound">lower bound of parsed double (inclusive)</param>
            /// <param name="upperBound">upper bound of parsed double (exclusive)</param>
            /// <returns>
            /// true if parsing was successful, else false
            /// </returns>
            public static bool TryParseDouble(
                string str,
                out double parseResult,
                double lowerBound = double.MinValue,
                double upperBound = double.MaxValue)
            {
                bool parseSuccess = double.TryParse(str, out parseResult);

                bool parseSuccessAndWithinBounds = parseSuccess 
                    && parseResult >= lowerBound 
                    && parseResult < upperBound;

                return parseSuccessAndWithinBounds;
            }

            /// <summary>
            /// asserts that <paramref name="value"/> is within range [<paramref name="lo"/>, <paramref name="hi"/>].
            /// </summary>
            /// <param name="value"></param>
            /// <param name="lo"></param>
            /// <param name="hi"></param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// thrown if <paramref name="value"/> is out of range [<paramref name="lo"/>, <paramref name="hi"/>]
            /// </exception>
            public static void AssertInRange(int value, int lo, int hi)
            {
                if(value < lo || value > hi)
                {
                    string exceptionMessage = string.Format(
                        "int value was out of range [{0},{1}]",
                        lo,
                        hi);
                    throw new ArgumentOutOfRangeException(exceptionMessage, (Exception)null);
                }
            }

            /// <summary>
            /// asserts that <paramref name="value"/> is larger than or equal to <paramref name="lo"/>.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="lo"></param>
            /// <param name="hi"></param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// thrown if <paramref name="value"/> is smaller than <paramref name="lo"/>
            /// </exception>
            public static void AssertAtLeast(int value, int lo)
            {
                if (value < lo)
                {
                    string exceptionMessage = string.Format(
                        "int value was smaller than {0}",
                        lo);
                    throw new ArgumentOutOfRangeException(exceptionMessage, (Exception)null);
                }
            }
        }
    }
}