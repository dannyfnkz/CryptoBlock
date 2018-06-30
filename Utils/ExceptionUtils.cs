using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains methods which provide additional utility for <see cref="System.Exception"/>.
        /// </summary>
        public static class ExceptionUtils
        {

            public static Exception ToException(string exceptionMessage, AggregateException aggregateException)
            {
                Exception exception;

                string aggregateExceptionString = GetUnifiedAggregateExceptionMessage(aggregateException);
                string unifiedExceptionMessage = string.Format(
                    "{0}{1}{2}",
                    exceptionMessage,
                    Environment.NewLine,
                    aggregateExceptionString);
                    

                exception = new Exception(unifiedExceptionMessage);

                return exception;
            }

            public static Exception ToException(AggregateException aggregateException)
            {
                return ToException(string.Empty, aggregateException);
            }

            /// <summary>
            /// returns a string consisting of the exception messages of <paramref name="aggregateException"/>'s
            /// inner exceptions.
            /// </summary>
            /// <remarks>
            /// exception message of each of <paramref name="aggregateException"/>'s inner exceptions is constructed
            /// recursively and contain the messages of all exceptions in the respective inner exception chain.
            /// </remarks>
            /// <param name="aggregateException"></param>
            /// <returns></returns>
            public static string GetUnifiedAggregateExceptionMessage(AggregateException aggregateException)
            {
                // append all inner exceptions in aggregateException into one error message
                StringBuilder exceptionMessageStringBuilder = new StringBuilder();

                // append individual inner exception
                for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
                {
                    Exception currentInnerException = aggregateException.InnerExceptions[i];

                    exceptionMessageStringBuilder.Append(i + 1);
                    exceptionMessageStringBuilder.Append(". ");

                    string innerExceptionMessage = GetExceptionMessageString(currentInnerException);
                    exceptionMessageStringBuilder.Append(innerExceptionMessage);

                    if (i < aggregateException.InnerExceptions.Count - 1)
                    {
                        exceptionMessageStringBuilder.Append(Environment.NewLine);
                    }
                }

                return exceptionMessageStringBuilder.ToString();
            }

            /// <summary>
            /// returns a string consisting of the exception message of <paramref name="exception"/>
            /// appended to the exception message of its inner exception.
            /// </summary>
            /// <remarks>
            /// uses <see cref="appendExceptionMessageString(Exception, StringBuilder, int)"/>.
            /// </remarks>
            /// <param name="exception"></param>
            /// <returns>
            ///  a string consisting of exception message of <paramref name="exception"/>
            ///  and its inner exception.
            /// </returns>
            public static string GetExceptionMessageString(Exception exception)
            {
                StringBuilder exceptionMessageStringBuilder = new StringBuilder();

                appendExceptionMessageString(exception, exceptionMessageStringBuilder, 0);

                return exceptionMessageStringBuilder.ToString();
            }

            /// <summary>
            /// appends exception message of <paramref name="exception"/>to specified exception message
            /// string builder, indentated by a number of TABs specified by <paramref name="exceptionInnerLevel"/>.
            /// in addition, if <paramref name="exception"/>has an inner exception, appends its exception message as
            /// well.
            /// </summary>
            /// <remarks>
            /// exception message of <paramref name="exception"/>'s inner exception is constructed recursively
            /// so that it contains the messages of all exceptions in the inner exception chain.
            /// </remarks>
            /// <param name="exception"></param>
            /// <param name="exceptionMessageStringBuilder"></param>
            /// <param name="exceptionInnerLevel"></param>
            private static void appendExceptionMessageString(
                Exception exception,
                StringBuilder exceptionMessageStringBuilder,
                int exceptionInnerLevel)
            {

                // append exception message lines with indentation
                String[] exceptionMessageLines = StringUtils.Split(exception.Message, Environment.NewLine);

                for(int i = 0; i < exceptionMessageLines.Length; i++)
                {
                    string exceptionMessageLine = exceptionMessageLines[i];

                    // add indentation corresponding to exception inner level
                    StringUtils.Append(exceptionMessageStringBuilder, StringUtils.TAB_STRING, exceptionInnerLevel);
                    exceptionMessageStringBuilder.Append(exceptionMessageLine);

                    if(i < exceptionMessageLines.Length - 1)
                    {
                        exceptionMessageStringBuilder.Append(Environment.NewLine);
                    }
                }
               
                // append inner exception if exists
                if (exception.InnerException != null)
                {
                    exceptionMessageStringBuilder.Append(Environment.NewLine);

                    appendExceptionMessageString(
                        exception.InnerException,
                        exceptionMessageStringBuilder,
                        exceptionInnerLevel + 1);
                }
            }
        }
    }
}

