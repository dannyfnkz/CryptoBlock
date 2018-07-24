using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.CollectionUtils
    {
        // space O(capacity)
        // ElementAt O(1)
        // all other operations as stack
        public class SearchableStack<T>
        {
            public class EmptyStackException : Exception
            {
                public EmptyStackException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Stack is empty.";
                }
            }

            private readonly int capacity;

            public readonly T[] array;
            private int lastElementIndex;
            private int numberOfElements;

            public SearchableStack(int capacity)
            {
                this.capacity = capacity;

                this.array = new T[capacity];
                this.lastElementIndex = capacity - 1;              
            }

            public int Capacity
            {
                get { return capacity; }
            }

            public bool Empty
            {
                get { return Count == 0; }
            }

            public int Count
            {
                get { return numberOfElements; } 
            }

            public void Push(T t)
            {
                int insertIndex;

                insertIndex = lastElementIndex + 1 < capacity ? lastElementIndex + 1 : 0;

                array[insertIndex] = t;
                lastElementIndex = insertIndex;

                if(numberOfElements < capacity)
                {
                    ++numberOfElements;
                }
            }

            public T Pop()
            {
                assertNotEmpty();

                T popped = array[lastElementIndex];

                lastElementIndex = lastElementIndex == 0 ? capacity - 1 : lastElementIndex - 1;
                --numberOfElements;

                return popped;
            }

            public T TopElement()
            {
                return ElementAt(0);
            }

            public T ElementAt(int indexFromTop)
            {
                assertNotEmpty();
                assertValidIndex(indexFromTop);

                int elementArrayIndex = ((lastElementIndex - indexFromTop) + capacity) % capacity;

                return array[elementArrayIndex];
            }

            public bool HasElementAt(int indexFromTop)
            {
                return indexFromTop >= 0 && indexFromTop <= numberOfElements - 1;
            }

            private void assertNotEmpty()
            {
                if(Empty)
                {
                    throw new EmptyStackException();
                }
            }

            private void assertValidIndex(int index)
            {
                if (index < 0 || index > numberOfElements - 1)
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
    }
}

