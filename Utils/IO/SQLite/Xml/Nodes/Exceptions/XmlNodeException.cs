using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml.Nodes.Exceptions
    {
        /// <summary>
        /// thrown when an operation on an <see cref="XmlNode"/> fails.
        /// </summary>
        public class XmlNodeException : Exception
        {
            public XmlNodeException(
                string exceptionMessage = null,
                Exception innerException = null)
                : base(exceptionMessage, innerException)
            {
                
            }
        }
    }
}
