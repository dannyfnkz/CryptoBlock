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
        /// thrown if a required file rename operation failed during file write with backup operation.
        /// </summary>
        public class BackupFileRenameException : BackupFileWriteException
        {
            private string backupFilePath;

            public BackupFileRenameException(
                string filePath,
                string backupFilePath,
                Exception innerException)
                : base(filePath, innerException, formatExceptionMessage(backupFilePath))
            {
                this.backupFilePath = backupFilePath;
            }

            /// <summary>
            /// path of backup file containing the new requested content.
            /// </summary>
            public string BackupFilePath
            {
                get { return backupFilePath; }
            }

            private static string formatExceptionMessage(string backupfilePath)
            {
                return string.Format(
                    "renaming file(s) failed.{0}"
                    + "'{1}' contains the new requested content.",
                    Environment.NewLine,
                    backupfilePath);
            }
        }
    }
}
