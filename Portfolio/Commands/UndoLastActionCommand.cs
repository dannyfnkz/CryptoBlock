using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        internal class UndoLastActionCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            // command sub-prefix
            private const string SUB_PREFIX = "undo";

            internal UndoLastActionCommand()
                : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                if(PortfolioManager.Instance.UndoableLastActionAvailable)
                {
                    PortfolioManager.Instance.UndoLastAction();

                    string noticeMessage = "Changes made to portfolio by last command were successfully" +
                        " undone.";
                    ConsoleIOManager.Instance.LogNotice(noticeMessage);

                    commandExecutedSuccessfuly = true;
                }
                else
                {
                    string errorMessage = "No changes were recently made to portfolio.";
                    ConsoleIOManager.Instance.LogError(errorMessage);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}