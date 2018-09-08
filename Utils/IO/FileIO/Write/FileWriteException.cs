using System;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO.Write
    {
        /// <summary>
        /// thrown if an exception occurs while trying to perform a file write operation.
        /// </summary>
        public class FileWriteException : Exception
        {
            private string filePath;

            public FileWriteException(
                string filePath,
                Exception innerException = null,
                string additionalDetails = null
                )
                : base(formatExceptionMessage(filePath, additionalDetails), innerException)
            {
                this.filePath = filePath;
            }

            /// <summary>
            /// path of file the write operation is performed on.
            /// </summary>
            public string FilePath
            {
                get { return filePath; }
            }

            private static string formatExceptionMessage(string filePath, string additionalDetails = null)
            {
                string lineAppendage = additionalDetails == null ?
                    "." :
                    ": " + additionalDetails;

                string message = string.Format(
                    "An exception occurred while trying to perform a write operation on file at" +
                    " location '{0}'{1}",
                    filePath,
                    lineAppendage);

                return message;
            }
        }
    }
}
