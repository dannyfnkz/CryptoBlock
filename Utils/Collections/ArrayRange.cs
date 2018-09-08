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
        /// represents a range of elements [<see cref="startIndex"/>, <see cref="endIndex"/>)
        /// in a specified array.                   
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ArrayRange<T>
        {
            // subarray containing items in range [startIndex, endIndex) from array specified
            // in constructor
            private readonly T[] subArray;

            // start index of range
            private readonly int startIndex;

            // end index of range
            private readonly int endIndex;

            /// <summary>
            /// represents an array range of <paramref name="array"/>, starting from
            /// <paramref name="startIndex"/> and ending at <paramref name="array"/>.Length
            /// </summary>
            /// <param name="array"></param>
            /// <param name="startIndex"></param>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="ArrayRange(T[], int, int)"/>
            /// </exception>
            public ArrayRange(T[] array, int startIndex)
                : this(array, startIndex, array.Length)
            {

            }

            /// <summary>
            /// represents an <paramref name="array"/> range of 
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>).
            /// </summary>
            /// <param name="list"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="ListUtils.AssertRangeIndicesValid{T}(int, int, IList{T})"/>
            /// </exception>
            public ArrayRange(T[] array, int startIndex, int endIndex)
            {
                ListUtils.AssertRangeIndicesValid(startIndex, endIndex, array);

                this.startIndex = startIndex;
                this.endIndex = endIndex;

                this.subArray = array.GetRange(startIndex, endIndex);
            }

            /// <summary>
            /// represents an array range of size 1, of a single-element array 
            /// containing <paramref name="element"/>.
            /// </summary>
            /// <param name="element"></param>
            public ArrayRange(T element)
            : this(new T[] { element}, 0, 1)
            {

            }

            /// <summary>
            /// start index of array range.
            /// </summary>
            public int StartIndex
            {
                get { return startIndex; }
            }

            /// <summary>
            /// end index of array range.
            /// </summary>
            public int EndIndex
            {
                get { return endIndex; }
            }

            /// <summary>
            /// subarray containing items in range [<see cref="startIndex"/>, <see cref="endIndex"/>)
            /// from specified array.
            /// </summary>
            public T[] SubArray
            {
                get { return subArray; }
            }
        }
    }
}
