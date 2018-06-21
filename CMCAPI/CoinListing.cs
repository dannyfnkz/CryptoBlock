using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinListing : Data
        {
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
                CoinListing[] coinListingArray = null;

                try
                {
                    JToken staticCoinDataJToken = (JToken)JsonConvert.DeserializeObject(ListingJSONString);
                    AssertExist(staticCoinDataJToken, "metadata", "data");

                    JToken staticCoinMetadataJToken = staticCoinDataJToken["metadata"];
                    AssertExist(staticCoinMetadataJToken, "num_cryptocurrencies");
                    int staticCoinDataArrayLength = GetPropertyValue<int>(staticCoinMetadataJToken, "num_cryptocurrencies");

                    if (staticCoinDataArrayLength <= 0)
                    {
                        throw new DataPropertyParseException("data.num_cryptocurrencies");
                    }

                    coinListingArray = new CoinListing[staticCoinDataArrayLength];

                    JToken CoinListingsArray = staticCoinDataJToken["data"];
                    fillStaticCoinDataArray(coinListingArray, CoinListingsArray);

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
            private static void fillStaticCoinDataArray(
                CoinListing[] staticCoinDataArray,
                JToken CoinListingArrayJToken)
            {
                try
                {
                    for (int i = 0; i < staticCoinDataArray.Length; i++)
                    {
                        AssertExist(CoinListingArrayJToken, i);
                        JToken currentCoinListing = CoinListingArrayJToken[i];

                        AssertExist(currentCoinListing, "id", "name", "symbol");

                        int id = GetPropertyValue<int>(currentCoinListing, "id");
                        string name = GetPropertyValue<string>(currentCoinListing, "name");
                        string symbol = GetPropertyValue<string>(currentCoinListing, "symbol");
                        long unixTimestamp = Utils.DateTimeUtils.GetUnixTimestamp();

                        staticCoinDataArray[i] = new CoinListing(id, name, symbol, unixTimestamp);
                    }
                }

                catch (ArgumentOutOfRangeException) // reported listing array size was incorrect
                {
                    throw new DataPropertyParseException("metadata.num_cryptocurrencies");
                }
            }

            public override string ToString()
            {
                return CryptoBlock.Utils.StringUtils.ToString(this);
            }
        }
    }
}