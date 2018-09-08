using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections.List
    {
        /// <summary>
        /// contains utility method for <see cref="IList{T}"/>.
        /// </summary>
        public static class ListUtils
        {
            /// <summary>
            /// returns a list containing items from <paramref name="arrayRanges"/>, maintaining
            /// the specified order between items (both within each <see cref="ArrayRange{T}"/>
            /// and between two different <see cref="ArrayRange{T}"/>s).
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arrayRanges"></param>
            /// <returns>
            /// list containing items from <paramref name="arrayRanges"/>, maintaining
            /// the specified order between items (both within each <see cref="ArrayRange{T}"/>
            /// and between two different <see cref="ArrayRange{T}"/>s)
            /// </returns>
            public static List<T> ListFromArrayRanges<T>(params ArrayRange<T>[] arrayRanges)
            {
                // result list
                List<T> listFromArrayRanges = new List<T>();

                // adds element from every ArrayRange in input array to result list
                foreach (ArrayRange<T> arrayRange in arrayRanges)
                {
                    T[] arrayRangeSubArray = arrayRange.SubArray;
                    listFromArrayRanges.AddRange(arrayRangeSubArray);
                }

                return listFromArrayRanges;
            }

            /// <summary>
            /// returns whether all items in <paramref name="list"/> are equal to <paramref name="item"/>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="list"></param>
            /// <param name="t"></param>
            /// <returns></returns>
            public static bool AllItemsEqualTo<T>(IList<T> list, T item)
            {
                foreach (T curItem in list)
                {
                    if (!curItem.Equals(item)) // a list item is not equal to item
                    {
                        return false;
                    }
                }

                // all list items equal to item
                return true;
            }

            /// <summary>
            /// asserts that <paramref name="index"/> represents a valid item index in
            /// <paramref name="list"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="index"></param>
            /// <param name="list"></param>
            /// <param name="indexParameterName"
            /// <exception cref="IndexOutOfRangeException">
            /// thrown if <paramref name="index"/> does not represent a valid index
            /// in <paramref name="list"/>
            /// </exception>
            internal static void AssertItemIndexValid<T>(int index, IList<T> list, string indexParameterName)
            {
                if (index < 0 || index > list.Count())
                {
                    string exceptionMessage = string.Format(
                        "Value of '{0}' must be non-negative and smaller than number of elements in" +
                        " list.",
                        indexParameterName);

                    throw new IndexOutOfRangeException(exceptionMessage);
                }
            }

            /// <summary>
            /// asserts that <paramref name="startIndex"/> and <paramref name="endIndex"/> represent
            /// valid range indices in <paramref name="list"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <param name="list"></param>
            /// <exception cref="IndexOutOfRangeException">
            /// thrown if <paramref name="startIndex"/> or <paramref name="endIndex"/> are not valid
            /// range indices in <paramref name="list"/>
            /// </exception>
            internal static void AssertRangeIndicesValid<T>(
                int startIndex,
                int endIndex,
                IList<T> list)
            {
                // assert that startIndex and endIndex are within list range
                bool startIndexWithinRange = IsRangeIndexWithinRange(startIndex, list);
                bool endIndexWithinRange = IsRangeIndexWithinRange(endIndex, list);

                if (!startIndexWithinRange || !endIndexWithinRange)
                {
                    string parameterName = !startIndexWithinRange ? "start index" : "end index";
                    string exceptionMessage = string.Format(
                        "Value of '{0}' must be non-negative and smaller than or equal to number of" +
                        " elements in list.",
                        parameterName);

                    throw new IndexOutOfRangeException(exceptionMessage);
                }
                else if(startIndex >= endIndex) // assert that startIndex is smaller than endIndex
                {
                    string exceptionMessage = "Start index must be smaller than end index.";
                    throw new IndexOutOfRangeException(exceptionMessage);
                }
            }

            /// <summary>
            /// returns whether <paramref name="rangeIndex"/> is within <paramref name="list"/> range.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="rangeIndex"></param>
            /// <param name="list"></param>
            /// <returns>
            /// whether <paramref name="rangeIndex"/> is within <paramref name="list"/> range
            /// </returns>
            private static bool IsRangeIndexWithinRange<T>(int rangeIndex, IList<T> list)
            {
                return rangeIndex >= 0 && rangeIndex <= list.Count;
            }
        }
    }
}

