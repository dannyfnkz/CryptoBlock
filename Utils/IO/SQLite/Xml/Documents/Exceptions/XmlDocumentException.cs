using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml.Documents.Exceptions
    {
        /// <summary>
        /// thrown when an exception occurs while handling an <see cref="XmlDocument"/>.
        /// </summary>
        public class XmlDocumentException : Exception
        {
            public XmlDocumentException(
                string exceptionMessageArguemnt = null,
                Exception innerException = null)
                : base(formatExceptionMessage(exceptionMessageArguemnt), innerException)
            {

            }

            private static string formatExceptionMessage(string exceptionMessageArguemnt)
            {
                const string defaultExceptionMessage = "An exception occurred while trying to handle" +
                    " XML document.";

                return exceptionMessageArguemnt != null
                    ? exceptionMessageArguemnt
                    : defaultExceptionMessage;
            }
        }
    }
}