using CryptoBlock.ConfigurationManagement.Settings;
using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.SettingsManagement.SavedCommands;
using CryptoBlock.Utils;
using CryptoBlock.Utils.IO.FileIO;
using CryptoBlock.Utils.IO.FileIO.Write;
using CryptoBlock.Utils.IO.FileIO.Write.Backup;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.ConfigurationManagement.Settings.SettingsContainer;

namespace CryptoBlock
{
    namespace ConfigurationManagement
    {
        /// <summary>
        /// manages program configuration.
        /// </summary>
        public class ConfigurationManager
        {
            /// <summary>
            /// thrown if an exception occurs while performing an operation on
            /// <see cref="ConfigurationManager"/>.
            /// </summary>
            public abstract class ConfigurationManagerException : Exception
            {
                protected ConfigurationManagerException(
                    string exceptionMessage = null,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {
                    
                }
            }

            /// <summary>
            /// thrown if an operation which requires that <see cref="ConfigurationManager"/> be
            /// initialized is called before <see cref="ConfigurationManager"/> was initialized.
            /// </summary>
            public class ConifgurationManagerNotInitializedException : ConfigurationManagerException
            {
                private readonly string operationName;

                public ConifgurationManagerNotInitializedException(
                    string operationName)
                    : base(formatExceptionMessage(operationName))
                {
                    this.operationName = operationName;
                }

                public string OperationName
                {
                    get { return operationName; }
                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "settings manager must be initialized before performing" +
                        " the following operation: '{0}'.",
                        operationName);
                }
            }

            /// <summary>
            /// thrown if an operation which requires that <see cref="ConfigurationManager"/> not be
            /// initialized is called after <see cref="ConfigurationManager"/> has already
            /// been initialized.
            /// </summary>
            public class ConifgurationManagerAlreadyInitializedException : ConfigurationManagerException
            {
                public ConifgurationManagerAlreadyInitializedException()
                    : base( formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Portolio manager is already initialized.";
                }
            }

