using CryptoBlock.CommandHandling;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using System.Collections.Generic;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// handles executing portfolio commands.
        /// </summary>
        public class PortfolioCommandExecutor : CommandExecutor
        {
            /// <summary>
            /// represents an executable portfolio command.
            /// </summary>
            private abstract class PortfolioCommand : Command
            {
                // command prefix
                private const string PREFIX = "portfolio";

                internal PortfolioCommand(
                    string inheritingCommandPrefix,
                    int minNumberOfArguments,
                    int maxNumberOfArguments)
                    : base(formatPrefix(inheritingCommandPrefix), minNumberOfArguments, maxNumberOfArguments)
                {

                }

                /// <summary>
                /// returns prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
                /// <see cref="Command.Prefix"/>.
                /// </summary>
                /// <param name="subPrefix"></param>
                /// <returns></returns>
                private static string formatPrefix(string inheritingCommandPrefix)
                {
                    return PREFIX + " " + inheritingCommandPrefix;
                }
            }

            /// <summary>
            /// <para>
            /// prints portfolio data in a tabular form.
            /// </para>
            /// <para>
            /// syntax: portfolio view ?[coin0 name/symbol] ?[coin1 name/symbol] ...
            /// </para>
            /// </summary>
            private class PortfolioViewCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 0;
                private const int MAX_NUMBER_OF_ARGUMENTS = 20;

                // command sub-prefix
                private const string SUBPREFIX = "view";

                internal PortfolioViewCommand()
                    : base(SUBPREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                /// <summary>
                /// executes command with given <paramref name="commandArguments"/>.
                /// </summary>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    try
                    {
                        // only coin ids which have a corresponding portfolio entry are displayed
                        List<int> coinIdsWithPortfolioEntry = new List<int>();
                        List<string> coinNamesWithoutPortfolioEntry = new List<string>();

                        if(commandArguments.Length == 0) 
                        {
                            // if no command args are provided, display all entries in portfolio
                            int[] allCoinIdsInPortfolio = PortfolioManager.Instance.CoinIds;
                            coinIdsWithPortfolioEntry.AddRange(allCoinIdsInPortfolio);
                        }
                        else // command args are provided
                        {
                            // fetch coin ids corresponding to coin names / symbols
                            int[] coinIds = CoinListingManager.Instance.FetchCoinIds(commandArguments);

                            // get coin ids with initialized ticker data
                            foreach (int coinId in coinIds)
                            {
                                if (PortfolioManager.Instance.IsInPortfolio(coinId))
                                {
                                    coinIdsWithPortfolioEntry.Add(coinId);
                                }
                                else
                                {
                                    string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);
                                    coinNamesWithoutPortfolioEntry.Add(coinName);
                                }
                            }
                        }

                        if (coinIdsWithPortfolioEntry.Count > 0)
                        {
                            // print coin portfolio entry display table containing portfolio entries corresponding
                            // to fetched coin ids
                            string portfolioEntryDisplayTableString =
                                PortfolioManager.Instance.GetPortfolioEntryDisplayTableString(
                                    coinIdsWithPortfolioEntry.ToArray());
                            ConsoleIOManager.Instance.PrintData(portfolioEntryDisplayTableString);
                        }

                        // if data for coin ids which don't have corresponding porfolio entries was requested, 
                        // display an appropriate message to user
                        if (coinNamesWithoutPortfolioEntry.Count > 0)
                        {
                            string errorMessage = StringUtils.Append(
                                "Following coin(s) are not in portfolio: ",
                                ", ",
                                coinNamesWithoutPortfolioEntry.ToArray())
                                + ".";
                            ConsoleIOManager.Instance.LogError(errorMessage);
                        }
                    }
                    catch (CoinListingManager.NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                }
            }

            /// <summary>
            /// <para>
            /// adds specified coin to portfolio.
            /// </para>
            /// <para>
            /// command syntax: portfolio add [coin name / symbol]
            /// </para>
            /// </summary>
            private class PortfolioAddCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;

                // command sub-prefix
                private const string SUBPREFIX = "add";

                internal PortfolioAddCommand()
                    : base(SUBPREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                /// <summary>
                /// executes command with given <paramref name="commandArguments"/>.
                /// </summary>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        // get coin id by name or symbol
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // add coin to portfolio
                        PortfolioManager.Instance.AddCoin(coinId);

                        // coin successfully added to portfolio
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogNoticeFormat(
                            false,
                            "'{0}' successfully added to portfolio.",                       
                            coinName);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                    catch (CoinAlreadyInPortfolioException coinAlreadyInPortfolioException)
                    {
                        // coin id is already in portfolio
                        int coinId = coinAlreadyInPortfolioException.CoinId;
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's already an entry in portfolio for '{0}'.",                  
                            coinName);
                    }
                }
            }

            /// <summary>
            /// <para>
            /// removes specified coin from portfolio.
            /// </para>
            /// <para>
            /// command syntax: portfolio remove [coin name / symbol]
            /// </para>
            /// </summary>
            private class PortfolioRemoveCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;

                // command sub-prefix
                private const string SUB_PREFIX = "remove";

                internal PortfolioRemoveCommand()
                    : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                /// <summary>
                /// executes command with given <paramref name="commandArguments"/>.
                /// </summary>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        // get coin id by name or symbol
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // remove portfolio entry corresponding to coin id from portfolio
                        PortfolioManager.Instance.RemoveCoin(coinId);

                        // coin successfully removed from portfolio
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogNoticeFormat(
                            false,
                            "'{0}' successfully removed from portfolio.",                          
                            coinName);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                    catch (CoinNotInPortfolioException coinNotInPortfolioException)
                    {
                        // coin id corresponding to given name / symbol does not exist in portfolio manager
                        int coinId = coinNotInPortfolioException.CoinId;
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's no entry in portfolio manager for '{0}'.",            
                            coinName);
                    }
                }
            }

            /// <summary>
            /// <para>
            /// buys specified amount of specified coin, for a specified price per coin.
            /// </para>
            /// <para>
            /// command syntax: portfolio buy [coin name / symbol] [buy amount] [buy price per coin]
            /// </para>
            /// </summary>
            private class PortfolioBuyCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 3;
                private const int MAX_NUMBER_OF_ARGUMENTS = 3;

                // command sub-prefix
                private const string SUB_PREFIX = "buy";

                internal PortfolioBuyCommand()
                    : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                /// <summary>
                /// executes command with given <paramref name="commandArguments"/>.
                /// </summary>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }
                    try
                    {
                        // price coin name or symbol from command argument 0
                        string coinNameOrSymbol = commandArguments[0];

                        // parse buy amount from command argument 1
                        bool buyAmountParseResult = NumberUtils.TryParseDouble(
                            commandArguments[1],
                            out double buyAmount,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);

                        // price buy price from command argument 2
                        bool buyPriceParseResult = NumberUtils.TryParseDouble(
                            commandArguments[2],
                            out double buyPricePerCoin,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);

                        if(!buyAmountParseResult || !buyPriceParseResult)
                        {
                            // user entered a non-numeric or out-of-bounds value as buy price or buy amount
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                "Invalid format: buy price and amount must be numeric values larger than {0}" +
                                " and smaller than {1}.",             
                                0,
                                PortfolioManager.MaxNumericalValueAllowed);

                            return;
                        }

                        // get coin id by name or symbol
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // get coin name
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        // current timestamp
                        long unixTimestamp = DateTimeUtils.GetUnixTimestamp();

                        // check if portfolio has an entry with specified id
                        if(!PortfolioManager.Instance.IsInPortfolio(coinId))
                        {
                            // portfolio has no entry with specified id
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                "There's no entry in portfolio manager for '{0}'.", 
                                coinName);

                            // ask user if they want to create a new portfolio entry
                            string promptMessage = "Create new entry?";
                            bool createNewPortfolioEntry = 
                                ConsoleIOManager.Instance.ShowConfirmationDialog(promptMessage);

                            if(createNewPortfolioEntry) // user chose to create a new portfolio entry
                            {
                                // create a new entry before proceeding to execute buy command 
                                PortfolioManager.Instance.AddCoin(coinId);

                                ConsoleIOManager.Instance.LogNoticeFormat(
                                    false,
                                    "'{0}' successfully added to portfolio.",
                                    coinName);
                            }
                            else // user chose not to create a new portfolio entry
                            {
                                ConsoleIOManager.Instance.LogNotice("Purchase cancelled.");
                                return;
                            }
                        }

                        // execute buy command
                        PortfolioManager.Instance.BuyCoin(coinId, buyAmount, buyPricePerCoin, unixTimestamp);

                        // purchase performed successfully
                        ConsoleIOManager.Instance.LogNoticeFormat(
                            false,
                            "Successfully purchased {0} {1} for {2}$ each.",
                            buyAmount,
                            coinName,
                            buyPricePerCoin);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                }
            }

            /// <summary>
            /// <para>
            /// sells specified amount of specified coin, for a specified price per coin.
            /// </para>
            /// <para>
            /// command syntax: portfolio sell [coin name / symbol] [sell amount] [sell price per coin]
            /// </para>
            /// </summary>
            private class PortfolioSellCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 3;
                private const int MAX_NUMBER_OF_ARGUMENTS = 3;

                private const string SUB_PREFIX = "sell";

                internal PortfolioSellCommand()
                    : base(SUB_PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                /// <summary>
                /// executes command with given <paramref name="commandArguments"/>.
                /// </summary>
                /// <param name="commandArguments"></param>
                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleWrongNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }
                    try
                    {
                        // price coin name or symbol from command argument 0
                        string coinNameOrSymbol = commandArguments[0];

                        // parse sell amount from command argument 1
                        bool sellAmountParseResult = Utils.NumberUtils.TryParseDouble(
                            commandArguments[1],
                            out double sellAmount,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);

                        // parse sell price from command argument 2
                        bool sellPriceParseResult = Utils.NumberUtils.TryParseDouble(
                            commandArguments[2],
                            out double sellPrice,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);

                        if (!sellAmountParseResult || !sellPriceParseResult)
                        {
                            // user entered a non-numeric or out-of-bounds value as buy price or buy amount
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                "Invalid format: buy price and amount must be numeric values larget than {0}" +
                                " and smaller than {1}.",
                                0,
                                PortfolioManager.MaxNumericalValueAllowed);

                            return;
                        }

                        // get coin id by name or symbol
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // get coin name & symbol
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);
                        string coinSymbol = CoinListingManager.Instance.GetCoinSymbolById(coinId);

                        // current timestamp
                        long unixTimestamp = DateTimeUtils.GetUnixTimestamp();

                        // check if portfolio has an entry with specified id
                        if (!PortfolioManager.Instance.IsInPortfolio(coinId))
                        {
                            // portfolio has no entry with specified id
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                "There's no entry in portfolio manager for '{0}'.",
                                coinName);

                            return;
                        }

                        // check if there are enough funds for sell operation
                        double coinHoldings = PortfolioManager.Instance.GetCoinHoldings(coinId);

                        if(coinHoldings < sellAmount) // not enough funds to sell requested amount
                        {
                            ConsoleIOManager.Instance.LogErrorFormat(
                                false,
                                "Not enough funds for sell operation. {0} holdings: {1} {2}.",                     
                                coinName,
                                coinHoldings,
                                coinSymbol);
                            return;
                        }

                        // execute sell command
                        PortfolioManager.Instance.SellCoin(coinId, sellAmount, sellPrice, unixTimestamp);

                        // purchase performed successfully
                        ConsoleIOManager.Instance.LogNoticeFormat(
                            false,
                            "Successfully purchased {0} '{1}' for {2}$ each.",
                            sellAmount,
                            coinName,
                            sellPrice);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                }
            }

            private const string COMMAND_TYPE = "Portfolio";

            public PortfolioCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(
                    new PortfolioViewCommand(),
                    new PortfolioAddCommand(),
                    new PortfolioRemoveCommand(),
                    new PortfolioBuyCommand(),
                    new PortfolioSellCommand());
            }

            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}