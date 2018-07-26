using System;
using System.Reflection;
using System.Text;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// string utility class.
        /// </summary>
        public static class StringUtils
        {
            public const string TAB_STRING = "\t";
            public const char TAB_CHAR = '\t';

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
            public static void Append(StringBuilder stringBuilder, string appendant, int numberOfTimes = 1)
            {
                for (int i = 0; i < numberOfTimes; i++)
                {
                    stringBuilder.Append(appendant);
                }
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
            public static string Append(string str, string seperator, params string[] appendants)
            {
                StringBuilder stringBuilder = new StringBuilder(str);

                for(int i = 0; i < appendants.Length; i++)
                {
                    stringBuilder.Append(appendants[i]);

                    if(i < appendants.Length - 1) // append seperator if not at last appendant
                    {
                        stringBuilder.Append(seperator);
                    }      
                }

                return stringBuilder.ToString();
            }

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
            public static string[] Split(string str, StringSplitOptions splitOptions, params string[] parameters)
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
            public static string[] Split(string str, params string[] parameters)
            {
                return Split(str, StringSplitOptions.None, parameters);
            }

            /// <summary>
            /// returns a substring of <paramref name="str"/>, starting immediately after <paramref name="prefix"/>.
            /// if <paramref name="prefix"/> does not exist in <paramref name="str"/>, returns <paramref name="str"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefix"></param>
            /// <returns>
            /// <para></para>substring of <paramref name="str"/>, starting immediately after <paramref name="prefix"/>, 
            /// if <paramref name="prefix"/> exists in in <paramref name="str"/>.</para>
            /// <para>else, <paramref name="str"/>.</para>
            /// </returns>
            public static string Substring(string str, string prefix)
            {
                return str.Substring(prefix.Length);
            }

            /// <summary>
            /// returns a string representation of <paramref name="obj"/>, including all properties and their
            /// corrosponding values.
            /// </summary>
            /// <remarks>
            /// uses <see cref="System.Reflection"/>.
            /// </remarks>
            /// <param name="obj"></param>
            /// <returns>
            /// a string representation of an object in the format:  class namespace.name [field1=value1, field2=value2, ...]
            /// </returns>
            public static string ToString(object obj)
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
            /// returns whether <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// true if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// else false
            /// </returns>
            public static bool StartsWith(string str, params string[] prefixes)
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
            public static string GetPrefixIfStartsWith(string str, params string[] prefixes)
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
        }
    }
}

