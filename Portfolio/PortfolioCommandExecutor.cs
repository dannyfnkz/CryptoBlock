using CryptoBlock.CommandHandling;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.PortfolioManagement.PortfolioManager;
using static CryptoBlock.ServerDataManagement.CoinListingManager;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class PortfolioCommandExecutor : CommandExecutor
        {
            private abstract class PortfolioCommand : Command
            {
                private const string PREFIX = "portfolio";

                internal PortfolioCommand(string subPrefix, int minNumberOfArguments, int maxNumberOfArguments)
                    : base(formatPrefix(subPrefix), minNumberOfArguments, maxNumberOfArguments)
                {

                }

                private static string formatPrefix(string subPrefix)
                {
                    return PREFIX + " " + subPrefix;
                }
            }

            private class PortfolioViewCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;
                private const string PREFIX = "view";

                internal PortfolioViewCommand()
                    : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }

                    string coinNameOrSymbol = commandArguments[0];

                    try
                    {
                        // get coin id by name or symbol
                        int coinId = CoinListingManager.Instance.GetCoinIdByNameOrSymbol(coinNameOrSymbol);

                        // print portfolio entry display table string
                        string portfolioEntryDisplayTableString =
                            PortfolioManager.Instance.GetPortfolioEntryDisplayTableString(coinId);
                        ConsoleIOManager.Instance.PrintData(portfolioEntryDisplayTableString);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                    catch (CoinIdNotInPortfolioException coinIdNotInPortfolioManagerException)
                    {
                        // coin id corresponding to given name / symbol does not exist in portfolio manager
                        int coinId = coinIdNotInPortfolioManagerException.CoinId;
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's no entry in portfolio manager for '{0}'.",                           
                            coinName);
                    }
                }
            }

            private class PortfolioAddCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;
                private const string PREFIX = "add";

                internal PortfolioAddCommand()
                    : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

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
                        PortfolioManager.Instance.CreatePortfolioEntry(coinId);

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
                    catch (CoinIdAlreadyInPortfolioException coinIdAlreadyInPortfolioManagerException)
                    {
                        // coin id is already in portfolio
                        int coinId = coinIdAlreadyInPortfolioManagerException.CoinId;
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's already an entry in portfolio for '{0}'.",                  
                            coinName);
                    }
                }
            }

            private class PortfolioRemoveCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 1;
                private const int MAX_NUMBER_OF_ARGUMENTS = 1;
                private const string PREFIX = "remove";

                internal PortfolioRemoveCommand()
                    : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

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
                        PortfolioManager.Instance.RemovePortfolioEntry(coinId);

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
                    catch (CoinIdNotInPortfolioException coinIdNotInPortfolioManagerException)
                    {
                        // coin id corresponding to given name / symbol does not exist in portfolio manager
                        int coinId = coinIdNotInPortfolioManagerException.CoinId;
                        string coinName = CoinListingManager.Instance.GetCoinNameById(coinId);

                        ConsoleIOManager.Instance.LogErrorFormat(
                            false,
                            "There's no entry in portfolio manager for '{0}'.",            
                            coinName);
                    }
                }
            }

            private class PortfolioBuyCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 3;
                private const int MAX_NUMBER_OF_ARGUMENTS = 3;
                private const string PREFIX = "buy";

                internal PortfolioBuyCommand()
                    : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }
                    try
                    {
                        string coinNameOrSymbol = commandArguments[0];

                        bool buyAmountParseResult = Utils.NumberUtils.TryParseDouble(
                            commandArguments[1],
                            out double buyAmount,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);
                        bool buyPriceParseResult = Utils.NumberUtils.TryParseDouble(
                            commandArguments[2],
                            out double buyPrice,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);

                        if(!buyAmountParseResult || !buyPriceParseResult)
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
                                PortfolioManager.Instance.CreatePortfolioEntry(coinId);

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
                        PortfolioManager.Instance.BuyCoin(coinId, buyAmount, buyPrice, unixTimestamp);

                        // purchase performed successfully
                        ConsoleIOManager.Instance.LogNoticeFormat(
                            false,
                            "Successfully purchased {0} {1} for {2}$ each.",
                            buyAmount,
                            coinName,
                            buyPrice);
                    }
                    catch (NoSuchCoinNameOrSymbolException noSuchCoinNameOrSymbolException)
                    {
                        // coin with specified name / symbol not found in listing repository
                        ConsoleIOManager.Instance.LogError(noSuchCoinNameOrSymbolException.Message);
                    }
                }
            }

            private class PortfolioSellCommand : PortfolioCommand
            {
                private const int MIN_NUMBER_OF_ARGUMENTS = 3;
                private const int MAX_NUMBER_OF_ARGUMENTS = 3;
                private const string PREFIX = "sell";

                internal PortfolioSellCommand()
                    : base(PREFIX, MIN_NUMBER_OF_ARGUMENTS, MAX_NUMBER_OF_ARGUMENTS)
                {

                }

                public override void ExecuteCommand(string[] commandArguments)
                {
                    // handle case where number of arguments is invalid
                    HandleInvalidNumberOfArguments(commandArguments, out bool invalidNumberOfArguments);

                    if (invalidNumberOfArguments)
                    {
                        return;
                    }
                    try
                    {
                        string coinNameOrSymbol = commandArguments[0];

                        bool sellAmountParseResult = Utils.NumberUtils.TryParseDouble(
                            commandArguments[1],
                            out double sellAmount,
                            0,
                            PortfolioManager.MaxNumericalValueAllowed);
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
                // populate commandPrefixToCommmand dictionary with (prefix, command) pairs
                AddPrefixToCommandPair(
                    new PortfolioViewCommand(),
                    new PortfolioAddCommand(),
                    new PortfolioRemoveCommand(),
                    new PortfolioBuyCommand(),
                    new PortfolioSellCommand());
            }

            protected override string GetCommandType()
            {
                return COMMAND_TYPE;
            }
        }
    }
}