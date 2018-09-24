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
        /// <summary>
        /// undoes the last action performed on portfolio, if one exists.
        /// </summary>
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

            /// <summary>
            /// undoes the last action performed on portfolio, if one exists.
            /// returns whether last action was successfully undone.
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// true if last action was successfully undone,
            /// else false
            /// </returns>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                // check whether an undoable last action is available (that is, an action was performed
                // on portfolio and has not yet been undone)
                if(PortfolioManager.Instance.UndoableLastActionAvailable)
                {
                    PortfolioManager.Instance.UndoLastAction();

                    string noticeMessage = "Changes made to portfolio by last command were successfully" +
                        " undone.";
                    ConsoleIOManager.Instance.LogNotice(
                        noticeMessage,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = true;
                }
                else
                {
                    string errorMessage = "No changes were recently made to portfolio.";
                    ConsoleIOManager.Instance.LogError(
                        errorMessage,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}