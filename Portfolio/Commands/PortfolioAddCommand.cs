using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands
    {
        /// <summary>
        /// <para>
        /// adds specified coin to portfolio.
        /// </para>
        /// <para>
        /// command syntax: portfolio add [coin name / symbol]
        /// </para>
        /// </summary>
        internal class PortfolioAddCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 1;
            private const int MAX_NUMBER_OF_ARGUMENTS = 10;

            // command sub-prefix
            private const string SUBPREFIX = "add";

            internal PortfolioAddCommand()
                : base(SUBPREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// adds new <see cref="PortfolioEntry"/> to portfolio, corresponding to coin id
            /// specified in <paramref name="commandArguments"/>[0].
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="PortfolioManager.AddCoin(int)"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                // command arguments should be coin names or symbols
                string[] coinNamesOrSymbols = commandArguments;

                try
                {
                    // get coin ids by name or symbol
                    long[] coinIds =
                        CoinListingManager.Instance.GetCoinIdsByNamesOrSymbols(coinNamesOrSymbols);

                    // add coins to portfolio
                    PortfolioManager.Instance.AddCoins(coinIds);

                    // log success notice
                    string coinsPortfolioAddSuccessNotice = buildPortfolioAddSuccessNotice(coinIds);
                    ConsoleIOManager.Instance.LogNotice(coinsPortfolioAddSuccessNotice);

                    commandExecutedSuccessfuly = true;
                }
                catch (CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    // coin with specified name / symbol not found in listing repository
                    ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    commandExecutedSuccessfuly = false;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                   PortfolioCommandUtils.HandleDatabaseCommunicationException(databaseCommunicationException);
                    commandExecutedSuccessfuly = false;
                }
                catch (CoinAlreadyInPortfolioException coinAlreadyInPortfolioException)
                {
                    // coin id is already in portfolio
                    long coinId = coinAlreadyInPortfolioException.CoinId;
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        "There's already an entry in portfolio for '{0}'.",
                        coinName);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }

            private static string buildPortfolioAddSuccessNotice(long[] coinIds)
            {
                StringBuilder successNoticeStringBuilder = new StringBuilder();

                for (int i = 0; i < coinIds.Length; i++)
                {
                    long coinId = coinIds[i];
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                    successNoticeStringBuilder.AppendFormat("'{0}'", coinName);

                    if (i < coinIds.Length - 1)
                    {
                        successNoticeStringBuilder.Append(", ");
                    }
                }

                successNoticeStringBuilder.Append(" successfully added to portfolio.");

                return successNoticeStringBuilder.ToString();
            }
        }
    }
}

