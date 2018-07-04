using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public class CollectionUtils
        {
            public static T[] ConvertToArray<T>(IEnumerable<T> enumerable)
            {
                if (enumerable == null)
                    throw new ArgumentNullException("enumerable");

                return enumerable as T[] ?? enumerable.ToArray();
            }

            public static int GetHashCode(object obj)
            {
                // get all instance fields in obj
                FieldInfo[] instanceFields = ReflectionUtils.GetInstanceFieldInfo(obj.GetType());

                int hash = 17;
                int prime = 23;
                
                for(int i = 0; i < instanceFields.Length; i++)
                {
                    object fieldValue = instanceFields[i].GetValue(obj);

                    if(fieldValue != null)
                    {
                        hash = hash * prime + fieldValue.GetHashCode();
                    }
                }

                return hash;
            }

            public static bool AllItemsEqualTo<T>(IList<T> list, T t)
            {
                foreach(T curItem in list)
                {
                    if(!curItem.Equals(t)) // a list item is not equal to t
                    {
                        return false;
                    }
                }

                // all list items equal to t
                return true;
            }

            public static T[] DuplicateToArray<T>(T item, int arraySize)
            {
                T[] duplicatedArray = new T[arraySize];

                for(int i = 0; i < arraySize; i++)
                {
                    duplicatedArray[i] = item;
                }

                return duplicatedArray;
            }

            public static int GetTotalCount<T>(params IEnumerable<T>[] enumerables)
            {
                int totalCount = 0;
                
                foreach(IEnumerable<T> enumerable in enumerables)
                {
                    totalCount += enumerable.Count();
                }

                return totalCount;
            }

            public static T[] MergeToArray<T>(params IEnumerable<T>[] enumerables)
            {
                int mergedArraySize = GetTotalCount(enumerables);

                T[] mergedArray = new T[mergedArraySize];

                int mergedArrayIndex = 0;
                foreach(IEnumerable<T> enumerable in enumerables)
                {
                    foreach(T item in enumerable)
                    {
                        mergedArray[mergedArrayIndex++] = item;
                    }
                }

                return mergedArray;
            }
        }
    }
}

