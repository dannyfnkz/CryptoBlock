using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Strings
    {
        public static class StringUtils
        {
            private const string TAB_STRING = "\t";
            private const char TAB_CHAR = '\t';

            public static string TabString
            {
                get { return TAB_STRING; }
            }

            public static char TabChar
            {
                get { return TAB_CHAR; }
            }

            /// <summary>
            /// returns a string representation of <paramref name="obj"/>,
            /// including all properties and their
            /// corrosponding values.
            /// </summary>
            /// <remarks>
            /// uses <see cref="System.Reflection"/>.
            /// </remarks>
            /// <param name="obj"></param>
            /// <returns>
            /// a string representation of an object in the format:  class namespace.name [field1=value1, field2=value2, ...]
            /// </returns>
            public static string GetPropertyStringRepresentation(object obj)
            {
                StringBuilder stringBuilder = new StringBuilder();

                Type objectType = obj.GetType();
                PropertyInfo[] propertyInfoList = objectType.GetProperties();

                // append object namespace.name
                stringBuilder.Append("class ");
                stringBuilder.Append(objectType.Namespace);
                stringBuilder.Append(".");
                stringBuilder.Append(objectType.Name);
                stringBuilder.Append(" [");

                // append object properties
                for (int i = 0; i < propertyInfoList.Length; i++)
                {
                    PropertyInfo propertyInfo = propertyInfoList[i];

                    stringBuilder.Append(propertyInfo.Name);
                    stringBuilder.Append("=");
                    stringBuilder.Append(propertyInfo.GetValue(obj));

                    if (i < propertyInfoList.Length - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append("]");

                return stringBuilder.ToString();
            }

            /// <summary>
            /// appends strings from <paramref name="appendants"/> at the the end of <paramref name="str"/>
            /// in their original order,
            /// every two strings seperated by <paramref name="seperator"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="seperator"></param>
            /// <param name="appendants"></param>
            /// <returns>
            /// a string containing the result of appending strings from <paramref name="appendants"/>
            /// at the the end of <paramref name="str"/> in their original order,
            /// every two strings seperated by <paramref name="seperator"/>
            /// </returns>
            public static string Append(this string str, string seperator, params string[] appendants)
            {
                StringBuilder stringBuilder = new StringBuilder(str);

                for (int i = 0; i < appendants.Length; i++)
                {
                    stringBuilder.Append(appendants[i]);

                    if (i < appendants.Length - 1) // append seperator if not at last appendant
                    {
                        stringBuilder.Append(seperator);
                    }
                }

                return stringBuilder.ToString();
            }

            /// <summary>
            /// if <paramref name="nullable"/> != null, returns its string representation;
            /// else, returns <paramref name="defaultStringValue"/>.
            /// </summary>
            /// <typeparam name="T">A type which inherits from ValueType</typeparam>
            /// <param name="nullable"></param>
            /// <param name="defaultStringValue"></param>
            /// <returns>
            /// if <paramref name="nullable"/> != null, <paramref name="nullable"/>.ToString()
            /// else, <paramref name="defaultStringValue"/>
            /// </returns>
            public static string ToString<T>(T? nullable, string defaultStringValue) where T : struct
            {
                return nullable.HasValue ? nullable.Value.ToString() : defaultStringValue;
            }

            /// <summary>
            /// if <paramref name="obj"/> != null, returns its string representation;
            /// else, returns <paramref name="defaultStringValue"/>.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="defaultStringValue"></param>
            /// <returns>
            /// if <paramref name="obj"/> != null, <paramref name="obj"/>.ToString()
            /// else, <paramref name="defaultStringValue"/>
            /// </returns>
            public static string ToString(object obj, string defaultStringValue)
            {
                return obj != null ? obj.ToString() : defaultStringValue;
            }

            /// <summary>
            /// appends <paramref name="appendant"/>to <paramref name="stringBuilder"/>
            /// <paramref name="numberOfTimes"/> times.
            /// </summary>
            /// <remarks>
            /// this method uses and extends <see cref="StringBuilder.Append(string)"/> to allow it to
            /// receive <c>string, int</c> as arguments.
            /// </remarks>
            /// <param name="stringBuilder"></param>
            /// <param name="appendant"></param>
            /// <param name="numberOfTimes">default value is 1.</param>
            public static void Append(
                StringBuilder stringBuilder,
                string appendant,
                int numberOfTimes = 1)
            {
                for (int i = 0; i < numberOfTimes; i++)
                {
                    stringBuilder.Append(appendant);
                }
            }

            public static string PascalCaseToCamelCase(string pascalCaseString)
            {
                // get char array corresponding to pascalCaseString 
                char[] pascalCaseStringCharArray = pascalCaseString.ToCharArray();

                // make first letter lowercase
                pascalCaseStringCharArray[0] = char.ToLower(pascalCaseStringCharArray[0]);

                string camelCaseString = new string(pascalCaseStringCharArray);

                return camelCaseString;
            }

            public static string PascalCaseToSnakeCase(string pascalCaseString)
            {
                if (pascalCaseString == string.Empty)
                {
                    return pascalCaseString;
                }

                if (!char.IsUpper(pascalCaseString[0]))
                {
                    throw new ArgumentException(
                        "a PascalCase string must begin with an uppercase letter.",
                        "pascalCaseString");
                }

                // count number of uppercase letters in pascalCaseString
                int numberOfUpperCaseLetters = pascalCaseString.Count(c => char.IsUpper(c));

                // add one underscore for each uppercase letter, except first letter in string
                char[] snakeCaseCharArray =
                    new char[pascalCaseString.Length + numberOfUpperCaseLetters - 1];

                int snakeCaseCharArrayIndex = 0;

                foreach (char pascalCaseStringCharacter in pascalCaseString)
                {
                    // uppercase character in pascalCaseString
                    if (char.IsUpper(pascalCaseStringCharacter))
                    {
                        // not first character in pascalCaseString
                        if (snakeCaseCharArrayIndex != 0)
                        {
                            // add underscore before an inner uppercase letter
                            snakeCaseCharArray[snakeCaseCharArrayIndex++] = '_';
                        }

                        // convert character to lowercase
                        snakeCaseCharArray[snakeCaseCharArrayIndex++] =
                            char.ToLower(pascalCaseStringCharacter);
                    }
                    else // lowercase character in pascalCaseString
                    {
                        // copy to snakeCaseCharArray as is
                        snakeCaseCharArray[snakeCaseCharArrayIndex++] = pascalCaseStringCharacter;
                    }
                }

                return new string(snakeCaseCharArray);
            }

            /// <summary>
            /// asserts that all string lengths in <paramref name="stringLengths"/> are non-negative.
            /// </summary>
            /// <param name="stringLengths"></param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// thrown if a string length in <paramref name="stringLengths"/> is negative
            /// </exception>
            internal static void assertValidStringLengths(params int[] stringLengths)
            {
                foreach (int stringLength in stringLengths)
                {
                    if (stringLength < 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "string length must be non-negative.",
                            (Exception)null);
                    }
                }
            }
        }
    }
}