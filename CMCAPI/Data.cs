using System;
using CryptoBlock.Utils;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class Data
        {
            internal class DataParseException : Exception
            {
                public DataParseException(string message)
                    : base(message)
                {

                }

                public DataParseException(string message, Exception innerException) 
                    : base(message, innerException)
                {

                }
            }

            internal class DataPropertyParseException : DataParseException
            {
                public DataPropertyParseException(string propertyName)
                    : base(formatExceptionMessage(propertyName))
                {

                }

                public DataPropertyParseException(string propertyName, Exception innerException)
                    : base(formatExceptionMessage(propertyName), innerException)
                {

                }

                private static string formatExceptionMessage(string propertyName)
                {
                    return string.Format("Property does not exist in JToken or is invalid: {0}.", propertyName);
                }
            }

            // column widths for table display
            protected const int ID_COLUMN_WIDTH = 8;
            protected const int NAME_COLUMN_WIDTH = 13;
            protected const int SYMBOL_COLUMN_WIDTH = 8;
            protected const int CIRCULATING_SUPPLY_COLUMN_WIDTH = 15;
            protected const int PRICE_USD_COLUMN_WIDTH = 14;
            protected const int VOLUME_COLUMN_WIDTH = 18;
            protected const int PERCENT_CHANGE_WIDTH = 11;

            // table row value display strings
            protected const string NULL_VALUE_TABLE_DISPLAY_STRING = "N/A";

            // returns null if property with specified name does not exist in jToken
            protected static T GetPropertyValue<T>(JToken jToken, string propertyName)
            {
                try
                {
                    // propertyName field exists in jToken, but its value is null
                    if (jToken[propertyName].Type == JTokenType.Null)
                    {
                        return default(T);
                    }

                    T propertyValue = jToken.Value<T>(propertyName);

                    return propertyValue;
                }
                catch (Exception exception)
                {
                    // type of T is wrong (e.g jToken[propertyName] is string but T was int)
                    if (exception is FormatException || exception is InvalidCastException
                        || exception is NullReferenceException) // propertyName does not exist in jToken
                    {
                        throw new DataPropertyParseException(propertyName, exception);
                    }

                    throw exception;
                }
            }

            protected static void AssertExist(JToken jToken, params object[] properties)
            {
                foreach (object property in properties)
                {
                    if (jToken[property] == null)
                    {
                        throw new DataPropertyParseException(property.ToString());
                    }
                }
            }

            protected string GetTableDisplayString<T>(T? propertyValue) where T : struct
            {
                return StringUtils.ToString(propertyValue, NULL_VALUE_TABLE_DISPLAY_STRING);
            }

            //protected static int GetIntProperty(object obj, string propertyName)
            //{
            //    object propertyValue = GetProperty(obj, propertyName);

            //    try
            //    {
            //        int intPropertyValue = (int)propertyValue;
            //        return intPropertyValue;
            //    }
            //    catch(Exception exception)
            //    {
            //        if(exception is FormatException || exception is InvalidCastException)
            //        {
            //            throw new DataParseException(propertyName);
            //        }

            //        throw exception;
            //    }      
            //}

            //protected static double GetDoubleProperty(object obj, string propertyName)
            //{
            //    object propertyValue = GetProperty(obj, propertyName);

            //    try
            //    {
            //        double doublePropertyValue = (double)propertyValue;
            //        return doublePropertyValue;
            //    }
            //    catch (Exception exception)
            //    {
            //        if (exception is FormatException || exception is InvalidCastException)
            //        {
            //            throw new DataParseException(propertyName);
            //        }

            //        throw exception;
            //    }
            //}

            //protected static string GetStringProperty(object obj, string propertyName)
            //{
            //    object propertyValue = GetProperty(obj, propertyName);

            //    try
            //    {
            //        string stringPropertyValue = (string)propertyValue;
            //        return stringPropertyValue;
            //    }
            //    catch (Exception exception)
            //    {
            //        if (exception is FormatException || exception is InvalidCastException)
            //        {
            //            throw new DataParseException(propertyName);
            //        }

            //        throw exception;
            //    }
            //}
        }
    }
}