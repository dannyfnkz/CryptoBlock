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
using static CryptoBlock.ConfigurationManagement.SettingsContainer;

namespace CryptoBlock
{
    namespace ConfigurationManagement
    {
        public class ConfigurationManager
        {
            public abstract class ConfigurationManagerException : Exception
            {
                protected ConfigurationManagerException(
                    string exceptionMessage = null,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {
                    
                }
            }

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

            public UserDefinedCommand[] UserDefinedCommands
            {
                get { return userDefinedCommandContainer.UserDefinedCommands; }
            }

            public void AddUserDefinedCommand(UserDefinedCommand userDefinedCommand)
            {
                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // add to UserDefinedCommandContainer
                this.userDefinedCommandContainer.AddUserDefinedCommand(userDefinedCommand);
            }

            public void RemoveUserDefinedCommand(string userDefinedCommandAlias)
            {
                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // remove from UserDefinedCommandContainer
                this.userDefinedCommandContainer.RemoveUserDefinedCommand(userDefinedCommandAlias);
            }

            public void ClearUserDefinedCommands()
            {
                // write updated UserDefinedCommandContainer data to file
                updateUserDefinedCommandContainerDataFile();

                // clear all UserDefinedCommands from UserDefinedCommandContainer
                this.userDefinedCommandContainer.ClearUserDefinedCommands();
            }

            public bool UserDefinedCommandExists(string userDefinedCommandAlias)
            {
                return this.userDefinedCommandContainer.UserDefinedCommandExists(userDefinedCommandAlias);
            }

            public UserDefinedCommand GetUserDefinedCommand(string userDefinedCommandAlias)
            {
                return this.userDefinedCommandContainer.GetUserDefinedCommand(userDefinedCommandAlias);
            }

            public static void Initialize()
            {
                assertManagerNotInitialized();

                instance = new ConfigurationManager();
            }

            private static void assertManagerInitialized(string operationName)
            {
                if (Instance == null)
                {
                    throw new ConifgurationManagerNotInitializedException(operationName);
                }
            }

            private static void assertManagerNotInitialized()
            {
                if (Instance != null)
                {
                    throw new ConifgurationManagerAlreadyInitializedException();
                }
            }

            private void initializeSettings()
            {
                if (FileIOUtils.FileExists(SETTINGS_FILE_PATH)) // settings file exists
                {
                    ConsoleIOManager.Instance.LogNotice(
                        "Settings file found. Using existing file.",
                        ConsoleIOManager.eOutputReportType.System);

                    try
                    {
                        useExistingSettingsFile();
                    }
                    catch (Exception exception)
                    {
                        if (exception is FileReadException) // settings file read failed
                        {
                            throw new FileReadSettingsInitializationException(
                                SETTINGS_FILE_PATH,
                                exception);
                        }
                        else if (
                            exception is JsonSerializationException
                            || exception is SettingsContainerObjectParseException) // corrupt settings file 
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
                        createNewSettingsFile();

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

            private void useExistingSettingsFile()
            {
                // initialize settings container with values from settings file
                string settingsContainerJsonString = FileIOUtils.ReadTextFromFile(SETTINGS_FILE_PATH);
                object settingsContainerJsonObject = JsonUtils.DeserializeObject<object>(
                    settingsContainerJsonString);
                this.settingsContainer = SettingsContainer.Parse(settingsContainerJsonObject);
            }

            private void createNewSettingsFile()
            {
                // initialize settings container with default values
                this.settingsContainer = new SettingsContainer();

                // create settings file having default values
                string settingsContainerJsonString = JsonUtils.SerializeObject(this.settingsContainer);
                FileIOUtils.WriteTextToFile(SETTINGS_FILE_PATH, settingsContainerJsonString);
            }


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
                        if(exception is FileReadException) // saved commands file read failed
                        {
                            throw new FileReadUserDefinedCommandsInitializationException(
                                USER_DEFINED_COMMANDS_FILE_PATH,
                                exception);
                        }
                        else if(exception is JsonSerializationException) // corrupt saved commands file
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

