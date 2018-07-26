using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IOUtils.FileIOUtils
    {
        public class FileWriteException : Exception
        {
            private string filePath;

            public FileWriteException(
                string filePath,
                string additionalDetails,
                Exception innerException)
                : base(formatExceptionMessage(filePath, additionalDetails), innerException)
            {
                this.filePath = filePath;
            }

            public string FilePath
            {
                get { return filePath; }
            }

            public FileWriteException(
                string filePath,
                Exception innerException)
                : base(formatExceptionMessage(filePath), innerException)
            {

            }

            private static string formatExceptionMessage(string filePath, string additionalDetails = null)
            {
                string lineAppendage = additionalDetails == null ?
                    "." :
                    ": " + additionalDetails;

                string message = string.Format(
                    "An exception occurred while trying to write to file: '{0}'{1}",
                    filePath,
                    lineAppendage);

                return message;
            }

            public class BackupFileCreateException : FileWriteException
            {
                public BackupFileCreateException(
                    string filePath,
                    Exception innerException)
                    : base(filePath, formatExceptionMessage(filePath), innerException)
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

            public class FileRenameException : FileWriteException
            {
                private string backupFilePath;

                public FileRenameException(
                    string filePath,
                    string backupFilePath,
                    Exception innerException)
                    : base(filePath, formatExceptionMessage(backupFilePath), innerException)
                {
                    this.backupFilePath = backupFilePath;
                }

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

            public class BackupFileDeleteException : FileWriteException
            {
                private string backupFilePath;

                public BackupFileDeleteException(
                    string filePath,
                    string backupFilePath,
                    Exception innerException)
                    : base(filePath, formatExceptionMessage(filePath, backupFilePath), innerException)
                {

                }

                public string BackupFilePath
                {
                    get { return backupFilePath; }
                }

                private static string formatExceptionMessage(string filePath, string backupFilePath)
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
}
