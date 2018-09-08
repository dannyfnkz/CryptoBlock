using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO.Write.Backup
    {
        /// <summary>
        /// thrown if backup file failed to be created during file write with backup operation.
        /// </summary>
        public class BackupFileCreateException : BackupFileWriteException
        {
            public BackupFileCreateException(
                string filePath,
                Exception innerException)
                : base(filePath, innerException, formatExceptionMessage(filePath))
            {

            }

            private static string formatExceptionMessage(string filePath)
            {
                return string.Format(
                    "Creating backup file failed.{0}"
                    + "'{1}' maintains its original content.",
                    Environment.NewLine,
                    filePath);
            }
        }
    }
}
