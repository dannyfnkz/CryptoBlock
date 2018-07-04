
using System;
using System.Collections.Generic;
using System.Text;
using CryptoBlock.Utils;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinData
        {
            public class CoinDataParseException : Exception
            {
                public CoinDataParseException(string message)
                    : base(message)
                {

                }

                public CoinDataParseException(string message, Exception innerException)
                    : base(message, innerException)
                {

                }
            }

            public class CoinDataPropertyParseException : CoinDataParseException
            {
                public CoinDataPropertyParseException(string propertyName)
                    : base(formatExceptionMessage(propertyName))
                {

                }

                public CoinDataPropertyParseException(string propertyName, Exception innerException)
                    : base(formatExceptionMessage(propertyName), innerException)
                {

                }

                private static string formatExceptionMessage(string propertyName)
                {
                    return string.Format("Property does not exist in JToken or is invalid: {0}.", propertyName);
                }
            }

            protected int id;
            protected string name;
            protected string symbol;
            protected long unixTimestamp;

            public CoinData(CoinTicker coinTicker)
                : this(coinTicker.id, coinTicker.name, coinTicker.symbol, coinTicker.unixTimestamp)
            {

            }

            public CoinData(CoinListing coinListing)
                : this(coinListing.id, coinListing.name, coinListing.symbol, coinListing.unixTimestamp)
            {

            }

            public CoinData(int id, string name, string symbol, long unixTimestamp)
            {
                this.id = id;
                this.name = name;
                this.symbol = symbol;
                this.unixTimestamp = unixTimestamp;
            }

            public int Id
            {
                get { return id; }
            }

            public string Name
            {
                get { return name; }
            }

            public string Symbol
            {
                get { return symbol; }
            }

            public long UnixTimestamp
            {
                get { return unixTimestamp; }
            }

            // table row value display strings
            protected const string NULL_VALUE_TABLE_DISPLAY_STRING = "N/A";

            // returns null if property with specified name does not exist in jToken
            protected static T GetPropertyValue<T>(JToken jToken, string propertyName)
            {
                try
                {
                    // propertyName field exists in jToken, but its value is null
                    if (IsNull(jToken, propertyName))
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
                        throw new CoinDataPropertyParseException(propertyName, exception);
                    }

                    throw exception;
                }
            }

            protected static bool CheckExist(JToken jToken, params object[] properties)
            {
                foreach (object property in properties)
                {
                    if (jToken[property] == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            protected static void AssertExist(JToken jToken, params object[] properties)
            {
                foreach (object property in properties)
                {
                    if (jToken[property] == null)
                    {
                        throw new CoinDataPropertyParseException(property.ToString());
                    }
                }
            }

            protected static bool IsNull(JToken jToken, string propertyName)
            {
                return jToken[propertyName].Type == JTokenType.Null;
            }

            protected static string GetTableDisplayString<T>(T? propertyValue) where T : struct
            {
                return StringUtils.ToString(propertyValue, NULL_VALUE_TABLE_DISPLAY_STRING);
            }

            protected static string GetTableDisplayString(object propertyValue)
            {
                return StringUtils.ToString(propertyValue, NULL_VALUE_TABLE_DISPLAY_STRING);
            }
        }
    }
}
