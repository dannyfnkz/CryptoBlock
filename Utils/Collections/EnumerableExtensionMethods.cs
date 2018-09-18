using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        public static class EnumerableExtensionMethods
        {
            public class ElementSatisfyingPredicateNotFoundException<T> : Exception
            {
                private readonly Predicate<T> predicate;

                public ElementSatisfyingPredicateNotFoundException(Predicate<T> predicate)
                {
                    this.predicate = predicate;
                }

                public Predicate<T> Predicate
                {
                    get { return predicate; }
                }

                private static string formatExceptionMessage()
                {
                    return "An Element satisfying the specified predicate does not exist in" +
                        " specified collection.";
                }
            }

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
                    throw new ElementSatisfyingPredicateNotFoundException<T>(predicate);
                }

                // return element satisfying predicate, if found, or null otherwise
                return predicateSatisfyingElement;
            }

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
