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
        /// thrown if specified attribute in <see cref="XmlNode"/> had an invalid type.
        /// </summary>
        public class InvalidAttributeTypeException : XmlNodeException
        {
            private readonly string attributeName;
            private readonly Type type;
            
            public InvalidAttributeTypeException(
                string attributeName,
                Type type,
                Exception innerException = null)
                : base(formatExceptionMessage(attributeName, type), innerException)
            {
                this.attributeName = attributeName;

            }

            public string AttributeName
            {
                get { return attributeName; }
            }

            public Type Type
            {
                get { return type; }
            }

            private static string formatExceptionMessage(string attributeName, Type type)
            {
                return string.Format(
                    "Required attribute '{0}' in XmlNode had an invalid type (expected type: '{0}').",
                    attributeName,
                    type.FullName);
            }
        }
    }
}

