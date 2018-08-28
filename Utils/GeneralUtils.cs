using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public static class GeneralUtils
        {
            public static T GetValueOrCallIfNull<T>(T value, Func<T> func) where T : class
            {
                T result = value ?? func();
                return result;
            }
        }
    }
}

