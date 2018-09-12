using CryptoBlock.ExceptionManagement;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using System.Threading;
using CryptoBlock.CommandHandling;
using CryptoBlock.PortfolioManagement;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using CryptoBlock.SettingsManagement;
using static CryptoBlock.IOManagement.ConsoleIOManager;
using static CryptoBlock.SettingsManagement.SettingsManager;
using System;

namespace CryptoBlock
{
    internal class ProgramManager
    {
        private const int USER_COMMAND_LISTEN_SLEEP_TIME_MILLIS = 10;

        private readonly EventWaitHandle userHitReturnKeyWaitHandle = new AutoResetEvent(false);

        internal void StartProgram()
        {
            ConsoleIOManager.Instance.RegisterInput = false;
            initializeManagers();
            ConsoleIOManager.Instance.RegisterInput = true;

            ListenForUserCommands();
        }

        internal void ListenForUserCommands()
        {
            ConsoleIOManager.Instance.EndOfInputKeyRegistered += consoleIOManager_EndOfInputKeyRegistered;

            while (true)
            {
                // wait till user hits return key
                userHitReturnKeyWaitHandle.WaitOne();
               
                // user hit return key

                // read and parse user command
                string userCommand = ConsoleIOManager.Instance.FlushInputBuffer();
                CommandParsingManager.Instance.ParseCommand(userCommand);

                // reset return key wait handle
                userHitReturnKeyWaitHandle.Reset();
            }
        }

        private void consoleIOManager_EndOfInputKeyRegistered(string inputLine)
        {
            userHitReturnKeyWaitHandle.Set();
        }

        private void coinDataManager_RepositoryInitialized(CoinTickerManager coinDataManager)
        {
            ConsoleIOManager.Instance.LogNotice("Coin ticker repository initialized successfully.",
                ConsoleIOManager.eOutputReportType.System);
        }

        private void initializeManagers()
        {
            initializeCoinListingManager();

            try
            {
                initializePortfolioManager();
            }
            catch (DatabaseCommunicationException databaseCommunicationException)
            {
                ConsoleIOManager.Instance.LogError(
                    "An exception occurred while trying to initialize Portfolio Manager." +
                    " Program cannot be started.",
                    eOutputReportType.SystemCritical);
                logExceptionAndEndProgram(databaseCommunicationException);

                return;
            }

            initializeCoinTickerManager(CoinListingManager.Instance.RepositoryCount);

            initializeCommandParsingManager();

            try
            { 
                initializeSettingsManager(); 
            }
            catch (SettingsManagerInitializationException settingsManagerInitializationException)
            {
                ConsoleIOManager.Instance.LogError(
                    "An exception occurred while trying to initialize Settings Manager." +
                    " Program cannot be started.",
                    eOutputReportType.SystemCritical);
                logExceptionAndEndProgram(settingsManagerInitializationException);

                return;
            }
        }

        private void initializeCoinListingManager()
        {
            bool coinListingRepositoryInitialized = false;

            while (!coinListingRepositoryInitialized)
            {
                ConsoleIOManager.Instance.LogNotice(
                    "Initializing coin listing repository ..",
                    ConsoleIOManager.eOutputReportType.SystemCritical);

                try
                {
                    CoinListingManager.Instance.Initialize();
                    coinListingRepositoryInitialized = true;
                }
                catch (CoinListingManager.RepositoryUpdateException repositoryUpdateException)
                {
                    ExceptionManager.Instance.LogException(repositoryUpdateException);

                    ConsoleIOManager.Instance.LogError(
                        "An error occurred while trying to initialize coin listing repository.",
                        eOutputReportType.SystemCritical);
                    ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                        eOutputReportType.CommandExecution);
                    ConsoleIOManager.Instance.LogNotice(
                        "Retrying ..",
                        eOutputReportType.SystemCritical);

                    Thread.Sleep(5000);
                }
            }

            ConsoleIOManager.Instance.LogNotice(
                "Coin listing repository initialized successfully.",
                ConsoleIOManager.eOutputReportType.SystemCritical);
        }

        private void initializeCoinTickerManager(int numberOfCoins)
        {
            CoinTickerManager.Initialize(numberOfCoins);
            CoinTickerManager.Instance.RepositoryInitializedEvent += coinDataManager_RepositoryInitialized;
        }

        private void initializePortfolioManager()
        {
            PortfolioManager.Initialize();
        }

        private void initializeCommandParsingManager()
        {
            CommandParsingManager.Initialize();
        }

        private void initializeSettingsManager()
        {
            SettingsManager.Initialize();
        }

        private void logExceptionAndEndProgram(Exception exception)
        {
            ExceptionManager.Instance.ConsoleLogReferToErrorLogFileMessage(
                eOutputReportType.SystemCritical);
            ExceptionManager.Instance.LogException(exception);

            ConsoleIOManager.Instance.ShowPressAnyKeyToContinueDialog(
                eOutputReportType.SystemCritical);
        }
    }
}
