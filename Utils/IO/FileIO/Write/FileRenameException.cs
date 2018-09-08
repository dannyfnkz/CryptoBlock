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
        /// thrown if an error occurs while trying to rename file.
        /// </summary>
        public class FileRenameException : FileWriteException
        {
            public FileRenameException(string oldFilePath, string newFilePath, Exception innerException)
                : base(oldFilePath, innerException, formatExceptionMessage(newFilePath))
            {

            }

            private static string formatExceptionMessage(string newFilePath)
            {
                return string.Format("Could not rename file to '{0}'.", newFilePath);
            }
        }
    }
}
