using CryptoBlock.ConfigurationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.ConfigurationManagement.ConfigurationManager;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.UserDefinedCommands
    {
        internal class UserDefinedCommandsClearCommand : UserDefinedCommandsCommand
        {
            private const string PREFIX = "clear";

            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            internal UserDefinedCommandsClearCommand()
                : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                try
                {
                    // remove all UserDefinedCommands
                    ConfigurationManager.Instance.ClearUserDefinedCommands();

                    // log success notice
                    string successNotice = "All User Defined Commands were successfully removed.";
                    LogCommandNotice(successNotice);

                    commandExecutedSuccessfuly = true;
                }
                catch (UserDefinedCommandsUpdateException userDefinedCommandsUpdateException)
                {
                    UserDefinedCommandsCommand.HandleUserDefinedCommandsUpdateException(
                        userDefinedCommandsUpdateException);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}