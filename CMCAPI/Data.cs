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
                public DataParseException(string message) : base(message)
                {

                }
            }

            internal class DataPropertyParseException : DataParseException
            {
                public DataPropertyParseException(string propertyName)
                    : base("Property does not exist in JToken or is invalid: " + propertyName)
                {

                }
            }

            // returns null if property with specified name does not exist in jToken
            protected static T GetPropertyValue<T>(JToken jToken, string propertyName)
            {
                try
                {
                    T propertyValue = jToken.Value<T>(propertyName);

                    return propertyValue;
                }
                catch (Exception exception)
                {
                    // type of T is wrong (e.g jToken[propertyName] is string but T was int)
                    if (exception is FormatException || exception is InvalidCastException)
                    {
                        throw new DataParseException(propertyName);
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