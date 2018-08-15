using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.IOManagement;
using CryptoBlock.PortfolioManagement.Transactions;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement.Commands.TransactionCommands
    {
        internal abstract class PortfolioTransactionCommand<T> : PortfolioCommand where T : Transaction
        {
            private class OddNumberOfArgumentsCommandArgumentConstraint : ICommandArgumentConstraint
            {
                bool ICommandArgumentConstraint.IsValid(string[] commandArgumentArray)
                {
                    return NumberUtils.IsOdd(commandArgumentArray.Length);
                }

                void ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[] commandArgumentArray)
                {
                    string errorMessage = "Invalid format: number of arguments must be odd.";
                    ConsoleIOManager.Instance.LogError(errorMessage);
                }
            }

            private const int MIN_NUMBER_OF_TRANSACTIONS = 1;
            private const int MAX_NUMBER_OF_TRANSACTIONS = 5;
            private const int MIN_NUMBER_OF_ARGUMENTS = 1 + 2 * MIN_NUMBER_OF_TRANSACTIONS;
            private const int MAX_NUMBER_OF_ARGUMENTS = 1 + 2 * MAX_NUMBER_OF_TRANSACTIONS;

            private static readonly Dictionary<Type, string> transactionTypeToSubPrefix
                = new Dictionary<Type, string>()
                {
                        { typeof(BuyTransaction), "buy" },
                        { typeof(SellTransaction), "sell" }
                };


            internal PortfolioTransactionCommand()
                : base(
                      transactionTypeToSubPrefix[typeof(T)],
                      MIN_NUMBER_OF_ARGUMENTS,
                      MAX_NUMBER_OF_ARGUMENTS)
            {
                base.commandArgumentConstraintList.Add(
                    new OddNumberOfArgumentsCommandArgumentConstraint());
            }


            protected static T[] tryParseTransactionArray(
                string[] commandArguments,
                long coinId,
                long unixTimestamp,
                out bool arrayParseSuccesss)
            {
                // first argument is coin name / symbol, rest are consecutive pairs of 
                // (buyAmount, pricePerCoin)
                T[] transactions = new T[(commandArguments.Length - 1) / 2];

                arrayParseSuccesss = false;

                for (int i = 1; i < commandArguments.Length; i += 2)
                {
                    string amountArgument = commandArguments[i];
                    string pricePerCoinArgument = commandArguments[i + 1];

                    T transaction = tryParseTransaction(
                        amountArgument,
                        pricePerCoinArgument,
                        coinId,
                        unixTimestamp,
                        out bool operationParseResult);

                    if (operationParseResult)
                    {
                        transactions[i / 2] = transaction;
                    }
                    else
                    {
                        return null;
                    }
                }

                arrayParseSuccesss = true;

                return transactions;
            }

            private static T tryParseTransaction(
                string amountArgument,
                string priceArgument,
                long coinId,
                long unixTimestamp,
                out bool parseResult)
            {
                Transaction transaction;

                // parse buy amount from buyAmountArgument
                bool amountParseResult = NumberUtils.TryParseDouble(
                    amountArgument,
                    out double amount,
                    0,
                    PortfolioManager.MaxNumericalValueAllowed);

                // price buy price from buyPriceArgument
                bool priceParseResult = NumberUtils.TryParseDouble(
                    priceArgument,
                    out double pricePerCoin,
                    0,
                    PortfolioManager.MaxNumericalValueAllowed);

                if (amountParseResult && priceParseResult)
                {
                    transaction = typeof(T) == typeof(BuyTransaction)
                        ? (Transaction)(new BuyTransaction(coinId, amount, pricePerCoin, unixTimestamp))
                        : (Transaction)(new SellTransaction(coinId, amount, pricePerCoin, unixTimestamp));

                    parseResult = true;
                }
                else
                {
                    // user entered a non-numeric or out-of-bounds value as buy price or buy amount
                    string errorMessage = string.Format(
                        "Invalid format: price and amount must be numeric values larger than {0}" +
                        " and smaller than {1}.",
                        0,
                        PortfolioManager.MaxNumericalValueAllowed);
                    ConsoleIOManager.Instance.LogError(errorMessage);

                    transaction = null;
                    parseResult = false;
                }

                return (T)transaction;
            }

        }
    }
}

