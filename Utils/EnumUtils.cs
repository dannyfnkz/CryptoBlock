using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public static class EnumUtils
        {
            public static T ParseEnum<T>(string value)
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
        }
    }
}