using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        internal class PortfolioClearCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 0;
            private const int MAX_NUMBER_OF_ARGUMENTS = 0;

            // command sub-prefix
            private const string SUB_PREFIX = "clear";

            internal PortfolioClearCommand()
                : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            public override void ExecuteCommand(string[] commandArguments)
            {
                bool commandArgumentsValid = base.CheckCommandArgumentConstraints(commandArguments);

                if (!commandArgumentsValid)
                {
                    return;
                }

                try
                {
                    // get all coinIds in portfolio
                    long[] coinIds = PortfolioManager.Instance.CoinIds;

                    foreach (long coinId in coinIds)
                    {
                        // delete PortfolioEntry corresponding to coinId from portfolio
                        PortfolioManager.Instance.RemoveCoin(coinId);
                    }

                    // log successful removal notice to console
                    ConsoleIOManager.Instance.LogNotice(
                        "All entries were successfully removed from portfolio.");
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                    HandleDatabaseCommunicationException(databaseCommunicationException);
                }
            }
        }
    }

}
