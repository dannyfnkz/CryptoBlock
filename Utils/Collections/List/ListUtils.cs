using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections.List
    {
        public static class ListUtils
        {
            public static List<T> ListFromArrayRanges<T>(params ArrayRange<T>[] arrayRanges)
            {
                List<T> listFromArrayRanges = new List<T>();

                foreach(ArrayRange<T> arrayRange in arrayRanges)
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

            internal static void AssertItemIndexWithinRange<T>(
                IList<T> list,
                int index,
                string indexParameterName)
            {
                if (index < 0 || index >= list.Count)
                {
                    string exceptionMessage = string.Format(
                        "Value '{0}' of parameter '{1}' was out of range.",
                        index,
                        indexParameterName);

                    throw new IndexOutOfRangeException(exceptionMessage);
                }
            }

            internal static void AssertRangeIndexWithinRange<T>(
                IList<T> list,
                int index,
                string indexParameterName)
            {
                if (index < 0 || index > list.Count)
                {
                    string exceptionMessage = string.Format(
                        "Value '{0}' of parameter '{1}' was out of range.",
                        index,
                        indexParameterName);

                    throw new IndexOutOfRangeException(exceptionMessage);
                }
            }
        }
    }
}

