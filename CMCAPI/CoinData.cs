
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

            public enum eDisplayProperty
            {
                Id,
                Name,
                Symbol,
                Rank,
                CirculatingSupply,
                TotalSupply,
                MaxSupply,
                PriceUsd,
                Volume24hUsd,
                MarketCapUsd,
                PercentChange24hUsd
            }

         //   public static readonly string ASSEMBLY_QUALIFIED_NAME = typeof(CoinData)

            private static Dictionary<eDisplayProperty, int> displayPropertyToColumnWidth =
                new Dictionary<eDisplayProperty, int>
                {
                    { eDisplayProperty.Id, 8},
                    { eDisplayProperty.Name, 13},
                    { eDisplayProperty.Symbol, 8},
                    { eDisplayProperty.Rank, 8},
                    { eDisplayProperty.CirculatingSupply, 15},
                    { eDisplayProperty.TotalSupply, 15},
                    { eDisplayProperty.MaxSupply, 15},
                    { eDisplayProperty.PriceUsd, 14},
                    { eDisplayProperty.Volume24hUsd, 18},
                    { eDisplayProperty.MarketCapUsd, 15},
                    { eDisplayProperty.PercentChange24hUsd, 11}
                };

            private static Dictionary<eDisplayProperty, string> displayPropertyToPropertyName =
                new Dictionary<eDisplayProperty, string>
                {
                    { eDisplayProperty.Id, "Id" },
                    { eDisplayProperty.Name, "Name" },
                    { eDisplayProperty.Symbol, "Symbol" },
                    { eDisplayProperty.Rank, "Rank" },
                    { eDisplayProperty.CirculatingSupply, "CirculatingSupply" },
                    { eDisplayProperty.TotalSupply, "TotalSupply" },
                    { eDisplayProperty.MaxSupply, "MaxSupply" },
                    { eDisplayProperty.PriceUsd, "PriceUsd" },
                    { eDisplayProperty.Volume24hUsd, "Volume24hUsd" },
                    { eDisplayProperty.MarketCapUsd, "MarketCapUsd" },
                    { eDisplayProperty.PercentChange24hUsd, "PercentChange24hUsd" }
                };

            private static Dictionary<eDisplayProperty, string> displayPropertyToColumnHeaderDisplayString =
                new Dictionary<eDisplayProperty, string>
                {
                    {
                        eDisplayProperty.Id,
                        getPaddedDisplayPropertyString("ID", eDisplayProperty.Id)
                    },
                    {
                        eDisplayProperty.Name,
                        getPaddedDisplayPropertyString("Name", eDisplayProperty.Name)
                    },
                    {
                        eDisplayProperty.Symbol,
                        getPaddedDisplayPropertyString("Symbol", eDisplayProperty.Symbol)
                    },
                    {   eDisplayProperty.Rank,
                        getPaddedDisplayPropertyString("Rank", eDisplayProperty.Rank)
                    },
                    {
                        eDisplayProperty.CirculatingSupply,
                        getPaddedDisplayPropertyString("Circ. Supply", eDisplayProperty.CirculatingSupply)
                    },
                    {
                        eDisplayProperty.TotalSupply,
                        getPaddedDisplayPropertyString("Total Supply", eDisplayProperty.TotalSupply)
                    },
                    {
                        eDisplayProperty.MaxSupply,
                        getPaddedDisplayPropertyString("Max Supply", eDisplayProperty.MaxSupply)
                    },
                    {
                        eDisplayProperty.PriceUsd,
                        getPaddedDisplayPropertyString("Price USD", eDisplayProperty.PriceUsd)
                    },
                    {
                        eDisplayProperty.Volume24hUsd,
                        getPaddedDisplayPropertyString("Volume 24h (USD)", eDisplayProperty.Volume24hUsd)
                    },
                    {
                        eDisplayProperty.MarketCapUsd,
                        getPaddedDisplayPropertyString("Market Cap (USD)", eDisplayProperty.MarketCapUsd)
                    },
                    {
                        eDisplayProperty.PercentChange24hUsd,
                        getPaddedDisplayPropertyString("% chg 24h", eDisplayProperty.PercentChange24hUsd)
                    }
                };

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

            public static string GetTableColumnHeaderString(params eDisplayProperty[] displayProperties)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (eDisplayProperty displayProperty in displayProperties)
                {
                    string columnHeaderDisplayString = displayPropertyToColumnHeaderDisplayString[displayProperty];
                    stringBuilder.Append(columnHeaderDisplayString);
                }

                return stringBuilder.ToString();
            }

            protected string GetTableRowString(params eDisplayProperty[] displayProperties)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (eDisplayProperty displayProperty in displayProperties)
                {
                    string propertyName = displayPropertyToPropertyName[displayProperty];
                    object propertyValue = ReflectionUtils.GetPropertyValue(this, propertyName);

                    string propertyValueString = GetTableDisplayString(propertyValue);
                    string paddedPropertyValueString = getPaddedDisplayPropertyString(
                        propertyValueString,
                        displayProperty);

                    stringBuilder.Append(paddedPropertyValueString);
                }

                return stringBuilder.ToString();
            }

            private static string getPaddedDisplayPropertyString(
                string str,
                eDisplayProperty displayProperty)
            {
                int paddedStringWidth = displayPropertyToColumnWidth[displayProperty];

                return str.PadRight(paddedStringWidth);
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
