using System;
using CryptoBlock.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static CryptoBlock.Utils.JsonUtils;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// represents a CMC coin listing.
        /// </summary>
        public class CoinListing : CoinData
        {
            /// <summary>
            /// thrown if <see cref="CoinListing"/> could not be parsed from server JSON response string.
            /// </summary>
            public class CoinListingJsonParseException : Exception
            {
                public CoinListingJsonParseException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "Invalid JSON server response string.";
                }
            }

            public CoinListing(int id, string name, string symbol, long unixTimestamp)
                : base(id, name, symbol, unixTimestamp)
            {

            }

            /// <summary>
            /// parses a <see cref="CoinListing"/> array from <paramref name="ListingJsonString"/>.
            /// </summary>
            /// <seealso cref="JsonConvert.DeserializeObject(string)"/>
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// <param name="ListingJSONString"></param>
            /// <returns>
            /// <see cref="CoinListing"/> array parsed from <paramref name="ListingJsonString"/>
            /// </returns>
            /// <exception cref="CoinListingJsonParseException">
            /// thrown if <paramref name="ListingJsonString"/> is invalid.
            /// </exception>
            internal static CoinListing[] ParseCoinListingArray(string ListingJsonString)
            {
                try
                {
                    CoinListing[] coinListingArray;

                    // deserialize json string to get a JToken
                    JToken coinListingJToken = (JToken)JsonConvert.DeserializeObject(ListingJsonString);

                    // handle matadata
                    JsonUtils.AssertExist(coinListingJToken, "metadata", "data");
                    JToken coinListingMetadataJToken = coinListingJToken["metadata"];

                    // get length of coin listing array
                    JsonUtils.AssertExist(coinListingMetadataJToken, "num_cryptocurrencies");
                    int coinListingArrayLength = JsonUtils.GetPropertyValue<int>(
                        coinListingMetadataJToken,
                        "num_cryptocurrencies");

                    if (coinListingArrayLength <= 0) // invalid coin listing array length
                    {
                        throw new JsonPropertyParseException("data.num_cryptocurrencies");
                    }

                    // init coin listing array
                    coinListingArray = new CoinListing[coinListingArrayLength];

                    // get coin listing array JToken
                    JToken CoinListingsArrayJToken = coinListingJToken["data"];

                    // fill coin listing array with data from JToken
                    fillCoinListingArray(coinListingArray, CoinListingsArrayJToken);

                    return coinListingArray;
                }
                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException 
                        || exception is JsonPropertyParseException 
                        || exception is InvalidCastException)
                    {
                        throw new CoinListingJsonParseException(exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            /// <summary>
            /// fills <paramref name="coinListingArray"/> with <see cref="CoinListing"/>s fetched from
            /// <paramref name="coinListingArrayJToken"/>.
            /// </summary>
            /// <param name="coinListingArray"></param>
            /// <param name="CoinListingArrayJToken"></param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// in addition, thrown if listing array length specified in <paramref name="coinListingArrayJToken"/>
            /// does not match length of <paramref name="coinListingArray"/>
            /// </exception>
            private static void fillCoinListingArray(
                CoinListing[] coinListingArray,
                JToken coinListingArrayJToken)
            {
                try
                {
                    for (int i = 0; i < coinListingArray.Length; i++)
                    {
                        AssertExist(coinListingArrayJToken, i);
                        JToken currentCoinListing = coinListingArrayJToken[i];

                        AssertExist(currentCoinListing, "id", "name", "symbol");

                        int id = GetPropertyValue<int>(currentCoinListing, "id");
                        string name = GetPropertyValue<string>(currentCoinListing, "name");
                        string symbol = GetPropertyValue<string>(currentCoinListing, "symbol");
                        long unixTimestamp = Utils.DateTimeUtils.GetUnixTimestamp();

                        coinListingArray[i] = new CoinListing(id, name, symbol, unixTimestamp);
                    }
                }
                // listing array size specified in coinListingArrayJToken does not match coinListingArray length
                catch (ArgumentOutOfRangeException) 
                {
                    throw new JsonPropertyParseException("metadata.num_cryptocurrencies");
                }
            }

            public override string ToString()
            {
                return StringUtils.ToString(this);
            }
        }
    }
}