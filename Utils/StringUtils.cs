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

            public static string ToString<T>(T? nullable, string defaultStringValue) where T : struct
            {
                return nullable.HasValue ? nullable.Value.ToString() : defaultStringValue;
            }

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
        }
    }
}

