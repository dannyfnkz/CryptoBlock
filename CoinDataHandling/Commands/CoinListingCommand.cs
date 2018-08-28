using CryptoBlock.IOManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ServerDataManagement.Commands
    {
        /// <summary>
        /// <para>
        /// prints <see cref="CoinListing"/> data in tabular format.
        /// </para>
        /// <para>
        /// syntax: listing [coin0 name/symbol] ?[coin1 name/symbol] ?[coin2 name/symbol] ...
        /// </para>
        /// </summary>
        internal class CoinListingCommand : ServerDataCommand
        {
            private const string PREFIX = "listing";

            internal CoinListingCommand()
                : base(PREFIX)
            {

            }

            /// <summary>
            /// prints <see cref="CoinListing"/> data corresponding to coin name / symbols
            /// contained in <paramref name="commandArguments"/> in tabular format.
            /// returns whether command was executed successfully.
            /// </summary>
            /// <seealso cref="CoinListingManager.FetchCoinIds(string[])"/>
            /// <seealso cref="CoinListingManager.GetCoinListingTableDisplayString(int[])"/>
            /// <param name="commandArguments"></param>
            protected override bool Execute(string[] commandArguments)
            {
                bool commandExecutedSuccessfuly;

                try
                {
                    // fetch coin ids corresponding to coin names / symbols
                    long[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

                    // print coin listing display table containing coin listings corresponding
                    // to fetched coin ids
                    string coinListingTableString =
                        CoinListingManager.Instance.GetCoinListingTableDisplayString(coinIds);
                    ConsoleIOManager.Instance.PrintData(coinListingTableString);

                    commandExecutedSuccessfuly = true;
                }
                catch (CoinListingManager.CoinNameOrSymbolNotFoundException coinNameOrSymbolNotFoundException)
                {
                    ConsoleIOManager.Instance.LogError(coinNameOrSymbolNotFoundException.Message);
                    commandExecutedSuccessfuly = false;
                }

                return commandExecutedSuccessfuly;
            }
        }
    }
}

