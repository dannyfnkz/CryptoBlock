using System.Collections.Generic;
using System;

namespace CryptoBlock
{
    namespace Utils.Collections.List
    {
        /// <summary>
        /// contains extension methods for <see cref="List{T}"/>
        /// </summary>
        public static class ListExtensionMethods
        {
            /// <summary>
            /// converts each element of type <typeparamref name="T"/> in <paramref name="list"/>
            /// into an element of derived type <typeparamref name="K"/> (in-place) using provided
            /// <paramref name="converter"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="K"></typeparam>
            /// <param name="list"></param>
            /// <param name="converter"><see cref="Converter{TInput, TOutput}"/> from
            /// <typeparamref name="T"/> to <typeparamref name="K"/>
            /// </param>
            public static void ConvertEachElement<T, K>(
                this IList<T> list,
                Converter<T, K> converter) where K : T
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = converter(list[i]);
                }
            }

            /// <summary>
            /// returns a subarray containing elements from <paramref name="sourceList"/> in
            /// range [<paramref name="startIndex"/>, <paramref name="endIndex"/>).
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sourceList"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <returns>
            /// subarray containing elements from <paramref name="sourceList"/> in
            /// range [<paramref name="startIndex"/>, <paramref name="endIndex"/>).
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="ListUtils.AssertRangeIndicesValid{T}(int, int, IList{T})"/>
            /// </exception>
            public static T[] GetRange<T>(
                this IList<T> sourceList,
                int startIndex,
                int endIndex)
            {
                ListUtils.AssertRangeIndicesValid(startIndex, endIndex, sourceList);

                int numberOfItemsInRange = endIndex - startIndex;
                T[] resultArray = new T[numberOfItemsInRange];

                // copy elements in range [startIndex, endIndex) in sourceList to resultArray
                for (int i = startIndex; i < endIndex; i++)
                {
                    T sourceListItem = sourceList[i];
                    int resultArrayIndex = i - startIndex;

                    resultArray[resultArrayIndex] = sourceListItem;
                }

                return resultArray;
            }

            /// <summary>
            /// adds elements from <paramref name="sourceList"/>
            /// in range [<paramref name="startIndex"/>,<paramref name="endIndex"/>)
            /// to end of <paramref name="destinationList"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="destinationList"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <param name="sourceList"></param>
            public static void AddRange<T>(
                this List<T> destinationList,     
                int startIndex,
                int endIndex,
                IList<T> sourceList)
            {
                ListUtils.AssertRangeIndicesValid(startIndex, endIndex, sourceList);

                int numberOfItemsToBeAddedToList = (endIndex - startIndex);
                int numberOfItemsInListAfterAdd = destinationList.Count + numberOfItemsToBeAddedToList;

                // number of items in list after add is greater than current list capacity,
                // and doubling list capacity fails to allocate enough space for added items
                if (destinationList.Capacity * 2 < numberOfItemsInListAfterAdd)
                {
                    // manually allocate additional memory to list 
                    destinationList.Capacity = numberOfItemsInListAfterAdd;
                }

                // add items from sourceList in range [beginIndex - endIndex) to destinationList
                for(int i = startIndex; i < endIndex; i++)
                {
                    T sourceListItem = sourceList[i];
                    destinationList.Add(sourceListItem);
                }
            }
        }
    }
}
