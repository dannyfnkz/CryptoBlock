using CryptoBlock.Utils.IO.FileIO.Write;
using CryptoBlock.Utils.IO.FileIO.Write.Backup;
using System;
using System.IO;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO
    {
        // note: still need exception handling for actions other than write text
        public static class FileIOUtils
        {
            private const string TEMP_FILE_EXTENSION = ".temp";

            /// <summary>
            /// <para>
            /// synchronously writes <paramref name="content"/> to file at location
            /// <paramref name="filePath"/>.
            /// if file at <paramref name="filePath"/> does not exist,
            /// creates a new file containing <paramref name="content"/>.
            /// </para>
            /// <para>
            /// if file with specified filePath already exists,
            /// performs a safe overwrite under the condition that <paramref name="backupFilePath"/> is not null. 
            /// </para>
            /// </summary>
            /// <remarks>
            /// if <paramref name="backupFilePath"/> != null, one of the following is guaranteed:
            /// 1. file at <paramref name="filePath"/> maintains its original content;
            /// 2. file at <paramref name="filePath"/> contains <paramref name="content"/>;
            /// 3. a generated backup file at location <paramref name="backupFilePath"/>
            /// contains <paramref name="content"/>.
            /// </remarks>
            /// <param name="filePath">location of file to write text to</param>
            /// <param name="content">content to be written to <paramref name="filePath"/></param>
            /// <param name="backupFilePath">location of generated backup file</param>
            /// <exception cref="BackupFileCreateException">
            /// thrown if an exception occurred while trying to write <paramref name="content"/> to backup file.
            /// </exception>
            /// <exception cref="BackupFileRenameException">
            /// thrown if an exception occurred while trying to rename backup file
            /// (containing <paramref name="content"/>)
            /// or file at <paramref name="filePath"/>, containing old content.
            /// </exception>
            /// <exception cref="BackupFileDeleteException">
            /// thrown if an exception occurred while trying to delete backup file.
            /// </exception>
            public static void WriteTextToFileWithBackup(
                string filePath,
                string content,
                string backupFilePath)
            {
                if (FileExists(filePath))
                {
                    // safely replace requested file content by first writing new content to a new temp file,
                    // then deleting old file, and finally renaming new temp file path to requested file path
                    try
                    {
                        // write new file content to backup file
                        WriteTextToFile(backupFilePath, content);
                    }
                    catch (Exception exception)
                    {
                        throw new BackupFileCreateException(filePath, exception);
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
                    catch (Exception exception)
                    {
                        throw new BackupFileRenameException(filePath, backupFilePath, exception);
                    }

                    try
                    {
                        // delete old file
                        DeleteFile(tempOldFilePath);
                    }
                    catch (Exception exception)
                    {
                        throw new BackupFileDeleteException(filePath, tempOldFilePath, exception);
                    }
                }
                else // !FileExists(filePath)
                {
                    // create file with specified content
                    WriteTextToFile(filePath, content);
                }
            }

            /// <summary>
            /// synchronously writes <paramref name="content"/> to file at location
            /// <paramref name="filePath"/>.
            /// if file at <paramref name="filePath"/> does not exist,
            /// creates a new file containing <paramref name="content"/>.
            /// </summary>
            /// <seealso cref="File.WriteAllText(string, string)"/>
            /// <param name="filePath"></param>
            /// <param name="content"></param>
            /// <exception cref="FileWriteException">
            /// thrown if an exception occurs while trying to write text to file
            /// <seealso cref="File.WriteAllText(string, string)"/>
            /// </exception>
            public static void WriteTextToFile(string filePath, string content)
            {
                try
                {
                    File.WriteAllText(filePath, content);
                }
                catch (Exception exception)
                {
                    throw new FileWriteException(filePath, exception);
                }
            }

            /// <summary>
            /// returns whether file with specified <paramref name="filePath"/> exists.
            /// </summary>
            /// <param name="filePath"></param>
            /// <returns>
            /// true if file with specified <paramref name="filePath"/> exists,
            /// else false
            /// </returns>
            public static bool FileExists(string filePath)
            {
                return File.Exists(filePath);
            }

            /// <summary>
            /// synchronously reads text from file located at <paramref name="filePath"/>.
            /// </summary>
            /// <param name="filePath">location of file to read text from</param>
            /// <returns>
            /// text read from file located at <paramref name="filePath"/>
            /// </returns>
            /// <exception cref="FileReadException">
            /// thrown if an exception occurred while trying to read text from file.
            /// </exception>
            public static string ReadTextFromFile(string filePath)
            {
                try
                {
                    string text = File.ReadAllText(filePath);

                    return text;
                }
                catch (Exception exception)
                {
                    throw new FileReadException(filePath, exception, null);
                }
            }

            /// <summary>
            /// synchronously appends <paramref name="text"/> to end of file at location <paramref name="filePath"/>.
            /// </summary>
            /// <seealso cref="File.AppendAllText(string, string)"/>
            /// <param name="filePath">location of file to append text to</param>
            /// <param name="text"></param>
            /// <exception cref="FileAppendException">
            /// thrown if an exception occurred while trying to append to file
            /// </exception>
            public static void AppendTextToFile(string filePath, string text)
            {
                try
                {
                    File.AppendAllText(filePath, text);
                }
                catch(Exception exception)
                {
                    throw new FileAppendException(filePath, exception);
                }
            }

            /// <summary>
            /// deletes file at <paramref name="filePath"/>, if exists.
            /// </summary>
            /// <seealso cref="File.Delete(string)"/>
            /// <param name="filePath">location of file to delete</param>
            /// <exception cref="FileDeleteException">
            /// thrown if an exception occurs while trying to delete file
            /// <seealso cref="File.Delete(string)"/>
            /// </exception>
            public static void DeleteFile(string filePath)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch(Exception exception)
                {
                    throw new FileDeleteException(filePath, exception);
                }                
            }

            /// <summary>
            /// renames file at location <paramref name="oldFilePathWithoutExtension"/>
            /// with extension <paramref name="oldFileExtension"/> to <paramref name="newFilePathWithoutExtension"/>
            /// with extension <paramref name="newFileExtension"/>.
            /// </summary>
            /// <seealso cref="RenameFile(string, string)"/>
            /// <param name="oldFilePathWithoutExtension"></param>
            /// <param name="oldFileExtension"></param>
            /// <param name="newFilePathWithoutExtension"></param>
            /// <param name="newFileExtension"></param>
            /// <exception cref="FileRenameException">
            /// <seealso cref="RenameFile(string, string)"/>
            /// </exception>
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
            /// <summary>
            /// renames file at location <paramref name="oldFilePath"/> to <paramref name="newFilePath"/>.
            /// </summary>
            /// <remarks>
            /// automic in both windows (NTFS) and linux: either the operation fails and old file path is preserved,
            /// or it succeeds and <paramref name="oldFilePath"/> is changed to <paramref name="newFilePath"/>.
            /// </remarks>
            /// <param name="oldFilePath"></param>
            /// <param name="newFilePath"></param>
            /// <exception cref="FileRenameException">
            /// thrown if an exception occurs while trying to rename file
            /// <seealso cref="File.Move(string, string)"/>
            /// </exception> 
            public static void RenameFile(string oldFilePath, string newFilePath)
            {
                try
                {
                    File.Move(oldFilePath, newFilePath);
                }
                catch(Exception exception)
                {
                    throw new FileRenameException(oldFilePath, newFilePath, exception);
                }
            }
        }
    }
}