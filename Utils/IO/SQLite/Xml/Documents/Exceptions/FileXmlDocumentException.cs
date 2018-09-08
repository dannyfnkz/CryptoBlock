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
        /// thrown when an exception occurs while handling an <see cref="FileXmlDocument"/>.
        /// </summary>
        public class FileXmlDocumentException : Exception
        {
            private readonly string filePath;

            public FileXmlDocumentException(
                string filePath,
                string additionalDetails = null,
                Exception innerException = null)
                : base(formatExceptionMessage(filePath, additionalDetails), innerException)
            {
                this.filePath = filePath;
            }

            public string FilePath
            {
                get { return filePath; }
            }

            private static string formatExceptionMessage(
                string filePath,
                string additionalDetails = null)
            {
                string messageSuffix = additionalDetails == null
                    ? "."
                    : string.Format(": {0}.", additionalDetails);

                return string.Format(
                    "An exception occurred while trying to handle XML document parsed from file" +
                    " at location '{0}'{1}",
                    filePath,
                    messageSuffix);
            }
        }
    }
}
