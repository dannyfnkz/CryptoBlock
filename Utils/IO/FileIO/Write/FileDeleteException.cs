using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO.Write
    {
        /// <summary>
        /// thrown if an error occurs while trying to delete file.
        /// </summary>
        public class FileDeleteException : FileWriteException
        {
            public FileDeleteException(string filePath, Exception innerException)
                : base(filePath, innerException, formatExceptionMessage())
            {

            }

            private static string formatExceptionMessage()
            {
                return "Could not delete file.";
            }
        }
    }
}
