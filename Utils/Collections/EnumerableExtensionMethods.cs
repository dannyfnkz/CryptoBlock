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
            public static void ConvertEachElement<T, K>(
                this IList<T> list,
                Converter<T, K> converter) where K : T
            {
                for(int i = 0; i < list.Count; i++)
                {
                    list[i] = converter(list[i]);
                }
            }
        }
    }
}