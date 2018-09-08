using CryptoBlock.Utils.Strings;
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
        /// contains utilty methods from <see cref="Enum"/>.
        /// </summary>
        public static class EnumUtils
        {
            /// <summary>
            /// thrown in case an <see cref="Enum"/> parse fails.
            /// </summary>
            public class EnumParseException : Exception
            {
                private readonly string enumValue;

                public EnumParseException(string enumValue, Exception innerException)
                    : base(formatExceptionMessage(enumValue), innerException)
                {
                    this.enumValue = enumValue;
                }

                public string EnumValue
                {
                    get { return enumValue; }
                }

                private static string formatExceptionMessage(string enumValue)
                {
                    return string.Format(
                        "Parsing of Enum from string value '{0}' failed.'",
                        enumValue);
                }
            }

            /// <summary>
            /// returns an <see cref="Enum"/> parsed from <paramref name="value"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            /// <returns>
            /// <see cref="Enum"/> parsed from <paramref name="value"/>
            /// </returns>
            /// <exception cref="EnumParseException">
            /// thrown if an exception occurs while trying to parse enum
            /// </exception>
            public static T ParseEnum<T>(string value)
            {
                try
                {
                    T parsedEnum = (T)Enum.Parse(typeof(T), value, true);
                    return parsedEnum;
                }
                catch(Exception exception)
                {
                    throw new EnumParseException(value, exception);
                }
            }

            /// <summary>
            /// returns a copy of <paramref name="camelCaseString"/>, converted into
            /// enum value name format.
            /// </summary>
            /// <remarks>
            /// enum value name format is PascalCase.
            /// </remarks>
            /// <seealso cref="StringUtils.CamelCaseToPascalCase(string)"/>
            /// <param name="camelCaseString"></param>
            /// <returns>
            /// copy of <paramref name="camelCaseString"/>, converted into
            /// enum value name format.
            /// </returns>
            public static string camelCaseStringToEnumValueNameFormat(string camelCaseString)
            {
                return StringUtils.CamelCaseToPascalCase(camelCaseString);
            }
        }
    }
}