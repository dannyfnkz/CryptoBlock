using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml.Documents.Exceptions
    {
        /// <summary>
        /// thrown if <see cref="FileXmlDocument"/> initialization failed.
        /// </summary>
        public class FileXmlDocumentInitializationException : FileXmlDocumentException
        {
            public FileXmlDocumentInitializationException(string filePath, Exception innerException = null)
                : base(filePath, formatExceptionMessage(), innerException)
            {

            }

            private static string formatExceptionMessage()
            {
                return "FileXmlDocument initialization failed.";
            }
        }
    }
}

