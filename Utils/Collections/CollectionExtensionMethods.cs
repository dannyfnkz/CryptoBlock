using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections
    {
        public static class CollectionExtensionMethods
        {
            public static object[] ToArray(this ICollection collection)
            {
                object[] array = new object[collection.Count];

                collection.CopyTo(array, 0);

                return array;
            }
        }
    }
}
