using CryptoBlock.Utils.IO.FileIO;
using System.IO;
using System.Xml;
using static CryptoBlock.Utils.IO.FileIO.FileWriteException;

namespace CryptoBlock
{
    namespace IOManagement
    {
        /// <summary>
        /// handles file I/O.
        /// </summary>
        public class FileIOManager
        {   
            // file extensions
            private const string TEMP_FILE_EXTENSION = ".temp";

            private static FileIOManager instance = new FileIOManager();

            // unique session id for temporary write files
            private static int tempWriteFileId;

            private FileIOManager()
            {

            }

            public static FileIOManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// returns a unique session id for a temporary write file.
            /// </summary>
            private static int TempWriteFileId
            {
                get { return tempWriteFileId++; }
            }


            public void AppendTextToFile(string filePath, string text)
            {
                FileIOUtils.AppendTextToFile(filePath, text);
            }


            public bool FileExists(string filePath)
            {
                return File.Exists(filePath);
            }

            public string ReadTextFromFile(string filePath)
            {
                return FileIOUtils.ReadTextFromFile(filePath);
            }

            public void DeleteFile(string filePath)
            {
                FileIOUtils.DeleteFile(filePath);
            }

            /// <summary>
            /// synchronously writes <paramref name="content"/> to file at location <paramref name="filePath"/>.
            /// if file at <paramref name="filePath"/> does not exist,
            /// creates a new file containing <paramref name="content"/>.
            /// if file exists, overrwrites existing file safely.
            /// </summary>
            /// <seealso cref="FileIOUtils.WriteTextToFile(string, string, string)"/>
            /// <param name="filePathWithoutExtension"></param>
            /// <param name="fileExtension"></param>
            /// <param name="content"></param>
            /// <exception cref="BackupFileCreateException">
            /// <seealso cref="FileIOUtils.WriteTextToFile(string, string, string)"/>
            /// </exception>
            /// <exception cref="FileRenameException">
            /// <seealso cref="FileIOUtils.WriteTextToFile(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileDeleteException">
            /// <seealso cref="FileIOUtils.WriteTextToFile(string, string, string)"/>
            /// </exception>
            public void WriteTextToFile(
                string filePathWithoutExtension,
                string fileExtension,
                string content)
            {
                string filePath = filePathWithoutExtension + fileExtension;
                string backupFilePath = getBackupFilePath(filePathWithoutExtension);

                try
                {
                    FileIOUtils.WriteTextToFile(filePath, content, backupFilePath);
                }
                catch(FileWriteException fileWriteException)
                {
                    // backup file could not be renamed to requested file path
                    if(fileWriteException is FileRenameException)
                    {
                        FileRenameException fileRenameException = fileWriteException as FileRenameException;

                        try
                        {
                            // try renaming backup file to to requested file path
                            FileIOUtils.RenameFile(fileRenameException.BackupFilePath, filePath);
                        }
                        catch(FileWriteException) // renaming failed again
                        {
                            throw fileWriteException; // throw original exception
                        }
                    }
                    else
                    {
                        throw fileWriteException; // throw original exception
                    }
                }
            }

            /// <summary>
            /// returns file path for back file at <paramref name="filePathWithoutExtension"/>.
            /// </summary>
            /// <param name="filePathWithoutExtension"></param>
            /// <returns>
            /// file path for back file at <paramref name="filePathWithoutExtension"/>
            /// </returns>
            private string getBackupFilePath(string filePathWithoutExtension)
            {
                return filePathWithoutExtension + TempWriteFileId + TEMP_FILE_EXTENSION;
            }
        }
    }
}