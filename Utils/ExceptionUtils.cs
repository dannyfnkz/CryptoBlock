﻿using CryptoBlock.Utils.Strings;
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
            /// <summary>
            /// represents a method parameter.
            /// </summary>
            /// <example>
            /// in case where '3' is passed to method defined as void foo(char ch),
            /// then MethodParameter.Name = "ch" and MethodParameter.value = '3'.
            /// </example>
            public class MethodParameter
            {
                private readonly object value;
                private readonly string name;

                public MethodParameter(object value, string name)
                {
                    this.value = value;
                    this.name = name;
                }

                /// <summary>
                /// parameter value.
                /// </summary>
                public object Value
                {
                    get { return value; }
                }

                /// <summary>
                /// parameter name.
                /// </summary>
                public string Name
                {
                    get { return name; }
                }
            }
            /// <summary>
            /// converts <paramref name="aggregateException"/> into an <see cref="System.Exception"/>,
            /// having <paramref name="exceptionMessage"/> as part of its exception message.
            /// </summary>
            /// <remarks>
            /// conversion is achieved by merging <paramref name="exceptionMessage"/>
            /// and unified aggregate exception message into a single exception message,
            /// which then serves as the converted exception's message.
            /// </remarks>
            /// <seealso cref="GetUnifiedAggregateExceptionMessage(AggregateException)"/>
            /// <param name="exceptionMessage"></param>
            /// <param name="aggregateException"></param>
            /// <returns>
            /// converted <see cref="System.Exception"/>, 
            /// based on <paramref name="aggregateException"/> and having <paramref name="exceptionMessage"/>
            /// as its message
            /// </returns>
            public static Exception ToException(string exceptionMessage, AggregateException aggregateException)
            {
                Exception exception;

                // merge aggregate exception message with exceptionMessage provided as argument
                string aggregateExceptionMessage = GetUnifiedAggregateExceptionMessage(aggregateException);
                string unifiedExceptionMessage = string.Format(
                    "{0}{1}{2}",
                    exceptionMessage,
                    Environment.NewLine,
                    aggregateExceptionMessage);
                    
                // return exception containing merged message
                exception = new Exception(unifiedExceptionMessage);

                return exception;
            }

            /// <summary>
            /// converts <paramref name="aggregateException"/> into an <see cref="System.Exception"/>.
            /// </summary>
            /// <see cref="ToException(string, AggregateException)"/>
            /// <param name="aggregateException"></param>
            /// <returns>
            /// converted <see cref="System.Exception"/>, based on <paramref name="aggregateException"/>
            /// </returns>
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
                String[] exceptionMessageLines = exception.Message.Split(Environment.NewLine);

                for(int i = 0; i < exceptionMessageLines.Length; i++)
                {
                    string exceptionMessageLine = exceptionMessageLines[i];

                    // add indentation corresponding to exception inner level
                    StringUtils.Append(
                        exceptionMessageStringBuilder, 
                        StringUtils.TabString,
                        exceptionInnerLevel);
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

            /// <summary>
            /// asserts that all <see cref="MethodParameter"/> in <paramref name="methodParameters"/>
            /// are not null.
            /// </summary>
            /// <param name="methodParameters"></param>
            /// <exception cref="ArgumentNullException">
            /// thrown if a <see cref="MethodParameter"/> in <paramref name="methodParameters"/> is null
            /// </exception>
            public static void AssertMethodParametersNotNull(params MethodParameter[] methodParameters)
            {
                foreach(MethodParameter methodParameter in methodParameters)
                {
                    if(methodParameter.Value == null)
                    {
                        throw new ArgumentNullException(methodParameter.Name);
                    }
                }
            }
        }
    }
}

