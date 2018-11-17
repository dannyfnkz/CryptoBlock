using CryptoBlock.Utils.Ints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Doubles
    {
        /// <summary>
        /// contains extension methods for <see cref="double"/>.
        /// </summary>
        public static class DoubleExtensionMethods
        {
            /// <summary>
            /// returns a string representation of the <paramref name="numberOfMostSignificantDigits"/>
            /// most significant digits of <paramref name="d"/>.
            /// </summary>
            /// <param name="d"></param>
            /// <param name="numberOfMostSignificantDigits"></param>
            /// <returns>
            /// <para>
            /// string representation of the <paramref name="numberOfMostSignificantDigits"/>
            /// most significant digits of <paramref name="d"/>;
            /// </para>
            /// <para>
            /// "0" if <paramref name="numberOfMostSignificantDigits"/> is less than or equal to 0
            /// </para>
            /// </returns>
            public static string toMostSignificantDigitString(
                this double d,
                int numberOfMostSignificantDigits)
            {
                string mostSignificantDigitStringRepresentation;

                int integerPart = (int)d;
                double mantissaPart = d - integerPart;

                int numberOfDigitsInIntegerPart = integerPart.getNumberOfDigits();

                if (numberOfDigitsInIntegerPart > numberOfMostSignificantDigits)
                {
                    // extract the [numberOfMostSignificantDigits] most significant digits in
                    // integer part
                    int numberOfDigitsDifference = numberOfDigitsInIntegerPart - numberOfMostSignificantDigits;
                    integerPart /= (int)Math.Pow(10, numberOfDigitsDifference);
                    numberOfDigitsInIntegerPart = numberOfMostSignificantDigits;
                }

                int numberOfDigitsInMantissaPart = numberOfMostSignificantDigits - numberOfDigitsInIntegerPart;

                if (numberOfDigitsInMantissaPart == 0)
                {
                    mostSignificantDigitStringRepresentation = integerPart.ToString();
                }
                else // numberOfDigitsInMantissaPart > 0
                {
                    // extract the [numberOfDigitsInMantissaPart] most significant digits
                    // in mantissa
                    int mantissaPartInt = (int)(mantissaPart * Math.Pow(10, numberOfDigitsInMantissaPart));

                    string signSymbolString = mantissaPartInt.getSignSymbolString();

                    // remove sign from integer antissa parts
                    integerPart = Math.Abs(integerPart);
                    mantissaPartInt = Math.Abs(mantissaPartInt);

                    mostSignificantDigitStringRepresentation = string.Format(
                          "{0}{1}.{2}",
                          signSymbolString,
                          integerPart.ToString(),
                          mantissaPartInt.ToString());
                }

                return mostSignificantDigitStringRepresentation;
            }
        }
    }
}
