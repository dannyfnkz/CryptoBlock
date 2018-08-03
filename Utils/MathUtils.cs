using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// provides utility methods for mathematical operations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// returns all sub-arrays of size <paramref name="subArraySize"/> in <paramref name="array"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="subsetSize"></param>
        /// <returns>
        /// matrix containing all sub-arrays of size <paramref name="subArraySize"/> in <paramref name="array"/>
        /// </returns>
        public static int[,] GetSubArrays(int[] array, int subArraySize)
        {
            int n = array.Length;
            int k = subArraySize;

            // number of subsets is n-choose-k
            int numSubsets = (int)BinomialCoefficient(n, k);

            // numSubsets subsets, each having k elements
            int[,] subsets = new int[numSubsets, k];

            if (k == 0) // empty set
            {
                return subsets;
            }

            // init k runners, each pointing to an array index
            // indices pointed to be indexRunners consitute a subset of array
            int[] indexRunners = new int[k];

            // init runner indices so that each runner index is larger than its left neighbor by one
            // (left-most runner is zero)
            for (int i = 1; i < k; i++)
            {
                indexRunners[i] = indexRunners[i - 1] + 1;
            }

            int subsetCount = 0;

            while (subsetCount < numSubsets) // fetch numSubsets subsets
            {
                if (indexRunners[k - 1] >= n) // right-most runner index reached array end
                {
                    int curRunner = k - 1;

                    // try incrementing index of runner to the left of curRunner
                    do
                    {
                        --curRunner;
                        ++indexRunners[curRunner];
                    }
                    // if incrementing index of curRunner causes right-most runner index to go
                    // beyond array bounds, try again with runner to the left of curRunner
                    while (indexRunners[curRunner] == n + 1 - k + curRunner);

                    // init runner indices so that each runner index is larger than its left neighbor by one
                    for (int i = curRunner + 1; i < k; i++)
                    {
                        indexRunners[i] = indexRunners[i - 1] + 1;
                    }
                }
                else // right-most runner index within array bounds
                {
                    for (int i = 0; i < k; i++) // fetch current subset pointed to by indexRunners
                    {
                        subsets[subsetCount, i] = array[indexRunners[i]];
                    }

                    // increment subset counter
                    ++subsetCount;

                    // increment right-most runner index
                    ++indexRunners[k - 1];
                }
            }

            return subsets;
        }

        /// <summary>
        /// returns the binomial coefficient (<paramref name="n"/>,<paramref name="k"/>) [n choose k].
        /// </summary>
        /// <remarks>
        /// calculation is based on the formula:
        /// (n,k) = (n/k) * (n-1,k-1)
        /// </remarks>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns>
        /// binomial coefficient (n,k)
        /// </returns>
        public static ulong BinomialCoefficient(int n, int k)
        {
            ulong binomialCoef = 1;

            for (int i = 1; i <= k; i++)
            {
                binomialCoef *= (ulong)(n - (k - i));
                binomialCoef /= (ulong)i;
            }

            return binomialCoef;
        }
    }
}
