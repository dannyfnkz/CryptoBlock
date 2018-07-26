using CryptoBlock.Utils.IOUtils.FileIOUtils;
using System.IO;
using static CryptoBlock.Utils.IOUtils.FileIOUtils.FileWriteException;

namespace CryptoBlock
{
    namespace IOManagement
    {
        public class FileIOManager
        {         
            private const string DATA_FILE_EXTENSION = ".json";
            private const string ERROR_LOG_FILE_EXTENSION = ".txt";
            private const string TEMP_FILE_EXTENSION = ".temp";

            private static FileIOManager instance = new FileIOManager();

            private static int tempSaveFileId;

            private FileIOManager()
            {

            }

            public static FileIOManager Instance
            {
                get { return instance; }
            }

            private static int TempSaveFileId
            {
                get { return tempSaveFileId++; }
            }

            public void WriteTextToDataFile(string fileName, string content)
            {
                string filePathWithoutExtension = getDataFilePathWithoutExtension(fileName);
                writeTextToFile(filePathWithoutExtension, DATA_FILE_EXTENSION, content);
            }

            public void AppendTextToErrorLogFile(string fileName, string text)
            {
                string filePath = getErrorLogFilePath(fileName);
                FileIOUtils.AppendTextToFile(filePath, text);
            }

            public bool DataFileExists(string fileName)
            {
                string filePath = getDataFilePath(fileName);

                return File.Exists(filePath);
            }

            public bool ErrorLogFileExists(string fileName)
            {
                string filePath = getErrorLogFilePath(fileName);

                return File.Exists(filePath);
            }

            public string ReadTextFromDataFile(string fileName)
            {
                string filePath = getDataFilePath(fileName);

                return FileIOUtils.ReadTextFromFile(filePath);
            }

            private string getDataFilePath(string fileName)
            {
                return getDataFilePathWithoutExtension(fileName) + DATA_FILE_EXTENSION;
            }

            private string getDataFilePathWithoutExtension(string fileName)
            {
                return fileName;
            }

            private string getErrorLogFilePath(string fileName)
            {
                return fileName + ERROR_LOG_FILE_EXTENSION;
            }

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
                        throw fileWriteException;
                    }
                }
            }

            private string getBackupFilePath(string filePathWithoutExtension)
            {
                return filePathWithoutExtension + TempSaveFileId + TEMP_FILE_EXTENSION;
            }
        }
    }
}