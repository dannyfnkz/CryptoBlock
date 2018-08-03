using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO
    {
        /// <summary>
        /// thrown if an error occurs while trying to read text from file.
        /// </summary>
        public class FileReadException : Exception
        {
            public FileReadException(
                string filePath,
                Exception innerException,
                string additionalDetails = null)
                : base(formatExceptionMessage(filePath, additionalDetails), innerException)
            {

            }

            private static string formatExceptionMessage(string filePath, string additionalDetails = null)
            {
                string lineAppendage = additionalDetails == null ?
                    "." :
                    ": " + additionalDetails;

                string message = string.Format(
                    "An exception occurred while trying to read from file at location '{0}'{1}",
                    filePath,
                    lineAppendage);

                return message;
            }
        }
    }
}