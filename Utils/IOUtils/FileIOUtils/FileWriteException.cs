﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IOUtils.FileIOUtils
    {
        /// <summary>
        /// thrown if an exception occurs while trying to perform a file write operation.
        /// </summary>
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

            public FileWriteException(
                string filePath,
                Exception innerException)
                : base(formatExceptionMessage(filePath), innerException)
            {

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
                    "An exception occurred while trying to write to file: '{0}'{1}",
                    filePath,
                    lineAppendage);

                return message;
            }

            /// <summary>
            /// thrown if backup file failed to be created during file write operation.
            /// </summary>
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

            /// <summary>
            /// thrown if a required file rename operation failed during file write operation.
            /// </summary>
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

            /// <summary>
            /// thrown if backup file failed to be deleted during file write operation.
            /// </summary>
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
}
