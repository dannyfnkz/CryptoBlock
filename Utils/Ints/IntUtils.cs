using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Ints
    {
        public static class IntUtils
        {
            public static int GetNonNegative(int i)
            {
                if(i < 0)
                {
                    i *= -1;
                }

                return i;
            }
        }
    }

}
