using CryptoBlock.Utils.IO.FileIO;
using System.IO;
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
            private const string DATA_FILE_EXTENSION = ".json";
            private const string ERROR_LOG_FILE_EXTENSION = ".txt";
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

            /// <summary>
            /// synchronously writes <paramref name="content"/> to data file named <paramref name="dataFileName"/>. 
            /// if file does not exist, creates a new file containing <paramref name="content"/>.
            /// if file exists, overrwrites existing file safely.
            /// </summary>
            /// <seealso cref="writeTextToFile(string, string, string)"/>
            /// <param name="dataFileName"></param>
            /// <param name="content"></param>
            /// <exception cref="BackupFileCreateException">
            /// <seealso cref="writeTextToFile(string, string, string)"/>
            /// </exception>
            /// <exception cref="FileRenameException">
            /// <seealso cref="writeTextToFile(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileDeleteException">
            /// <seealso cref="writeTextToFile(string, string, string)"/>
            /// </exception>
            public void WriteTextToDataFile(string dataFileName, string content)
            {
                string filePathWithoutExtension = getDataFilePathWithoutExtension(dataFileName);
                writeTextToFile(filePathWithoutExtension, DATA_FILE_EXTENSION, content);
            }

            /// <summary>
            /// synchronously appends <paramref name="text"/> to error log file named
            /// <paramref name="errorLogFileName"/>.
            /// </summary>
            /// <seealso cref="getErrorLogFilePath(string)"/>
            /// <seealso cref="FileIOUtils.AppendTextToFile(string, string)"/>
            /// <param name="errorLogFileName"></param>
            /// <param name="text"></param>
            /// <exception cref="FileAppendException">
            /// <seealso cref="FileIOUtils.AppendTextToFile(string, string)"/>
            /// </exception>
            public void AppendTextToErrorLogFile(string errorLogFileName, string text)
            {
                string filePath = getErrorLogFilePath(errorLogFileName);
                FileIOUtils.AppendTextToFile(filePath, text);
            }

            /// <summary>
            /// returns whether data file named <paramref name="dataFileName"/> exists.
            /// </summary>
            /// <seealso cref="getDataFilePath(string)"/>
            /// <seealso cref="File.Exists(string)"/>
            /// <param name="dataFileName"></param>
            /// <returns>
            /// true if data file with <paramref name="dataFileName"/> exists,
            /// else false
            /// </returns>
            public bool DataFileExists(string dataFileName)
            {
                string filePath = getDataFilePath(dataFileName);

                return File.Exists(filePath);
            }

            /// <summary>
            /// returns whether error log file named <paramref name="errorLogFileName"/> exists.
            /// </summary>
            /// <see cref="getErrorLogFilePath(string)"/>
            /// <seealso cref="File.Exists(string)"/>
            /// <param name="errorLogFileName"></param>
            /// <returns>
            /// true if error log file with <paramref name="errorLogFileName"/> exists,
            /// else false.
            /// </returns>
            public bool ErrorLogFileExists(string errorLogFileName)
            {
                string filePath = getErrorLogFilePath(errorLogFileName);

                return File.Exists(filePath);
            }

            /// <summary>
            /// returns textual content of data file at location <paramref name="dataFileName"/>.
            /// </summary>
            /// <seealso cref="FileIOUtils.ReadTextFromFile(string)"/>
            /// <param name="dataFileName"></param>
            /// <returns>
            /// textual content of data file at location <paramref name="dataFileName"/>.
            /// </returns>
            /// <exception cref="FileReadException">
            /// <seealso cref="FileIOUtils.ReadTextFromFile(string)"/>
            /// </exception>
            public string ReadTextFromDataFile(string dataFileName)
            {
                string filePath = getDataFilePath(dataFileName);

                return FileIOUtils.ReadTextFromFile(filePath);
            }

            /// <summary>
            /// returns file path for <paramref name="dataFileName"/>.
            /// </summary>
            /// <param name="dataFileName"></param>
            /// <returns>
            /// file path for <paramref name="dataFileName"/>
            /// </returns>
            private string getDataFilePath(string dataFileName)
            {
                return getDataFilePathWithoutExtension(dataFileName) + DATA_FILE_EXTENSION;
            }

            /// <summary>
            /// returns file path, without extension, for <paramref name="dataFileName"/>.
            /// </summary>
            /// <param name="dataFileName"></param>
            /// <returns>
            /// file path, without extension, for <paramref name="dataFileName"/>
            /// </returns>
            private string getDataFilePathWithoutExtension(string dataFileName)
            {
                return dataFileName;
            }

            /// <summary>
            /// returns file path for <paramref name="errorLogFileName"/>.
            /// </summary>
            /// <param name="errorLogFileName"></param>
            /// <returns>
            /// file path for <paramref name="errorLogFileName"/>
            /// </returns>
            private string getErrorLogFilePath(string errorLogFileName)
            {
                return errorLogFileName + ERROR_LOG_FILE_EXTENSION;
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
            private void writeTextToFile(string filePathWithoutExtension, string fileExtension, string content)
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