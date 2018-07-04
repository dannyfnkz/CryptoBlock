using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class Range
        {
            private readonly int lowerBound;
            private readonly int upperBound;

            public Range(int lowerBound, int upperBound)
            {
                this.lowerBound = lowerBound;
                this.upperBound = upperBound;
            }

            public int LowerBound
            {
                get { return lowerBound; }
            }

            public int UpperBound
            {
                get { return upperBound; }
            }

            public bool IsWithinRange(int value)
            {
                return value >= lowerBound && value <= upperBound;
            }
        }
    }
}