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
        /// <summary>
        /// represents a <see cref="PortfolioCommand"/> which performs buy / sell operation(s)
        /// (based on specified <typeparamref name="T"/>) of specified coin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal abstract class PortfolioTransactionCommand<T> : PortfolioCommand where T : Transaction
        {
            /// <summary>
            /// represents a <see cref="ICommandArgumentConstraint"/> requiring that the number of 
            /// command arguments be odd.
            /// </summary>
            private class OddNumberOfArgumentsCommandArgumentConstraint : ICommandArgumentConstraint
            {
                /// <summary>
                /// returns whether number of command arguments is odd.
                /// </summary>
                /// <param name="commandArgumentArray"></param>
                /// <returns>
                /// true if number of command arguments is odd,
                /// else false
                /// </returns>
                bool ICommandArgumentConstraint.IsValid(string[] commandArgumentArray)
                {
                    return NumberUtils.IsOdd(commandArgumentArray.Length);
                }

                /// <summary>
                /// logs an error message if number of command arguments is invalid.
                /// </summary>
                /// <param name="commandArgumentArray"></param>
                void ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[] commandArgumentArray)
                {
                    string errorMessage = "Invalid format: number of arguments must be odd.";
                    ConsoleIOManager.Instance.LogError(
                        errorMessage,
                        ConsoleIOManager.eOutputReportType.CommandExecution);
                }
            }

            // min number of buy / sell operations on specified coin
            private const int MIN_NUMBER_OF_OPERATIONS = 1;

            // max number of buy / sell operations on specified coin
            private const int MAX_NUMBER_OF_OPERATIONS = 5;

            private const int MIN_NUMBER_OF_ARGUMENTS = 1 + 2 * MIN_NUMBER_OF_OPERATIONS;
            private const int MAX_NUMBER_OF_ARGUMENTS = 1 + 2 * MAX_NUMBER_OF_OPERATIONS;

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
                // add OddNumberOfArgumentsCommandArgumentConstraint to command constraint list
                base.commandArgumentConstraintList.Add(
                    new OddNumberOfArgumentsCommandArgumentConstraint());
            }

            /// <summary>
            /// <para>
            /// tries parsing a <typeparamref name="T"/> (derived from <see cref="Transaction"/>)
            /// array from <paramref name="commandArguments"/> and specified <paramref name="coinId"/>
            /// and <paramref name="unixTimestamp"/>.
            /// </para>
            /// <para>
            /// <paramref name="commandArguments"/> array is expected to be in format:
            /// 0: [coin name / symbol] 1: [amount] [pricePerCoin] 2: [amount] [pricePerCoin] ...
            /// </para>
            /// returns parsed array if parsing succeeded, or null otherwise
            /// </summary>
            /// <param name="commandArguments"></param>
            /// <param name="coinId"></param>
            /// <param name="unixTimestamp"></param>
            /// <param name="arrayParseSuccesss">whether parsing succeeded</param>
            /// <returns>
            /// parsed <typeparamref name="T"/> (derived from <see cref="Transaction"/>)
            /// array if parsing succeeded,
            /// else null
            /// </returns>
            protected static T[] tryParseTransactionArray(
                string[] commandArguments,
                long coinId,
                long unixTimestamp,
                out bool transactionArrayParseSuccesss)
            {
                // first argument is coin name / symbol, rest are consecutive pairs of 
                // (amount, pricePerCoin)
                T[] transactions = new T[(commandArguments.Length - 1) / 2];

                transactionArrayParseSuccesss = false;

                // go through all pairs of arguments, starting from 1'th argument
                for (int i = 1; i < commandArguments.Length; i += 2)
                {
                    // try parsing operation amount and pricePerCoin for argument pair
                    string amountArgument = commandArguments[i];
                    string pricePerCoinArgument = commandArguments[i + 1];

                    // try parsing Transaction from specified amountArgument and pricePerCoinArgument
                    T transaction = tryParseTransaction(
                        amountArgument,
                        pricePerCoinArgument,
                        coinId,
                        unixTimestamp,
                        out bool transactionParseResult);

                    if (transactionParseResult) // transaction parse success
                    {
                        transactions[i / 2] = transaction;
                    }
                    else // transaction parse failed
                    {
                        return null;
                    }
                }

                transactionArrayParseSuccesss = true;

                return transactions;
            }

            /// <summary>
            /// <para>
            /// tries parsing a <typeparamref name="T"/> (derived from <see cref="Transaction"/>)
            /// from specified <paramref name="amountArgument"/>, <paramref name="pricePerCoinArgument"/>,
            /// <paramref name="coinId"/>, and <paramref name="unixTimestamp"/>.
            /// </para>
            /// <para>
            /// returns the parsed <typeparamref name="T"/> if parsing succeeded,
            /// logs an error message and returns null otherwise.
            /// </para>
            /// </summary>
            /// <param name="amountArgument"></param>
            /// <param name="pricePerCoinArgument"></param>
            /// <param name="coinId"></param>
            /// <param name="unixTimestamp"></param>
            /// <param name="parseSuccess">whether parsing succeeded</param>
            /// <returns>
            /// parsed <typeparamref name="T"/> (derived from <see cref="Transaction"/>) if
            /// parsing succeeded,
            /// else null
            /// </returns>
            private static T tryParseTransaction(
                string amountArgument,
                string pricePerCoinArgument,
                long coinId,
                long unixTimestamp,
                out bool parseSuccess)
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
                    pricePerCoinArgument,
                    out double pricePerCoin,
                    0,
                    PortfolioManager.MaxNumericalValueAllowed);

                if (amountParseResult && priceParseResult)
                {
                    transaction = typeof(T) == typeof(BuyTransaction)
                        ? (Transaction)(new BuyTransaction(coinId, amount, pricePerCoin, unixTimestamp))
                        : (Transaction)(new SellTransaction(coinId, amount, pricePerCoin, unixTimestamp));

                    parseSuccess = true;
                }
                else
                {
                    // user entered a non-numeric or out-of-bounds value as buy price or buy amount
                    string errorMessage = string.Format(
                        "Invalid format: price and amount must be numeric values larger than {0}" +
                        " and smaller than {1}.",
                        0,
                        PortfolioManager.MaxNumericalValueAllowed);
                    ConsoleIOManager.Instance.LogError(
                        errorMessage,
                        ConsoleIOManager.eOutputReportType.CommandExecution);

                    transaction = null;
                    parseSuccess = false;
                }

                return (T)transaction;
            }

        }
    }
}

