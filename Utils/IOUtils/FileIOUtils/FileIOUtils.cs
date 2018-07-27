﻿using System;
using System.IO;

namespace CryptoBlock
{
    namespace Utils.IOUtils.FileIOUtils
    {
        // note: still need exception handling for actions other than write text
        public static class FileIOUtils
        {
            private const string TEMP_FILE_EXTENSION = ".temp";

            /// <summary>
            /// synchronously writes <paramref name="content"/> to file at location <paramref name="filePath"/>.
            /// if file at <paramref name="filePath"/> does not exist,
            /// creates a new file containing <paramref name="content"/>.
            /// else, if file with specified filePath already exists,
            /// performs a safe overwrite under the condition that <paramref name="backupFilePath"/> is not null. 
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

            /// <summary>
            /// synchronously reads text from file located at <paramref name="filePath"/>.
            /// </summary>
            /// <param name="filePath">location of file to read text from</param>
            /// <returns>
            /// text read from file located at <paramref name="filePath"/>
            /// </returns>
            public static string ReadTextFromFile(string filePath)
            {
                string text = File.ReadAllText(filePath);

                return text;
            }

            /// <summary>
            /// synchronously appends <paramref name="text"/> to end of file at location <paramref name="filePath"/>.
            /// </summary>
            /// <param name="filePath">location of file to append text to</param>
            /// <param name="text"></param>
            public static void AppendTextToFile(string filePath, string text)
            {
                File.AppendAllText(filePath, text);
            }

            /// <summary>
            /// deletes file at <paramref name="filePath"/>, if exists.
            /// </summary>
            /// <param name="filePath">location of file to delete</param>
            public static void DeleteFile(string filePath)
            {
                File.Delete(filePath);
            }

            /// <summary>
            /// renames file at location <paramref name="oldFilePathWithoutExtension"/>
            /// with extension <paramref name="oldFileExtension"/> to <paramref name="newFilePathWithoutExtension"/>
            /// with extension <paramref name="newFileExtension"/>.
            /// </summary>
            /// <param name="oldFilePathWithoutExtension"></param>
            /// <param name="oldFileExtension"></param>
            /// <param name="newFilePathWithoutExtension"></param>
            /// <param name="newFileExtension"></param>
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
            public static void RenameFile(string oldFilePath, string newFilePath)
            {
                File.Move(oldFilePath, newFilePath);
            }
        }
    }
}