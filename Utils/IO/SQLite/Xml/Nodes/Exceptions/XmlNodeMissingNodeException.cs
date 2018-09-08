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
        /// thrown if a required child <see cref="XmlNode"/> was missing from <see cref="XmlNode"/>.
        /// </summary>
        public class XmlNodeMissingNodeException : XmlNodeException
        {
            private readonly string xPath;

            public XmlNodeMissingNodeException(string xPath)
                : base(formatExceptionMessage(xPath))
            {
                this.xPath = xPath;
            }

            public string XPath
            {
                get { return xPath; }
            }

            private static string formatExceptionMessage(string xPath)
            {
                return string.Format(
                    "Required node '{0}' was missing from XmlNode.",
                    xPath);
            }
        }
    }
}