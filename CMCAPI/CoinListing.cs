using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinListing : CoinData
        {
            public CoinListing(int id, string name, string symbol, long unixTimestamp)
                : base(id, name, symbol, unixTimestamp)
            {

            }

            // throws CoinDataParseException.NullMetadataFieldException if metadata field was null
            // throws CoinDataParseException.NullDataFieldException if CoinData field was null
            // throws CoinDataParseException if an inner CoinData / metadata field was null
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
                        throw new CoinDataPropertyParseException("data.num_cryptocurrencies");
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
                        throw new CoinDataParseException("Invalid JSON string.");
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
                    throw new CoinDataPropertyParseException("metadata.num_cryptocurrencies");
                }
            }

            public override string ToString()
            {
                return Utils.StringUtils.ToString(this);
            }
        }
    }
}