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
        public static class DoubleExtensionMethods
        {
            // if numberOfMostSignificantDigits <=0  return "0'
            // if numberOfMostSignificantDigits > total number of digits in d (including integer
            // part and mantissa) pads with zeros to the right of the mantissa 
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
                    integerPart = IntUtils.GetNonNegative(integerPart);
                    mantissaPartInt = IntUtils.GetNonNegative(mantissaPartInt);

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
