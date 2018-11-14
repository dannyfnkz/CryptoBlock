using CryptoBlock.Utils.Doubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tables.StringTransformers;

namespace CryptoBlock
{
    namespace Utils.Tables.StringTransformers
    {
        public class DoubleStringTransformer : IStringTransformer
        {
            private const int DEFAULT_NUMBER_OF_MOST_SIGNIFICANT_DIGITS_IN_STRING_REPRESENTATION = 7;

            private readonly int numberOfMostSignificantDigits;

            public DoubleStringTransformer(
                int numberOfMostSignificantDigits = 
                    DEFAULT_NUMBER_OF_MOST_SIGNIFICANT_DIGITS_IN_STRING_REPRESENTATION)
            {
                this.numberOfMostSignificantDigits = numberOfMostSignificantDigits;
            }

            public string TransformToString(object obj)
            {
                double d = (double)obj; // transformer only used for double type
                return d.toMostSignificantDigitString(numberOfMostSignificantDigits);
            }
        }
    }
}