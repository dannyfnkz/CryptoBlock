namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// represents a range having a lower and upper bound
        /// </summary>
        public struct Range
        {
            private readonly int lowerBound;
            private readonly int upperBound;

            public Range(int lowerBound, int upperBound)
            {
                this.lowerBound = lowerBound;
                this.upperBound = upperBound;
            }

            /// <summary>
            /// lower bound or range.
            /// </summary>
            public int LowerBound
            {
                get { return lowerBound; }
            }

            /// <summary>
            /// upper bound of range.
            /// </summary>
            public int UpperBound
            {
                get { return upperBound; }
            }

            /// <summary>
            /// returns whether <paramref name="value"/> is within range.
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// true if <paramref name="value"/> is within range,
            /// else false
            /// </returns>
            public bool IsWithinRange(int value)
            {
                return value >= lowerBound && value <= upperBound;
            }
        }
    }
}