using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        /// <summary>
        /// contains utility methods for various types of collections in the .NET framework.
        /// </summary>
        public static class CollectionUtils
        {
            /// <summary>
            /// converts <paramref name="enumerable"/> into an array.
            /// </summary>
            /// <seealso cref="System.Collections.Generic.IEnumerable{T}.ToArray()"/>
            /// <typeparam name="T"></typeparam>
            /// <param name="enumerable"></param>
            /// <returns>
            /// array representing <paramref name="enumerable"/>
            /// </returns>
            /// <exception cref="ArgumentNullException">thrown if <paramref name="enumerable"/> was null.</exception>
            public static T[] ConvertToArray<T>(IEnumerable<T> enumerable)
            {
                if (enumerable == null)
                    throw new ArgumentNullException("enumerable");

                return enumerable as T[] ?? enumerable.ToArray();
            }

            /// <summary>
            /// returns hashcode of <paramref name="obj"/> based on its instance fields (regardless of access level).
            /// </summary>
            /// <remarks>
            /// uses hash function:
            /// <c>
            /// hash = prime0;
            /// foreach (fieldInfo in instanceFields)
            ///     hash = hash * prime1 + fieldInfo.GetValue(obj);
            /// </c>
            /// where prime0, prime1 are prime numbers.
            /// </remarks>
            /// <param name="obj"></param>
            /// <returns>
            /// hashcode of <paramref name="obj"/> based on its instance fields
            /// </returns>
            public static int GetHashCode(object obj)
            {
                // get all instance fields in obj
                FieldInfo[] instanceFields = ReflectionUtils.GetInstanceFieldInfo(obj.GetType());

                int hash = 17;
                int prime = 23;

                for (int i = 0; i < instanceFields.Length; i++)
                {
                    object fieldValue = instanceFields[i].GetValue(obj);

                    if (fieldValue != null)
                    {
                        hash = hash * prime + fieldValue.GetHashCode();
                    }
                }

                return hash;
            }

            /// <summary>
            /// returns an array containing <paramref name="item"/> cloned <paramref name="arraySize"/> times.
            /// </summary>
            /// <remarks>
            /// uses shallow cloning.
            /// </remarks>
            /// <typeparam name="T"></typeparam>
            /// <param name="item"></param>
            /// <param name="arraySize"></param>
            /// <returns>
            /// array containing <paramref name="item"/> cloned <paramref name="arraySize"/> times
            /// </returns>
            public static T[] DuplicateToArray<T>(T item, int arraySize)
            {
                T[] duplicatedArray = new T[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    duplicatedArray[i] = item;
                }

                return duplicatedArray;
            }

            /// <summary>
            /// returns the total count of all <see cref="System.Collections.Generic.IEnumerable{T}"/>s
            /// in <paramref name="enumerables"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="enumerables"></param>
            /// <returns>
            /// total count of all <see cref="System.Collections.Generic.IEnumerable{T}"/>s
            /// in <paramref name="enumerables"/>.
            /// </returns>
            public static int GetTotalCount<T>(params IEnumerable<T>[] enumerables)
            {
                int totalCount = 0;

                foreach (IEnumerable<T> enumerable in enumerables) // add each individual count to totalCount
                {
                    totalCount += enumerable.Count();
                }

                return totalCount;
            }

            /// <summary>
            /// merges all individual items in<see cref="System.Collections.Generic.IEnumerable{T}"/>s
            /// contained in <paramref name="enumerables"/>
            /// into a single array.
            /// </summary>
            /// <seealso cref="GetTotalCount{T}(IEnumerable{T}[])"/>
            /// <typeparam name="T"></typeparam>
            /// <param name="enumerables"></param>
            /// <returns>
            /// an array containing all individual items in <see cref="System.Collections.Generic.IEnumerable{T}"/>s
            /// contained in <paramref name="enumerables"/>
            /// </returns>
            public static T[] MergeToArray<T>(params IEnumerable<T>[] enumerables)
            {
                int mergedArraySize = GetTotalCount(enumerables);
                
                T[] mergedArray = new T[mergedArraySize];

                int mergedArrayIndex = 0;
                foreach (IEnumerable<T> enumerable in enumerables) // go through all enumerables in array
                {
                    foreach (T item in enumerable) // add each individual item in current enumerable to mergedArray
                    {
                        mergedArray[mergedArrayIndex++] = item;
                    }
                }

                return mergedArray;
            }
        }
    }
}