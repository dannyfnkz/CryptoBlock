using System;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        /// <summary>
        /// represents a stack having a limited capacity which supports retrieving by index in constant time.
        /// once capacity is reached, each new item added causes the least recently added item to be discarded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// stack is implemented using a circular array: when end of array is reached, index starts again
        /// at 0 (overriding existing elements).
        /// </para>
        /// <para>
        /// time complexity:
        /// <see cref="ElementAt(int)"/>, <see cref="HasElementAt(int)"/> - O(1).
        /// for all other operations, same as regular stack.
        /// </para>
        /// <para>
        /// space complexity:
        /// O(<see cref="Capacity"/>).
        /// </para>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        public class IndexableStack<T>
        {
            /// <summary>
            /// thrown an operation was performed which requires a non-empty stack.
            /// </summary>
            public class EmptyStackException : Exception
            {
                public EmptyStackException()
                    : base(formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Stack was empty.";
                }
            }

            // max number of elements in stack
            private readonly int capacity;

            // circular array holding stack elements
            public readonly T[] array;

            // array index of last element in stack
            private int lastElementIndex;

            // number of elements in stack
            private int numberOfElements;

            public IndexableStack(int capacity)
            {
                this.capacity = capacity;

                this.array = new T[capacity];
                this.lastElementIndex = capacity - 1;              
            }

            /// <summary>
            /// max number of elements in stack.
            /// </summary>
            public int Capacity
            {
                get { return capacity; }
            }

            /// <summary>
            /// true if stack has no elements, else false
            /// </summary>
            public bool Empty
            {
                get { return Count == 0; }
            }

            /// <summary>
            /// number of elemnts in stack
            /// </summary>
            public int Count
            {
                get { return numberOfElements; } 
            }

            /// <summary>
            /// pushes a new element to the top of the stack.
            /// </summary>
            /// <remarks>
            /// <para>
            /// if stack if full (capacity has been reached), removes the least recently added element from stack.
            /// </para>
            /// <para>
            /// time complexity: O(1).
            /// </para>
            /// </remarks>
            /// <param name="element"></param>
            public void Push(T element)
            {
                int insertIndex;

                // if end of array is reached, set index to 0
                insertIndex = lastElementIndex + 1 < capacity ? lastElementIndex + 1 : 0;

                // insert element at correct index
                array[insertIndex] = element;

                // set last element index as insert index
                lastElementIndex = insertIndex;

                // increase element count if it's less than capacity
                if(numberOfElements < capacity)
                {
                    ++numberOfElements;
                }
            }

            /// <summary>
            /// if stack is not empty, removes the element at the top of the stack and returns it.
            /// </summary>
            /// <remarks>
            /// <para>
            /// time complexity: O(1).
            /// </para> 
            /// </remarks>
            /// <returns>
            /// element at top of the stack if stack is not empty
            /// </returns>
            /// <exception cref="EmptyStackException"><seealso cref="assertNotEmpty"/></exception>
            public T Pop()
            {
                // check stack is not empty
                assertNotEmpty();

                // get element at top of stack
                T popped = array[lastElementIndex];

                // decrement index by 1. if new index is less than zero, set index to end of the end of circular array.
                lastElementIndex = lastElementIndex == 0 ? capacity - 1 : lastElementIndex - 1;

                // decrement stack count
                --numberOfElements;

                return popped;
            }

            /// <summary>
            /// if stack is not empty, returns the element at the top of the stack.
            /// </summary>
            /// <returns>
            /// if stack is not empty, element at the top of the stack
            /// </returns>
            /// <exception cref="EmptyStackException"><seealso cref="ElementAt(int)"/></exception>
            /// <exception cref="IndexOutOfRangeException"><seealso cref="ElementAt(int)"/></exception>
            public T TopElement()
            {
                return ElementAt(0);
            }

            /// <summary>
            /// if element at index <paramref name="indexFromTopOfStack"/> from the top of stack exists,
            /// returns element at said index.
            /// </summary>
            /// <param name="indexFromTop">index of element, counting from top of stack.</param>
            /// <returns>
            /// element at index <paramref name="indexFromTopOfStack"/> from the top of stack
            /// </returns>
            /// <exception cref="EmptyStackException"><seealso cref="assertNotEmpty"/></exception>
            /// <exception cref="IndexOutOfRangeException"><seealso cref="assertValidIndex(int)"/></exception>
            public T ElementAt(int indexFromTopOfStack)
            {
                assertNotEmpty();
                assertValidIndex(indexFromTopOfStack);

                // calculate index of requested element in circular array
                int elementArrayIndex = ((lastElementIndex - indexFromTopOfStack) + capacity) % capacity;

                return array[elementArrayIndex];
            }

            /// <summary>
            /// returns whether stack has element at index <paramref name="indexFromTopOfStack"/> 
            /// from top of stack.
            /// </summary>
            /// <param name="indexFromTopOfStack">index of element, counting from top of stack.</param>
            /// <returns>
            /// true if element at index <paramref name="indexFromTopOfStack"/> from top of stack exists,
            /// else false
            /// </returns>
            public bool HasElementAt(int indexFromTopOfStack)
            {
                return indexFromTopOfStack >= 0 && indexFromTopOfStack <= numberOfElements - 1;
            }

            /// <summary>
            /// asserts stack is not empty.
            /// </summary>
            /// <exception cref="EmptyStackException"></exception>
            private void assertNotEmpty()
            {
                if(Empty)
                {
                    throw new EmptyStackException();
                }
            }

            /// <summary>
            /// asserts <paramref name="index"/> is valid.
            /// </summary>
            /// <param name="index"></param>
            /// <exception cref="System.IndexOutOfRangeException"></exception>
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

