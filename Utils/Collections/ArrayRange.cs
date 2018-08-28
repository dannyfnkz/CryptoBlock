using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        public class ArrayRange<T>
        {
            private readonly T[] array;
            private readonly int startIndex;
            private readonly int endIndex;

            // exclusive
            public ArrayRange(T[] array, int startIndex, int endIndex)
            {
                assertValidIndices(array, startIndex, endIndex);

                this.array = array;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }

            public ArrayRange(T[] array, int startIndex)
                : this(array, startIndex, array.Length)
            {

            }

            public ArrayRange(T element)
            : this(new T[] { element}, 0, 1)
            {

            }

            public int StartIndex
            {
                get { return startIndex; }
            }

            public int EndIndex
            {
                get { return endIndex; }
            }

            public T[] SubArray
            {
                get { return array.SubArray(startIndex, endIndex); }
            }

            private static void assertValidIndices(T[] array, int startIndex, int endIndex)
            {
                CollectionUtils.AssertIndexValid(startIndex, array);
                CollectionUtils.AssertIndexValid(endIndex, array);
                CollectionUtils.AssertStartAndEndIndicesValid(startIndex, endIndex);
            }
        }
    }
}
