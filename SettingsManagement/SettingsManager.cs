using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
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
using static CryptoBlock.SettingsManagement.SettingsContainer;

namespace CryptoBlock
{
    namespace SettingsManagement
    {
        public class SettingsManager
        {
            public abstract class SettingsManagerException : Exception
            {
                private readonly string settingsFilepath;

                protected SettingsManagerException(
                    string settingsFilepath,
                    string exceptionMessage = null,
                    Exception innerException = null)
                    : base(exceptionMessage, innerException)
                {
                    this.settingsFilepath = settingsFilepath;
                }

                public string SettingsFilepath
                {
                    get { return settingsFilepath; }
                }
            }

            public class ManagerNotInitializedException : SettingsManagerException
            {
                private readonly string operationName;

                public ManagerNotInitializedException(string settingsFilepath, string operationName)
                    : base(settingsFilepath, formatExceptionMessage(operationName))
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

            public class ManagerAlreadyInitializedException : SettingsManagerException
            {
                public ManagerAlreadyInitializedException(string settingsFilepath)
                    : base(settingsFilepath, formatExceptionMessage())
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Portolio manager is already initialized.";
                }
            }

            public abstract class SettingsManagerInitializationException : SettingsManagerException
            {

                protected SettingsManagerInitializationException(
                    string settingsFilepath,
                    string exceptionMessage,
                    Exception innerException)
                    : base(settingsFilepath, exceptionMessage, innerException)
                {

                }
            }

            public class FileReadSettingManagerInitializationException :
                SettingsManagerInitializationException
            {
                public FileReadSettingManagerInitializationException(
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

            public class FileCreateSettingsManagerInitializationException :
                SettingsManagerInitializationException
            {
                public FileCreateSettingsManagerInitializationException(
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

            public class CorruptFileSettingManagerInitializationException :
                SettingsManagerInitializationException
            {
                public CorruptFileSettingManagerInitializationException(
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

            public class SettingsManagerUpdateException : SettingsManagerException
            {
                public SettingsManagerUpdateException(string settingsFilePath, Exception innerException)
                    : base(settingsFilePath, formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Updating settings manager failed.";
                }
            }

            private const string SETTINGS_FILE_PATH = "settings.json";
            private const string BACKUP_SETTINGS_FILE_PATH = "settings.temp.json";

            private static SettingsManager instance;

            private SettingsContainer settingsContainer;

            private SettingsManager()
            {
                if(FileIOUtils.FileExists(SETTINGS_FILE_PATH)) // settings file found
                {
                    ConsoleIOManager.Instance.LogNotice(
                        "Settings file found. Using existing file.",
                        ConsoleIOManager.eOutputReportType.System);

                    try
                    {
                        useExistingSettingsFile();
                    }
                    catch(Exception exception)
                    {
                        if(exception is FileReadException) // settings file read failed
                        {
                            throw new FileReadSettingManagerInitializationException(
                                SETTINGS_FILE_PATH,
                                exception);
                        }
                        else if(
                            exception is JsonSerializationException
                            || exception is SettingsContainerObjectParseException) // corrupt settings file 
                        {
                            throw new CorruptFileSettingManagerInitializationException(
                                SETTINGS_FILE_PATH,
                                exception);
                        }
                    }                  
                }
                else // settings file not found
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
                    catch(FileWriteException fileWriteException)
                    {
                        throw new FileCreateSettingsManagerInitializationException(
                            SETTINGS_FILE_PATH,
                            fileWriteException);
                    }
                }
            }

            public static SettingsManager Instance
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
                    updateSettingsFile();
                }
            }

            private void updateSettingsFile()
            {
                try
                {
                    string settingsContainerJsonString = JsonUtils.SerializeObject(this.settingsContainer);
                    FileIOUtils.WriteTextToFileWithBackup(
                        SETTINGS_FILE_PATH,
                        settingsContainerJsonString,
                        BACKUP_SETTINGS_FILE_PATH);
                }
                catch(BackupFileWriteException backupFileWriteException)
                {
                    throw new SettingsManagerUpdateException(SETTINGS_FILE_PATH, backupFileWriteException);
                }
            }

            public static void Initialize()
            {
                assertManagerNotInitialized();

                instance = new SettingsManager();
            }

            private static void assertManagerInitialized(string operationName)
            {
                if (Instance == null)
                {
                    throw new ManagerNotInitializedException(SETTINGS_FILE_PATH, operationName);
                }
            }

            private static void assertManagerNotInitialized()
            {
                if (Instance != null)
                {
                    throw new ManagerAlreadyInitializedException(SETTINGS_FILE_PATH);
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
        }
    }
}

