using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public static class NumberUtils
        {
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
        }
    }
}