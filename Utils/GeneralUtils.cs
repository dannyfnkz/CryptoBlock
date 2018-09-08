using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains general purpose utility methods.
        /// </summary>
        public static class GeneralUtils
        {
            /// <summary>
            /// returns <paramref name="value"/> if it is not null, otherwise returns
            /// result of calling <paramref name="func"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <param name="func"></param>
            /// <returns>
            /// <paramref name="value"/> if <paramref name="value"/> is not null,
            /// else result of calling <paramref name="func"/>
            /// </returns>
            public static T GetValueOrCallIfNull<T>(T value, Func<T> func) where T : class
            {
                T result = value ?? func();
                return result;
            }
        }
    }
}

