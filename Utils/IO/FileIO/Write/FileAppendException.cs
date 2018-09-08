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
        /// thrown if an error occurs while trying to append text to file.
        /// </summary>
        public class FileAppendException : FileWriteException
        {
            public FileAppendException(string filePath, Exception innerException)
                : base(filePath, innerException, formatExceptionMessage())
            {

            }

            private static string formatExceptionMessage()
            {
                return "Could not append to file.";
            }
        }
    }
}
