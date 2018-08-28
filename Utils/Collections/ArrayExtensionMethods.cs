
using CryptoBlock.Utils.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        public static class ArrayExtensionMethods
        {
            public static K[] CastAll<T,K>(this T[] sourceArray) where K : class where T : class
            {
                K[] resultArray = new K[sourceArray.Length];

                for(int i = 0; i < sourceArray.Length; i++)
                {
                    resultArray[i] = sourceArray[i] as K;
                }

                return resultArray;
            }

            // exclusive
            public static T[] SubArray<T>(this T[] sourceArray, int startIndex, int endIndex)
            {
                ListUtils.AssertRangeIndexWithinRange<T>(sourceArray, startIndex, "startIndex");
                ListUtils.AssertRangeIndexWithinRange<T>(sourceArray, endIndex, "endIndex");
                CollectionUtils.AssertStartAndEndIndicesValid(startIndex, endIndex);

                T[] subArray;

                // allocate memory for subArray
                int subArrayLength = endIndex - startIndex;
                subArray = new T[subArrayLength];

                // copy element from sourceArray into subArray
                Array.Copy(sourceArray, startIndex, subArray, 0, subArrayLength);

                return subArray;
            }

            public static T[] SubArray<T>(this T[] sourceArray, int startIndex)
            {
                return SubArray<T>(sourceArray, startIndex, sourceArray.Length);
            }

            public static T GetItemAtIndexOrNull<T>(this T[] array, int? index) where T : class
            {
                T itemAtIndex;

                if(index.HasValue)
                {
                    int indexValue = index.GetValueOrDefault();

                    ListUtils.AssertItemIndexWithinRange<T>(array, indexValue, "index");

                    itemAtIndex = array[indexValue];
                }
                else
                {
                    itemAtIndex = null;
                }

                return itemAtIndex;
            }
        }
    }

}
