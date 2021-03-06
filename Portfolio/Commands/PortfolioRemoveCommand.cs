﻿using CryptoBlock.IOManagement;
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
        /// removes specified coin from portfolio.
        /// </para>
        /// <para>
        /// command syntax: portfolio remove [coin name / symbol]
        /// </para>
        /// </summary>
        internal class PortfolioRemoveCommand : PortfolioCommand
        {
            private const int MIN_NUMBER_OF_ARGUMENTS = 1;
            private const int MAX_NUMBER_OF_ARGUMENTS = 10;

            // command sub-prefix
            private const string SUB_PREFIX = "remove";

            internal PortfolioRemoveCommand()
                : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
            {

            }

            /// <summary>
            /// removes <see cref="PortfolioEntry"/> corresponding to coin id
            /// specified in <paramref name="commandArguments"/>[0] from portfolio.
            /// returns whether command executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.GetCoinIdByNameOrSymbol(string)"/>
            /// <seealso cref="PortfolioManager.RemoveCoin(int)"/>
            /// <param name="commandArguments"></param>
            /// <returns>
            /// true if command executed successfully,
            /// else false
            /// </returns>
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

                    // remove coins from portfolio
                    PortfolioManager.Instance.RemoveCoins(coinIds);

                    // log success notice
                    string portfolioCoinRemoveSuccessNotice = buildPortfolioCoinRemoveSuccessNotice(coinIds);
                    ConsoleIOManager.Instance.LogNotice(
                        portfolioCoinRemoveSuccessNotice,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = true;
                }
                catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    // coin with specified name / symbol not found in listing repository
                    ConsoleIOManager.Instance.LogError(
                        coinNameOrSymbolNotFoundException.Message,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    commandExecutedSuccessfuly = false;
                }
                catch (CoinNotInPortfolioException coinNotInPortfolioException)
                {
                    // coin id corresponding to given name / symbol does not exist in portfolio manager
                    long coinId = coinNotInPortfolioException.CoinId;
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                    ConsoleIOManager.Instance.LogErrorFormat(
                        false,
                        ConsoleIOManager.eOutputReportType.CommandExecution,
                        "There's no entry in portfolio manager for '{0}'.",
                        coinName);

                    commandExecutedSuccessfuly = false;
                }
                catch (DatabaseCommunicationException databaseCommunicationException)
                {
                   PortfolioCommand.HandleDatabaseCommunicationException(databaseCommunicationException);

                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }

            /// <summary>
            /// returns a success notice for removing coins corresponding to
            /// specified <paramref name="coinIds"/>.
            /// </summary>
            /// <param name="coinIds"></param>
            /// <returns>
            /// success notice for removing coins corresponding to
            /// specified <paramref name="coinIds"/>
            /// </returns>
            private static string buildPortfolioCoinRemoveSuccessNotice(long[] coinIds)
            {
                StringBuilder removeNoticeStringBuilder = new StringBuilder();

                for (int i = 0; i < coinIds.Length; i++)
                {
                    long coinId = coinIds[i];
                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                    removeNoticeStringBuilder.AppendFormat("'{0}'", coinName);

                    if (i < coinIds.Length - 1)
                    {
                        removeNoticeStringBuilder.Append(", ");
                    }
                }

                removeNoticeStringBuilder.Append(" successfully removed from portfolio.");

                return removeNoticeStringBuilder.ToString();
            }
        }
    }
}
