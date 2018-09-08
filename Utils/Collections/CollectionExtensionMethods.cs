using System;
using System.Collections;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        /// <summary>
        /// contains extension methods for <see cref="ICollection"/>.
        /// </summary>
        public static class CollectionExtensionMethods
        {
            /// <summary>
            /// returns result of converting <paramref name="collection"/> into an
            /// <see cref="Object"/> array.
            /// </summary>
            /// <remarks>
            /// note <see cref="ICollection"/> does not specify an order, therefore
            /// order of elements in returned array is also unspecified.
            /// </remarks>
            /// <param name="collection"></param>
            /// <returns>
            /// <see cref="Object"/> array containing all elements from <paramref name="collection"/>
            /// </returns>
            public static object[] ToArray(this ICollection collection)
            {
                object[] resultArray = new object[collection.Count];

                collection.CopyTo(resultArray, 0);

                return resultArray;
            }
        }
    }
}
