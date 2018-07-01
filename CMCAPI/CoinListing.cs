using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinListing : Data
        {
            private static readonly eDisplayProperty[] TABLE_DISPLAY_PROPERTIES
                = new eDisplayProperty[]
                {
                    eDisplayProperty.Id,
                    eDisplayProperty.Name,
                    eDisplayProperty.Symbol
                };

            private int id;
            private string name;
            private string symbol;
            private long unixTimestamp;

            public CoinListing(int id, string name, string symbol, long unixTimestamp)
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

            // throws DataParseException.NullMetadataFieldException if metadata field was null
            // throws DataParseException.NullDataFieldException if data field was null
            // throws DataParseException if an inner data / metadata field was null
            internal static CoinListing[] ParseStaticCoinDataArray(string ListingJSONString)
            {
                try
                {
                    CoinListing[] coinListingArray = null;

                    JToken coinListingJToken = (JToken)JsonConvert.DeserializeObject(ListingJSONString);

                    AssertExist(coinListingJToken, "metadata", "data");

                    JToken coinListingMetadataJToken = coinListingJToken["metadata"];
                    AssertExist(coinListingMetadataJToken, "num_cryptocurrencies");
                    int coinListingArrayLength = GetPropertyValue<int>(
                        coinListingMetadataJToken,
                        "num_cryptocurrencies");

                    if (coinListingArrayLength <= 0)
                    {
                        throw new DataPropertyParseException("data.num_cryptocurrencies");
                    }

                    coinListingArray = new CoinListing[coinListingArrayLength];

                    JToken CoinListingsArrayJToken = coinListingJToken["data"];
                    fillCoinListingArray(coinListingArray, CoinListingsArrayJToken);

                    return coinListingArray;
                }
                catch (Exception exception)
                {
                    if (exception is JsonReaderException || exception is InvalidCastException)
                    {
                        throw new DataParseException("Invalid JSON string.");
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

            // throws ArgumentNullException if a required field does not exist in staticCoinDataJSONObject
            private static void fillCoinListingArray(
                CoinListing[] coinListingArray,
                JToken CoinListingArrayJToken)
            {
                try
                {
                    for (int i = 0; i < coinListingArray.Length; i++)
                    {
                        AssertExist(CoinListingArrayJToken, i);
                        JToken currentCoinListing = CoinListingArrayJToken[i];

                        AssertExist(currentCoinListing, "id", "name", "symbol");

                        int id = GetPropertyValue<int>(currentCoinListing, "id");
                        string name = GetPropertyValue<string>(currentCoinListing, "name");
                        string symbol = GetPropertyValue<string>(currentCoinListing, "symbol");
                        long unixTimestamp = Utils.DateTimeUtils.GetUnixTimestamp();

                        coinListingArray[i] = new CoinListing(id, name, symbol, unixTimestamp);
                    }
                }

                catch (ArgumentOutOfRangeException) // listing array size specified in JSON string was incorrect
                {
                    throw new DataPropertyParseException("metadata.num_cryptocurrencies");
                }
            }

            public static string GetTableColumnHeaderString()
            {
                return Data.GetTableColumnHeaderString(TABLE_DISPLAY_PROPERTIES);
            }

            public string ToTableRowString()
            {
                return base.GetTableRowString(TABLE_DISPLAY_PROPERTIES);
            }

            public override string ToString()
            {
                return Utils.StringUtils.ToString(this);
            }
        }
    }
}