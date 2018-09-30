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
        /// contains <see cref="Enumerable"/> extension methods.
        /// </summary>
        public static class EnumerableExtensionMethods
        {
            /// <summary>
            /// thrown if an element satisfying the specified <see cref="Predicate{T}"/>
            /// does not exist in specified <see cref="IEnumerable{T}"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ElementSatisfyingPredicateNotFoundException<T> : Exception
            {
                private readonly Predicate<T> predicate;
                private readonly IEnumerable<T> enumerable;

                public ElementSatisfyingPredicateNotFoundException(
                    Predicate<T> predicate,
                    IEnumerable<T> enumerable)
                {
                    this.predicate = predicate;
                    this.enumerable = enumerable;
                }

                public Predicate<T> Predicate
                {
                    get { return predicate; }
                }

                public IEnumerable<T> Enumerable
                {
                    get { return enumerable; }
                }

                private static string formatExceptionMessage()
                {
                    return "An Element satisfying the specified predicate does not exist in" +
                        " specified collection.";
                }
            }

            /// <summary>
            /// <para>
            /// returns the first element in <paramref name="enumerable"/> which satisfies
            /// the specified <paramref name="predicate"/>.
            /// </para>
            /// <para>
            /// if no such element exists, returns null if <typeparamref name="T"/> is a reference type,
            /// or throws <see cref="ElementSatisfyingPredicateNotFoundException{T}"/> if
            /// <typeparamref name="T"/> is a value type.
            /// </para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="enumerable"></param>
            /// <param name="predicate"></param>
            /// <returns>
            /// first element in <paramref name="enumerable"/> which satisfies
            /// the specified <paramref name="predicate"/>,
            /// or null if no such element exists and <typeparamref name="T"/> is a value type
            /// </returns>
            /// <exception cref="ElementSatisfyingPredicateNotFoundException{T}">
            /// thrown if no element in <paramref name="enumerable"/> satisfies the specified
            /// <paramref name="predicate"/>, and <typeparamref name="T"/> is a value type.
            /// </exception>
            public static T FirstElementWhichSatisfies<T>(
                this IEnumerable<T> enumerable,
                Predicate<T> predicate)
            {
                T predicateSatisfyingElement = default(T);
                bool elementFound = false;

                foreach (T t in enumerable)
                {
                    if (predicate(t))
                    {
                        elementFound = true;
                        predicateSatisfyingElement = t;
                        break;
                    }
                }

                // if no element satisfying predicate was found, and T is a value type,
                // throw exception
                if(!elementFound && typeof(T).IsValueType)
                {
                    throw new ElementSatisfyingPredicateNotFoundException<T>(predicate, enumerable);
                }

                // return element satisfying predicate, if found, or null otherwise
                return predicateSatisfyingElement;
            }

            /// <summary>
            /// returns whether at least one element in specified <paramref name="enumerable"/> satisfies
            /// the specified <paramref name="predicate"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="enumerable"></param>
            /// <param name="predicate"></param>
            /// <returns>
            /// true if at least one element in specified <paramref name="enumerable"/> satisfies
            /// the specified <paramref name="predicate"/>,
            /// else false
            /// </returns>
            public static bool TrueForAny<T>(
                this IEnumerable<T> enumerable,
                Predicate<T> predicate)
            {
                bool trueForAnyElement = false;

                foreach(T t in enumerable)
                {
                    if(predicate(t))
                    {
                        trueForAnyElement = true;
                        break;
                    }
                }

                return trueForAnyElement;
            }
        }
    }
}
