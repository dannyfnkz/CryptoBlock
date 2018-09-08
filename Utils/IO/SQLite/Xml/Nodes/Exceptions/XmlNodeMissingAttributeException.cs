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
        /// thrown if a required <see cref="XmlAttribute"/> was missing from <see cref="XmlNode"/>.
        /// </summary>
        public class XmlNodeMissingAttributeException : XmlNodeException
        {
            private readonly string attributeName;

            public XmlNodeMissingAttributeException(string attributeName)
                : base(formatExceptionMessage(attributeName))
            {
                this.attributeName = attributeName;
            }

            public string AttributeName
            {
                get { return attributeName; }
            }

            private static string formatExceptionMessage(string attributeName)
            {
                return string.Format(
                    "Required attribute '{0}' was missing from XmlNode.",
                    attributeName);
            }
        }
    }
}