            /// <summary>
            /// thrown if initialization of <see cref="ConfigurationManager"/> failed.
            /// </summary>
            public abstract class ConfigurationManagerInitializationException :
                ConfigurationManagerException
            {

                protected ConfigurationManagerInitializationException(
                    string exceptionMessage,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {

                }
            }

            /// <summary>
            /// thrown if initialization of <see cref="SettingsContainer"/> and / or corresponding data
            /// file failed.
            /// </summary>
            public abstract class SettingsInitializationException :
                ConfigurationManagerInitializationException
            {
                private readonly string settingsFilePath;

                public SettingsInitializationException(
                    string settingsFilePath,
                    string exceptionMessage,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {
                    this.settingsFilePath = settingsFilePath;
                }

                public string SettingsFilePath
                {
                    get { return settingsFilePath; }
                }
            }

            /// <summary>
            /// thrown if reading <see cref="SettingsContainer"/> data file failed while trying to
            /// initialize <see cref="SettingsContainer"/>.
            /// </summary>
            public class FileReadSettingsInitializationException :
                SettingsInitializationException
            {
                public FileReadSettingsInitializationException(
                    string settingsFilepath, 
                    Exception innerException)
                    : base(settingsFilepath, formatExceptionMessage(settingsFilepath), innerException)
                {

                }

                private static string formatExceptionMessage(string settingsFilepath)
                {
                    return string.Format(
                        "Read from settings file at location '{0}' failed.",
                        settingsFilepath);
                }
            }

            /// <summary>
            /// thrown if creation of <see cref="SettingsContainer"/> data file failed while trying to
            /// initialize <see cref="SettingsContainer"/>.
            /// </summary>
            public class FileCreateSettingsInitializationException :
                SettingsInitializationException
            {
                public FileCreateSettingsInitializationException(
                    string settingsFilepath,
                    Exception innerException)
                    : base(settingsFilepath, formatExceptionMessage(settingsFilepath), innerException)
                {

                }

                private static string formatExceptionMessage(string settingsFilepath)
                {
                    return string.Format(
                        "Creating settings file at location '{0}' failed.",
                        settingsFilepath);
                }
            }

            /// <summary>
            /// thrown if <see cref="SettingsContainer"/> data file, used for
            /// <see cref="SettingsContainer"/> initialization, was corrupt.
            /// </summary>
            public class CorruptFileSettingsInitializationException :
                SettingsInitializationException
            {
                public CorruptFileSettingsInitializationException(
                    string settingsFilepath,
                    Exception innerException)
                    : base(settingsFilepath, formatExceptionMessage(settingsFilepath), innerException)
                {

                }

                private static string formatExceptionMessage(string settingsFilepath)
                {
                    return string.Format(
                        "Settings file at location '{0}' was corrupt .",
                        settingsFilepath);
                }
            }

            /// <summary>
            /// thrown if updating <see cref="SettingsContainer"/> and / or its corresponding
            /// data file failed.
            /// </summary>
            public class SettingsUpdateException : ConfigurationManagerException
            {
                public SettingsUpdateException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Updating settings failed.";
                }
            }

            /// <summary>
            /// thrown if initialization of <see cref="UserDefinedCommandContainer"/>
            /// and / or corresponding data file failed.
            /// </summary>
            public abstract class UserDefinedCommandsInitializationException :
                ConfigurationManagerInitializationException
            {
                private readonly string userDefinedCommandsFilePath;

                public UserDefinedCommandsInitializationException(
                    string userDefinedCommandsFilePath,
                    string exceptionMessage,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {
                    this.userDefinedCommandsFilePath = userDefinedCommandsFilePath;
                }

                public string UserDefinedCommandsFilePath
                {
                    get { return userDefinedCommandsFilePath; }
                }
            }

            /// <summary>
            /// thrown if reading <see cref="UserDefinedCommandContainer"/>
            /// data file failed while trying to
            /// initialize <see cref="UserDefinedCommandContainer"/>.
            /// </summary>
            public class FileReadUserDefinedCommandsInitializationException :
                UserDefinedCommandsInitializationException
            {
                public FileReadUserDefinedCommandsInitializationException(
                    string userDefinedCommandsFilePath,
                    Exception innerException)
                    : base(
                          userDefinedCommandsFilePath,
                          formatExceptionMessage(userDefinedCommandsFilePath),
                          innerException)
                {

                }

                private static string formatExceptionMessage(string userDefinedCommandsFilePath)
                {
                    return string.Format(
                        "Read from User Defined Commands file at location '{0}' failed.",
                        userDefinedCommandsFilePath);
                }
            }

            /// <summary>
            /// thrown if creation of <see cref="UserDefinedCommandContainer"/> 
            /// data file failed while trying to
            /// initialize <see cref="UserDefinedCommandContainer"/>.
            /// </summary>
            public class FileCreateUserDefinedCommandsInitializationException :
                UserDefinedCommandsInitializationException
            {
                public FileCreateUserDefinedCommandsInitializationException(
                    string userDefinedCommandsFilePath,
                    Exception innerException)
                    : base(userDefinedCommandsFilePath, formatExceptionMessage(userDefinedCommandsFilePath), innerException)
                {

                }

                private static string formatExceptionMessage(string userDefinedCommandsFilePath)
                {
                    return string.Format(
                        "Creating User Defined Commands file at location '{0}' failed.",
                        userDefinedCommandsFilePath);
                }
            }

            /// <summary>
            /// thrown if <see cref="UserDefinedCommandContainer"/> data file, used for
            /// <see cref="UserDefinedCommandContainer"/> initialization, was corrupt.
            /// </summary>
            public class CorruptFileUserDefinedCommandsInitializationException :
                UserDefinedCommandsInitializationException
            {
                public CorruptFileUserDefinedCommandsInitializationException(
                    string userDefinedCommandsFilePath,
                    Exception innerException)
                    : base(
                          userDefinedCommandsFilePath, 
                          formatExceptionMessage(userDefinedCommandsFilePath),
                          innerException)
                {

                }

                private static string formatExceptionMessage(string userDefinedCommandsFilePath)
                {
                    return string.Format(
                        "User Defined Commands file at location '{0}' was corrupt .",
                        userDefinedCommandsFilePath);
                }
            }

            /// <summary>
            /// thrown if updating <see cref="UserDefinedCommandContainer"/> and / or its corresponding
            /// data file failed.
            /// </summary>
            public class UserDefinedCommandsUpdateException : ConfigurationManagerException
            {
                public UserDefinedCommandsUpdateException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Updating User Defined Commands failed.";
                }
            }

            private const string SETTINGS_FILE_PATH = "settings.json";
            private const string BACKUP_SETTINGS_FILE_PATH = "settings.temp.json";

            private const string USER_DEFINED_COMMANDS_FILE_PATH = "user_commands.json";
            private const string BACKUP_USER_DEFINED_COMMANDS_FILE_PATH = "user_commands.temp.json";

            private static ConfigurationManager instance;

            private SettingsContainer settingsContainer;
            private UserDefinedCommandContainer userDefinedCommandContainer;

            private ConfigurationManager()
            {
                initializeSettings();
                initializeUserDefinedCommands();
            }

            public static ConfigurationManager Instance
            {
                get { return instance; }
            }

            /// <summary>
            /// current defined <see cref="OutputReportingProfile"/>.
            /// </summary>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            /// <exception cref="SettingsUpdateException">
            /// <seealso cref="updateSettingsContainerDataFile"/>
            /// </exception>
            public OutputReportingProfile OutputReportingProfile
            {
                get
                {
                    assertManagerInitialized("OutputReportingProfile");
                    return settingsContainer.OutputReportingProfile;
                }
                set
                {
                    assertManagerInitialized("OutputReportingProfile");

                    settingsContainer.OutputReportingProfile = value;
                    updateSettingsContainerDataFile();
                }
            }

            /// <summary>
            /// all <see cref="UserDefinedCommand"/>s which were added.
            /// </summary>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public UserDefinedCommand[] UserDefinedCommands
            {
                get
                {
                    assertManagerInitialized("UserDefinedCommands");
                    return userDefinedCommandContainer.UserDefinedCommands;
                }
            }

            /// <summary>
            /// adds specified <see cref="UserDefinedCommand"/>.
            /// </summary>
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// <seealso cref="UserDefinedCommandContainer.AddUserDefinedCommand(UserDefinedCommand)"/>
            /// <param name="userDefinedCommand"></param>
            /// <exception cref="UserDefinedCommandsUpdateException">
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// </exception>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public void AddUserDefinedCommand(UserDefinedCommand userDefinedCommand)
            {
                assertManagerInitialized("AddUserDefinedCommand");

                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // add to UserDefinedCommandContainer
                this.userDefinedCommandContainer.AddUserDefinedCommand(userDefinedCommand);
            }

            /// <summary>
            /// removes <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/>.
            /// </summary>
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// <seealso cref="UserDefinedCommandContainer.RemoveUserDefinedCommand(string)"/>
            /// <param name="userDefinedCommandAlias"></param>
            /// <exception cref="UserDefinedCommandsUpdateException">
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// </exception>
            /// <exception cref="UserDefinedCommandContainer.RemoveUserDefinedCommand(string)">
            /// thrown if <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/> does not exist
            /// </exception>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public void RemoveUserDefinedCommand(string userDefinedCommandAlias)
            {
                assertManagerInitialized("RemoveUserDefinedCommand");

                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // remove from UserDefinedCommandContainer
                this.userDefinedCommandContainer.RemoveUserDefinedCommand(userDefinedCommandAlias);
            }

            /// <summary>
            /// removes all <see cref="UserDefinedCommand"/>s.
            /// </summary>
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// <seealso cref="UserDefinedCommandContainer.ClearUserDefinedCommands"/>
            /// <exception cref="UserDefinedCommandsUpdateException">
            /// <seealso cref="updateUserDefinedCommandContainerDataFile"/>
            /// </exception>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public void ClearUserDefinedCommands()
            {
                assertManagerInitialized("ClearUserDefinedCommands");

                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // clear all UserDefinedCommands from UserDefinedCommandContainer
                this.userDefinedCommandContainer.ClearUserDefinedCommands();
            }

            /// <summary>
            /// returns whether <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/> exists.
            /// </summary>
            /// <seealso cref="UserDefinedCommandContainer.UserDefinedCommandExists(string)"/>
            /// <param name="userDefinedCommandAlias"></param>
            /// <returns>
            /// true if <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/> exists,
            /// else false
            /// </returns>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public bool UserDefinedCommandExists(string userDefinedCommandAlias)
            {
                assertManagerInitialized("UserDefinedCommandExists");
                return this.userDefinedCommandContainer.UserDefinedCommandExists(userDefinedCommandAlias);
            }

            /// <summary>
            /// returns <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/>.
            /// </summary>
            /// <seealso cref="UserDefinedCommandContainer.GetUserDefinedCommand(string)"/>
            /// <param name="userDefinedCommandAlias"></param>
            /// <returns>
            /// <see cref="UserDefinedCommand"/> associated with specified
            /// <paramref name="userDefinedCommandAlias"/>
            /// </returns>
            /// <exception cref="UserDefinedCommandContainer.UserDefinedCommandNotFoundException">
            /// <seealso cref="UserDefinedCommandContainer.GetUserDefinedCommand(string)"/>
            /// </exception>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// <seealso cref="assertManagerInitialized(string)"/>
            /// </exception>
            public UserDefinedCommand GetUserDefinedCommand(string userDefinedCommandAlias)
            {
                assertManagerInitialized("GetUserDefinedCommand");
                return this.userDefinedCommandContainer.GetUserDefinedCommand(userDefinedCommandAlias);
            }

            /// <summary>
            /// initializes <see cref="ConfigurationManager"/>.
            /// </summary>
            /// <exception cref="ConifgurationManagerAlreadyInitializedException">
            /// <seealso cref="assertManagerNotInitialized"/>
            /// </exception>
            public static void Initialize()
            {
                assertManagerNotInitialized();

                instance = new ConfigurationManager();
            }

            /// <summary>
            /// asserts that this manager has been initialized.
            /// </summary>
            /// <param name="operationName"></param>
            /// <exception cref="ConifgurationManagerNotInitializedException">
            /// thrown if this manager has not yet been initialized
            /// </exception>
            private static void assertManagerInitialized(string operationName)
            {
                if (Instance == null)
                {
                    throw new ConifgurationManagerNotInitializedException(operationName);
                }
            }

            /// <summary>
            /// asserts that this manager was not yet initialized.
            /// </summary>
            /// <exception cref="ConifgurationManagerAlreadyInitializedException">
            /// thrown if this manager has already been initialized
            /// </exception>
            private static void assertManagerNotInitialized()
            {
                if (Instance != null)
                {
                    throw new ConifgurationManagerAlreadyInitializedException();
                }
            }

            /// <summary>
            /// initializes <see cref="SettingsContainer"/> and corresponding data file.
            /// </summary>
            /// <seealso cref="initializeSettingsContainerFromExistingDataFile"/>
            /// <seealso cref="initializeDefaultSettingsContainerAndDataFile"/>
            /// <exception cref="FileReadSettingsInitializationException">
            /// thrown if reading data from <see cref="SettingsContainer"/> file failed
            /// </exception>
            /// <exception cref="CorruptFileSettingsInitializationException">
            /// thrown if <see cref="SettingsContainer"/> file data is not in a valid JSON format,
            /// or <see cref="SettingsContainer"/> parsing from data failed
            /// </exception>
            /// <exception cref="FileCreateSettingsInitializationException">
            /// thrown if creating a <see cref="SettingsContainer"/> data file with default settings
            /// failed
            /// </exception>
            private void initializeSettings()
            {
                if (FileIOUtils.FileExists(SETTINGS_FILE_PATH)) // settings file exists
                {
                    ConsoleIOManager.Instance.LogNotice(
                        "Settings file found. Using existing file.",
                        ConsoleIOManager.eOutputReportType.System);

                    try
                    {
                        initializeSettingsContainerFromExistingDataFile();
                    }
                    catch (Exception exception)
                    {
                        if (exception is FileReadException) // settings file read failed
                        {
                            throw new FileReadSettingsInitializationException(
                                SETTINGS_FILE_PATH,
                                exception);
                        }
                        // corrupt settings file 
                        else if (
                            exception is JsonSerializationException
                            || exception is SettingsContainerJsonObjectParseException) 
                        {
                            throw new CorruptFileSettingsInitializationException(
                                SETTINGS_FILE_PATH,
                                exception);
                        }
                        else // unhandled exception
                        {
                            throw exception;
                        }
                    }
                }
                else // settings file does not exist
                {
                    ConsoleIOManager.Instance.LogNotice(
                        "Settings file not found. Creating new settings file with default values ..",
                        ConsoleIOManager.eOutputReportType.System);

                    try
                    {
                        initializeDefaultSettingsContainerAndDataFile();

                        ConsoleIOManager.Instance.LogNotice(
                            "New Settings file created successfully.",
                            ConsoleIOManager.eOutputReportType.System);
                    }
                    catch (FileWriteException fileWriteException)
                    {
                        throw new FileCreateSettingsInitializationException(
                            SETTINGS_FILE_PATH,
                            fileWriteException);
                    }
                }
            }

            /// <summary>
            /// initializes <see cref="SettingsContainer"/> with data from existing
            /// corresponding data file.
            /// </summary>
            /// <exception cref="FileReadException">
            /// thrown if reading from <see cref="SettingsContainer"/> data file failed
            /// </exception>
            /// <exception cref="JsonSerializationException">
            /// thrown if <see cref="SettingsContainer"/> data file content is not in a valid
            /// JSON format.
            /// </exception>
            /// <exception cref="SettingsContainerJsonObjectParseException">
            /// thrown if parsing <see cref="SettingsContainer"/> from <see cref="SettingsContainer"/>
            /// data file content failed
            /// </exception>
            private void initializeSettingsContainerFromExistingDataFile()
            {
                // initialize settings container with values from settings file
                string settingsContainerJsonString = FileIOUtils.ReadTextFromFile(SETTINGS_FILE_PATH);
                object settingsContainerJsonObject = JsonUtils.DeserializeObject<object>(
                    settingsContainerJsonString);
                this.settingsContainer = SettingsContainer.Parse(settingsContainerJsonObject);
            }

            /// <summary>
            /// initializes a <see cref="SettingsContainer"/> with default values,
            /// and creates a corresponding <see cref="SettingsContainer"/> data file.
            /// </summary>
            /// <exception cref="FileWriteException">
            /// <seealso cref="FileIOUtils.WriteTextToFile(string, string)"/>
            /// </exception>
            private void initializeDefaultSettingsContainerAndDataFile()
            {
                // initialize settings container with default values
                this.settingsContainer = new SettingsContainer();

                // create settings file having default values
                string settingsContainerJsonString = JsonUtils.SerializeObject(this.settingsContainer);
                FileIOUtils.WriteTextToFile(SETTINGS_FILE_PATH, settingsContainerJsonString);
            }

            /// <summary>
            /// updates <see cref="SettingsContainer"/> data file with present contents
            /// of <see cref="SettingsContainer"/>.
            /// </summary>
            /// <exception cref="SettingsUpdateException">
            /// thrown if updating <see cref="SettingsContainer"/> data file failed
            /// </exception>
            private void updateSettingsContainerDataFile()
            {
                try
                {
                    writeSettingsContainerDataToFile();
                }
                catch (BackupFileWriteException backupFileWriteException)
                {
                    throw new SettingsUpdateException(backupFileWriteException);
                }
            }

            /// <summary>
            /// writes <see cref="SettingsContainer"/> object JSON serialization to
            /// data file.
            /// </summary>
            /// <exception cref="BackupFileCreateException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileRenameException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileDeleteException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            private void writeSettingsContainerDataToFile()
            {
                // get SettingsContainer JSON string
                string settingsContainerJsonString = JsonUtils.SerializeObject(this.settingsContainer);

                // write SettingsContainer JSON string to data file
                FileIOUtils.WriteTextToFileWithBackup(
                    SETTINGS_FILE_PATH,
                    settingsContainerJsonString,
                    BACKUP_SETTINGS_FILE_PATH);
            }

            /// <summary>
            /// initializes <see cref="UserDefinedCommandContainer"/> and corresponding data file.
            /// </summary>
            /// <exception cref="FileReadUserDefinedCommandsInitializationException">
            /// thrown if reading data from existing <see cref="UserDefinedCommandContainer"/>
            /// data file failed
            /// </exception>
            /// <exception cref="CorruptFileUserDefinedCommandsInitializationException">
            /// thrown if data in existing <see cref="UserDefinedCommandContainer"/> data file
            /// is invalid
            /// </exception>
            private void initializeUserDefinedCommands()
            {
                if(FileIOUtils.FileExists(USER_DEFINED_COMMANDS_FILE_PATH)) // saved commands file exists
                {
                    try
                    {
                        // initialize UserDefinedCommandsContainer with data from file
                        string userDefinedCommandsContainerJsonString = FileIOUtils.ReadTextFromFile(
                            USER_DEFINED_COMMANDS_FILE_PATH);
                        this.userDefinedCommandContainer = JsonUtils.DeserializeObject<UserDefinedCommandContainer>(
                            userDefinedCommandsContainerJsonString);
                    }
                    catch(Exception exception)
                    {
                        // reading from user defined commands file failed
                        if (exception is FileReadException) 
                        {
                            throw new FileReadUserDefinedCommandsInitializationException(
                                USER_DEFINED_COMMANDS_FILE_PATH,
                                exception);
                        }
                        // corrupt user defined commands file
                        else if (exception is JsonSerializationException) 
                        {
                            throw new CorruptFileUserDefinedCommandsInitializationException(
                                USER_DEFINED_COMMANDS_FILE_PATH,
                                exception);
                        }
                        else // unhandled exception
                        {
                            throw exception;
                        }
                    }
                }
                else // saved commands file does not exist
                {
                    // initialize an empty UserDefinedCommandContainer
                    this.userDefinedCommandContainer = new UserDefinedCommandContainer();

                    // write empty UserDefinedCommandContainer data to file
                    writeUserDefinedCommandContainerDataToFile();
                }
            }

            /// <summary>
            /// updates <see cref="UserDefinedCommandContainer"/> data file with present contents
            /// of <see cref="UserDefinedCommandContainer"/>.
            /// </summary>
            /// <exception cref="UserDefinedCommandsUpdateException">
            /// thrown if updating <see cref="UserDefinedCommandContainer"/> data file failed
            /// </exception>
            private void updateUserDefinedCommandContainerDataFile()
            {
                try
                {
                    writeUserDefinedCommandContainerDataToFile();
                }
                // writing to saved commands file failed
                catch (BackupFileWriteException backupFileWriteException) 
                {
                    throw new UserDefinedCommandsUpdateException(backupFileWriteException);
                }
            }

            /// <summary>
            /// writes <see cref="UserDefinedCommandContainer"/> object JSON serialization to
            /// data file.
            /// </summary>
            /// <exception cref="BackupFileCreateException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileRenameException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            /// <exception cref="BackupFileDeleteException">
            /// <seealso cref="FileIOUtils.WriteTextToFileWithBackup(string, string, string)"/>
            /// </exception>
            private void writeUserDefinedCommandContainerDataToFile()
            {
                // get UserDefinedCommandContainer JSON string
                string userDefinedCommandContainerJsonString = JsonUtils.SerializeObject(
                    this.userDefinedCommandContainer);

                // write UserDefinedCommandContainer JSON string to fiel
                FileIOUtils.WriteTextToFileWithBackup(
                    USER_DEFINED_COMMANDS_FILE_PATH,
                    userDefinedCommandContainerJsonString,
                    BACKUP_USER_DEFINED_COMMANDS_FILE_PATH);
            }
        }
    }
}

