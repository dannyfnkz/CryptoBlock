
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
        /// <summary>
        /// contains extension methods for arrays.
        /// </summary>
        public static class ArrayExtensionMethods
        {
            public static bool Contains<T>(this T[] array, T element)
            {
                return Array.IndexOf(array, element) > -1;
            }

            public static K[] CastAll<T,K>(this T[] sourceArray) where K : class where T : class
            {
                K[] resultArray = new K[sourceArray.Length];

                for(int i = 0; i < sourceArray.Length; i++)
                {
                    resultArray[i] = sourceArray[i] as K;
                }

                return resultArray;
            }

            /// <summary>
            /// returns subarray of <paramref name="sourceArray"/> containing items in range
            /// [<paramref name="startIndex"/>, <paramref name="sourceArray"/>.Count).
            /// </summary>
            /// <seealso cref="Subarray{T}(T[], int, int)"/>
            /// <typeparam name="T"></typeparam>
            /// <param name="sourceArray"></param>
            /// <param name="startIndex"></param>
            /// <returns>
            /// subarray of <paramref name="sourceArray"/> containing items in range
            /// [<paramref name="startIndex"/>, <paramref name="sourceArray"/>.Count).
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="Subarray{T}(T[], int, int)"/>
            /// </exception>
            public static T[] Subarray<T>(this T[] sourceArray, int startIndex)
            {
                return Subarray(sourceArray, startIndex, sourceArray.Length);
            }


            /// <summary>
            /// returns subarray of <paramref name="sourceArray"/> containing items in range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>).
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sourceArray"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <returns>
            /// subarray of <paramref name="sourceArray"/> containing items in range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>)
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="ListUtils.AssertRangeIndicesValid{T}(int, int, IList{T})"/>
            /// </exception>
            public static T[] Subarray<T>(this T[] sourceArray, int startIndex, int endIndex)
            {
                ListUtils.AssertRangeIndicesValid(startIndex, endIndex, sourceArray);

                T[] subarray;

                // allocate memory for subArray
                int subarrayLength = endIndex - startIndex;
                subarray = new T[subarrayLength];

                // copy element from sourceArray into subArray
                Array.Copy(sourceArray, startIndex, subarray, 0, subarrayLength);

                return subarray;
            }

            /// <summary>
            /// returns item at <paramref name="index"/> in <paramref name="array"/>
            /// if <paramref name="index"/>.HasValue, null otherwise.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="index"></param>
            /// <returns>
            /// item at <paramref name="index"/> in <paramref name="array"/>
            /// if <paramref name="index"/>.HasValue
            /// else null
            /// </returns>
            public static T GetItemAtIndexOrNull<T>(this T[] array, int? index) where T : class
            {
                T itemAtIndex;

                if(index.HasValue)
                {
                    int indexValue = index.GetValueOrDefault();

                    ListUtils.AssertItemIndexValid(indexValue, array, "index");

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
