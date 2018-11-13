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
            /// command arguments, when devided by three,
            /// gives a remainder of one.
            /// </summary>
            private class OneRemainderInDivisionByThreeNumberOfArgumentsCommandArgumentConstraint 
                : ICommandArgumentConstraint
            {
                /// <summary>
                /// returns whether number of command arguments, when devided by three,
                /// gives a remainder of one.
                /// </summary>
                /// <param name="commandArgumentArray"></param>
                /// <returns>
                /// true if number of command arguments, when devided by three,
                /// gives a remainder of one,
                /// else false
                /// </returns>
                bool ICommandArgumentConstraint.IsValid(string[] commandArgumentArray)
                {
                    return commandArgumentArray.Length % 3 == 1;
                }

                /// <summary>
                /// logs an error message if number of command arguments is invalid.
                /// </summary>
                /// <param name="commandArgumentArray"></param>
                void ICommandArgumentConstraint.OnInvalidCommandArgumentArray(string[] commandArgumentArray)
                {
                    string errorMessage = "Invalid format: number of arguments must be 3x + 1, " +
                        "where x is the number of operations.";
                    ConsoleIOManager.Instance.LogError(
                        errorMessage,
                        ConsoleIOManager.eOutputReportType.CommandExecution);
                }
            }

            // min number of buy / sell operations on specified coin
            private const int MIN_NUMBER_OF_OPERATIONS = 1;

            // max number of buy / sell operations on specified coin
            private const int MAX_NUMBER_OF_OPERATIONS = 5;

            // number of arguments (e.g amount, pricePerCoin) for each operation
            private const int NUMBER_OF_ARGUMENTS_PER_OPERATION = 3;

            private const int MIN_NUMBER_OF_ARGUMENTS = 
                1 + NUMBER_OF_ARGUMENTS_PER_OPERATION * MIN_NUMBER_OF_OPERATIONS;
            private const int MAX_NUMBER_OF_ARGUMENTS =
                1 + NUMBER_OF_ARGUMENTS_PER_OPERATION * MAX_NUMBER_OF_OPERATIONS;

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
                    new OneRemainderInDivisionByThreeNumberOfArgumentsCommandArgumentConstraint());
            }

            /// <summary>
            /// <para>
            /// tries parsing a <typeparamref name="T"/> (derived from <see cref="Transaction"/>)
            /// array from <paramref name="commandArguments"/> and specified <paramref name="coinId"/>
            /// and <paramref name="unixTimestamp"/>.
            /// </para>
            /// <para>
            /// <paramref name="commandArguments"/> array is expected to be in format:
            /// 0: [coin name / symbol] 1: [amount] [pricePerCoin] [exchangeName] 
            /// 2: [amount] [pricePerCoin] [exchangeName] 
            /// ...
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
                // first argument is coin name / symbol, rest are consecutive triplets of 
                // (amount, pricePerCoin, exchangeName)
                T[] transactions = new T[(commandArguments.Length - 1) / NUMBER_OF_ARGUMENTS_PER_OPERATION];

                transactionArrayParseSuccesss = false;

                // go through all pairs of arguments, starting from 1'th argument
                for (int i = 1; i < commandArguments.Length; i += NUMBER_OF_ARGUMENTS_PER_OPERATION)
                {
                    // try parsing operation amount, pricePerCoin and exchangeName for argument pair
                    string amountArgument = commandArguments[i];
                    string pricePerCoinArgument = commandArguments[i + 1];
                    string exchangeName = commandArguments[i + 2];

                    // try parsing Transaction from specified amountArgument and pricePerCoinArgument
                    T transaction = tryParseTransaction(
                        amountArgument,
                        pricePerCoinArgument,
                        coinId,
                        exchangeName,
                        unixTimestamp,
                        out bool transactionParseResult);

                    if (transactionParseResult) // transaction parse success
                    {
                        transactions[i / NUMBER_OF_ARGUMENTS_PER_OPERATION] = transaction;
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
            /// from specified parameters.
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
            /// <param name="exchangeName"
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
                string exchangeName,
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
                        ? (Transaction)(new BuyTransaction(
                            coinId,
                            amount, 
                            pricePerCoin,
                            exchangeName,
                            unixTimestamp))
                        : (Transaction)(new SellTransaction(
                            coinId,
                            amount, 
                            pricePerCoin,
                            exchangeName,
                            unixTimestamp));

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

