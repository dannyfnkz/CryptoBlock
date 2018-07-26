using System;
using System.IO;

namespace CryptoBlock
{
    namespace Utils.IOUtils.FileIOUtils
    {
        public static class FileIOUtils
        {
            private const string TEMP_FILE_EXTENSION = ".temp";

            // writes specified content to requested file path.
            // if file with specified filePath does not exist, creates a new file with requested content.
            // else, if file with specified filePath already exists,
            // performs a safe overwrite under the condition that backupFilePath != null:
            // at the end of a successful or unsuccessful execution, it is guaranteed that
            // either 1. file with specified filePath maintains its original content,
            // or 2. backup file contains the requested content.
            public static void WriteTextToFile(string filePath, string content, string backupFilePath = null)
            {
                if (File.Exists(filePath) && backupFilePath != null)
                {
                    // safely replace requested file content by first writing new content to a new temp file,
                    // then deleting old file, and finally renaming new temp file path to requested file path
                    try
                    {
                        // write new file content to backup file
                        WriteTextToFile(backupFilePath, content, null);
                    }
                    catch(Exception exception)
                    {
                        throw new FileWriteException.BackupFileCreateException(filePath, exception);
                    }

                    string tempOldFilePath = filePath + TEMP_FILE_EXTENSION;

                    try
                    {
                        // rename old file into a temp file
                        // note renameFile is automic in both windows (NTFS) and linux                      
                        RenameFile(filePath, tempOldFilePath);

                        // rename backup file path to requested file path
                        // note renameFile is automic in both windows (NTFS) and linux
                        RenameFile(backupFilePath, filePath);
                    }
                    catch(Exception exception)
                    {
                        throw new FileWriteException.FileRenameException(filePath, backupFilePath, exception);
                    }

                    try
                    {
                        // delete old file
                        DeleteFile(tempOldFilePath);
                    }
                    catch(Exception exception)
                    {
                        throw new FileWriteException.BackupFileDeleteException(filePath, tempOldFilePath, exception);
                    }
                }
                else // file does not exist: create a new file
                {
                    try
                    {
                        File.WriteAllText(filePath, content);
                    }
                    catch(Exception exception)
                    {
                        throw new FileWriteException(filePath, exception);
                    }
                }
            }

            public static string ReadTextFromFile(string filePath)
            {
                string text = File.ReadAllText(filePath);
                return text;
            }

            public static void AppendTextToFile(string filePath, string text)
            {
                File.AppendAllText(filePath, text);
            }

            public static void DeleteFile(string filePath)
            {
                File.Delete(filePath);
            }

            public static void RenameFile(
                string oldFilePathWithoutExtension,
                string oldFileExtension,
                string newFilePathWithoutExtension,
                string newFileExtension)
            {
                string oldFilePath = oldFilePathWithoutExtension + oldFileExtension;
                string newFilePath = newFilePathWithoutExtension + newFileExtension;

                RenameFile(oldFilePath, newFilePath);
            }

            // automic in both windows (NTFS) and linux: either the operation fails and old file path is preserved,
            // or it succeeds and file path is changed to newFilePath
            public static void RenameFile(string oldFilePath, string newFilePath)
            {
                File.Move(oldFilePath, newFilePath);
            }
        }
    }
}