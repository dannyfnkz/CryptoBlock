using System;
using System.Reflection;
using System.Text;
using System.Linq;
using static CryptoBlock.Utils.ExceptionUtils;

namespace CryptoBlock
{
    namespace Utils.Strings
    {
        /// <summary>
        /// string utility class.
        /// </summary>
        public static class StringExtensionMethods
        {
            /// <summary>
            /// splits <paramref name="str"/> into multiple strings, according to parameter list 
            /// <paramref name="parameters"/>.
            /// </summary>
            /// <remarks>
            /// this method uses and extends<see cref="String.Split(string[], StringSplitOptions)"/> 
            /// to allow it to receive <c>params string[], StringSplitOptions</c> as arguments.
            /// </remarks>
            /// <param name="str"></param>
            /// <param name="splitOptions"></param>
            /// <param name="parameters">an array of strings used to split <paramref name="str"/>.</param>
            /// <returns>
            /// an array containing the strings obtained from splitting <paramref name="str"/>
            /// </returns>
            public static string[] Split(
                this string str,
                StringSplitOptions splitOptions,
                params string[] parameters)
            {
                return str.Split(parameters, splitOptions);
            }

            /// <summary>
            /// splits <paramref name="str"/> into multiple strings, according to parameter list 
            /// <paramref name="parameters"/>.
            /// </summary>
            /// this method calls <see cref="Split(string, StringSplitOptions, string[])"/> with argument
            /// <c>StringSplitOptions.None</c>.
            /// <param name="str"></param>
            /// <param name="parameters">an array of strings used to split <paramref name="str"/>.</param>
            /// <returns>
            /// an array containing the strings obtained from splitting <paramref name="str"/>
            /// </returns>
            public static string[] Split(this string str, params string[] parameters)
            {
                return Split(str, StringSplitOptions.None, parameters);
            }

            /// <summary>
            /// returns a substring of <paramref name="str"/>,
            /// starting immediately after <paramref name="prefix"/>.
            /// if <paramref name="prefix"/> does not exist in <paramref name="str"/>,
            /// returns <paramref name="str"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefix"></param>
            /// <returns>
            /// <para>
            /// substring of <paramref name="str"/>, starting immediately after <paramref name="prefix"/>, 
            /// if <paramref name="prefix"/> exists in in <paramref name="str"/>.
            /// </para>
            /// <para>
            /// else, <paramref name="str"/>.
            /// </para>
            /// </returns>
            public static string GetSubstringAfterPrefix(this string str, string prefix)
            {
                return str.Substring(prefix.Length);
            }

            public static string CharactersAtIndicesToUpper(this string str, params int[] indices)
            {
                char[] stringCharArray = str.ToCharArray();

                for(int i = 0; i < indices.Length; i++)
                {
                    stringCharArray[i] = char.ToUpper(stringCharArray[i]);
                }

                return new string(stringCharArray);
            }

            public static string CharactersAtIndicesToLower(this string str, params int[] indices)
            {
                char[] stringCharArray = str.ToCharArray();

                for (int i = 0; i < indices.Length; i++)
                {
                    stringCharArray[i] = char.ToLower(stringCharArray[i]);
                }

                return new string(stringCharArray);
            }

            /// <summary>
            /// returns whether <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// true if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// else false
            /// </returns>
            public static bool StartsWith(this string str, params string[] prefixes)
            {
                foreach (string prefix in prefixes)
                {
                    if (str.StartsWith(prefix))
                    {
                        return true;
                    }
                }

                return false;
            }

            // if str starts with one of the prefixes in in params array, returns that prefix
            // else, returns null
            /// <summary>
            /// if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// returns that prefix; else returns null.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// returns that prefix
            /// else, returns null. 
            /// </returns>
            public static string GetPrefixIfStartsWith(this string str, params string[] prefixes)
            {
                foreach (string prefix in prefixes)
                {
                    if (str.StartsWith(prefix))
                    {
                        return prefix;
                    }
                }

                return null;
            }

            /// <summary>
            /// if <paramref name="inputString"/> is longer than <paramref name="maxStringLength"/>,
            /// shorten string to its first <paramref name="maxStringLength"/> characters,
            /// where the last <paramref name="cutSuffix"/>.Length characters are replaced by 
            /// <paramref name="cutSuffix"/>.
            /// </summary>
            /// <param name="inputString"></param>
            /// <param name="maxStringLength">maximum allowed string length</param>
            /// <param name="cutSuffix">appended to end of <paramref name="inputString"/> if it is shortened</param>
            /// <returns>
            /// inputString shortened to its first <paramref name="maxStringLength"/> -
            /// <paramref name="cutSuffix"/>.Length characters, with <paramref name="cutSuffix"/>
            /// appended to ending
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="ExceptionUtils.AssertMethodParametersNotNull(MethodParameter[])"/>
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="NumberUtils.AssertAtLeast(int, int)"/>
            /// </exception>
            public static string ShortenIfLongerThan(
                this string inputString,
                int maxStringLength,
                string cutSuffix = "..")
            {
                ExceptionUtils.AssertMethodParametersNotNull(
                    new MethodParameter(inputString, "inputString"));

                // maxStringLength must be at least cutSuffix.Length
                NumberUtils.AssertAtLeast(maxStringLength, cutSuffix.Length);

                string resultString;

                if(inputString.Length > maxStringLength) // inputString longer than maxStringLength
                {
                    // cut inputString to maxStringLength, replacing its ending with cutSuffix
                    int resultStringLength = maxStringLength - cutSuffix.Length;
                    resultString = inputString.Substring(0, resultStringLength) + cutSuffix;
                }
                else // inputString not longer than maxStringLength - inputString remains unchanged
                {
                    resultString = inputString;
                }

                return resultString;
            }

            public static string UppercaseOnlyCharactersInIndices(this string str, params int[] indices)
            {
                string upperCaseString;

                // assert that all indices are valid
                for(int i = 0; i < indices.Length; i++)
                {
                    assertValidIndexInString(
                        str,
                        indices[i],
                        string.Format("indices[{0}]", i)
                    );
                }

                char[] stringCharArray = str.ToCharArray();
                
                // turn all characters to lowercase
                for(int i = 0; i < stringCharArray.Length; i++)
                {
                    stringCharArray[i] = char.ToLower(stringCharArray[i]);
                }

                // make characters at specified indices uppercase
                foreach(int index in indices)
                {
                    stringCharArray[index] = char.ToUpper(stringCharArray[index]);
                }

                upperCaseString = new string(stringCharArray);

                return upperCaseString;
            }

            private static void assertValidIndexInString(
                this string str,
                int index, 
                string indexParameterName)
            {
                if(index < 0 || index >= str.Length)
                {
                    string exceptionMessage = string.Format(
                        "Value '{0}' of parameter '{1}' was out of range in string.",
                        index,
                        indexParameterName);
                    throw new IndexOutOfRangeException(exceptionMessage);
                }
            }

            public static string ToEnumNameFormat(this string enumName)
            {
                const int enumNameFirstLetterIndex = 0;
                string enumNameInFormat = enumName.UppercaseOnlyCharactersInIndices(
                    enumNameFirstLetterIndex);

                return enumNameInFormat;
            }
        }
    }
}

