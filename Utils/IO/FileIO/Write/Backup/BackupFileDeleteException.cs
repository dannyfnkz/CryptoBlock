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
        /// thrown if backup file failed to be deleted during file write with backup operation.
        /// </summary>
        public class BackupFileDeleteException : BackupFileWriteException
        {
            private string backupFilePath;

            public BackupFileDeleteException(
                string filePath,
                string backupFilePath,
                Exception innerException)
                : base(filePath, innerException, formatExceptionMessage(filePath, backupFilePath))
            {
                this.backupFilePath = backupFilePath;
            }

            /// <summary>
            /// path of backup file which failed to be deleted.
            /// </summary>
            public string BackupFilePath
            {
                get { return backupFilePath; }
            }

            private static new string formatExceptionMessage(string filePath, string backupFilePath)
            {
                return string.Format(
                    "Deleting backup file at location '{0}' failed.{1}"
                    + "{2} contains the new requested content.",
                    backupFilePath,
                    Environment.NewLine,
                    filePath);
            }
        }
    }
}
