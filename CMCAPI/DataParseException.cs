using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// thrown if an error occurres while trying to parse data.
        /// </summary>
        public class DataParseException : ArgumentNullException
        {
            public DataParseException(string propertyName) : base(propertyName)
            {

            }
        }
    }
}