using System;
using System.Collections.Generic;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static CryptoBlock.Utils.JsonUtils;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// a CMC coin ticker.
        /// </summary>
        public class CoinTicker : CoinData
        {
            /// <summary>
            /// thrown if <see cref="CoinTicker"/> could not be parsed from server JSON response string.
            /// </summary>
            public class CoinTickerJsonParseException : Exception
            {
                public CoinTickerJsonParseException(string message, Exception innerException)
                    : base(message, innerException)
                {

                }

                public CoinTickerJsonParseException(string message)
                    : base(message)
                {

                }
            }

            /// <summary>
            /// thrown if the specified coin index (coin id in case of single ticker request) was not found.
            /// </summary>
            public class CoinIndexNotFoundException : Exception
            {
                private int coinIndex;

                public CoinIndexNotFoundException(int coinIndex)
                    : base(formatExceptionMessage(coinIndex))
                {
                    this.coinIndex = coinIndex;
                }

                public int CoinIndex
                {
                    get { return coinIndex; }
                }

                private static string formatExceptionMessage(int coinIndex)
                {
                    return string.Format("Coin index not found: {0}.", coinIndex);
                }
            }

            // value in JSON server response string metadata.error field in case coin index was not found
            private const string RESPONSE_COIN_INDEX_NOT_FOUND_ERROR_FIELD_VALUE = "id not found";

            private int rank;
            private double? circulatingSupply;
            private double? totalSupply;
            private double? maxSupply;
            private double? priceUsd;
            private double? volume24hUsd;
            private double? marketCapUsd;
            private double? pricePercentChange24hUsd;

            public CoinTicker(
                int id,
                string name,
                string symbol,
                int rank,
                double? circulatingSupply,
                double? totalSupply,
                double? maxSupply,
                double? priceUsd,
                double? volume24hUsd,
                double? marketCapUsd,
                double? pricePercentChange24hUsd,
                long unixTimestamp)
                : base(id, name, symbol, unixTimestamp)
            {
                this.rank = rank;
                this.circulatingSupply = circulatingSupply;
                this.totalSupply = totalSupply;
                this.maxSupply = maxSupply;
                this.priceUsd = priceUsd;
                this.volume24hUsd = volume24hUsd;
                this.marketCapUsd = marketCapUsd;
                this.pricePercentChange24hUsd = pricePercentChange24hUsd;
            }

            /// <summary>
            /// rank determined by <see cref="MarketCapUsd"/>.
            /// </summary>
            public int Rank
            {
                get { return rank; }
            }

            /// <summary>
            /// approximation of the number of coins that are circulating in the market.
            /// </summary>
            /// <remarks>
            /// if null, data regarding circulating supply is not available.
            /// </remarks>
            public double? CirculatingSupply
            {
                get { return circulatingSupply; }
            }

            /// <summary>
            /// total amount of coins in existence
            /// </summary>
            /// <remarks>
            /// if null, data regarding total supply is not available.
            /// </remarks>
            public double? TotalSupply
            {
                get { return totalSupply; }
            }

            /// <summary>
            /// approximation of the maximum amount of coins that will ever exist in the lifetime of the coin.
            /// </summary>
            /// <remarks>
            /// if null, data regarding max supply is not available.
            /// </remarks>
            public double? MaxSupply
            {
                get { return maxSupply; }
            }

            /// <summary>
            /// price of coin in USD.
            /// </summary>
            /// <remarks>
            /// if null, data regarding price in USD is not available.
            /// </remarks>
            public double? PriceUsd
            {
                get { return priceUsd; }
            }

            /// <summary>
            /// 24 hour period trading volume of coin in USD.
            /// </summary>
            /// <remarks>
            /// if null, data regarding 24 hour USD volume is not available.
            /// </remarks>
            public double? Volume24hUsd
            {
                get { return volume24hUsd; }
            }

            /// <summary>
            /// calculate as (<see cref = "PriceUsd" /> x < see cref="CirculatingSupply"/>).
            /// </summary>
            /// <remarks>
            /// if null, data regarding market cap in usd is not available.
            /// </remarks>
            public double? MarketCapUsd
            {
                get { return marketCapUsd; }
            }

            /// <summary>
            /// percentage of the change in coin price in the last 24 hour period.
            /// </summary>
            /// <remarks>
            /// if null, data regarding coin 24 hour price change percentage is not available.
            /// </remarks>
            public double? PricePercentChange24hUsd
            {
                get { return pricePercentChange24hUsd; }
            }

            /// <summary>
            /// parses array of <see cref="CoinTicker"/>s from <paramref name="tickerArrayJSONString"/>,
            /// starting at <paramref name="startingCoinIndex"/>,
            /// and having a maximum length of <paramref name="CoinTickerArrayMaxSize"/>.
            /// </summary>
            /// <param name="tickerArrayJSONString"></param>
            /// <param name="startingCoinIndex">
            /// index of first coin in <paramref name="tickerArrayJSONString"/>
            /// </param>
            /// <param name="CoinTickerArrayMaxSize">
            /// maximum length of parsed array
            /// </param>
            /// <returns>
            /// array of <see cref="CoinTicker"/>s parsed from <paramref name="tickerArrayJSONString"/>,
            /// starting at <paramref name="startingCoinIndex"/>,
            /// and having a maximum length of <paramref name="CoinTickerArrayMaxSize"/>
            /// </returns>
            /// <exception cref="CoinTickerJsonParseException">
            /// thrown if <paramref name="tickerArrayJSONString"/> was invalid.
            /// </exception>
            /// <exception cref="CoinIndexNotFoundException">
            /// thrown if specified <paramref name="startingCoinIndex"/> does not exist in server.
            /// </exception>
            public static CoinTicker[] ParseArray(
                string tickerArrayJSONString,
                int startingCoinIndex,
                int coinTickerArrayMaxLength)
            {
                try
                {
                    List<CoinTicker> coinTickerList = new List<CoinTicker>();

                    JToken coinTickerArrayJToken = (JToken)JsonConvert.DeserializeObject(tickerArrayJSONString);

                    // handle metadata fields
                    JsonUtils.AssertExist(coinTickerArrayJToken, "metadata");
                    JToken coinTickerArrayMetadataJToken = coinTickerArrayJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerArrayMetadataJToken);

                    // assert that (valid) server response did not specify an error.
                    assertNoErrorSpecifiedInResponse(coinTickerArrayMetadataJToken, startingCoinIndex);

                    // fetch ticker array JToken, located in data field
                    JsonUtils.AssertExist(coinTickerArrayJToken, "data");
                    JArray coinTickerJArray = (JArray)coinTickerArrayJToken["data"];

                    // fill coinTickerList with data from ticker JArray
                    fillCoinTickerList(
                        coinTickerList,
                        coinTickerJArray,
                        unixTimestamp);

                    int coinTickerArrayLength = Math.Min(coinTickerArrayMaxLength, coinTickerList.Count);

                    return coinTickerList.GetRange(0, coinTickerArrayLength).ToArray();
                }
                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException
                        || exception is JsonPropertyParseException
                        || exception is InvalidCastException)
                    {
                        throw new CoinTickerJsonParseException("Invalid JSON string.", exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            /// <summary>
            /// parses a single <see cref="CoinTicker"/> with <paramref name="coinId"/>
            /// from <paramref name="tickerJSONString"/>.
            /// </summary>
            /// <param name="tickerJSONString"></param>
            /// <param name="coinId"></param>
            /// <returns>
            /// <see cref="CoinTicker"/> having <paramref name="coinId"/>
            /// parsed from <paramref name="tickerJSONString"/>
            /// </returns>
            /// <exception cref="CoinIndexNotFoundException">
            /// <seealso cref="assertNoErrorSpecifiedInResponse(JToken, int)"/>
            /// </exception>
            /// <exception cref="CoinTickerJsonParseException">
            /// thrown if <paramref name="tickerJSONString"/> is invalid
            /// </exception>
            public static CoinTicker Parse(string tickerJSONString, int coinId)
            {
                try
                {
                    JToken coinTickerJToken = (JToken)JsonConvert.DeserializeObject(tickerJSONString);

                    // handle metadata fields
                    JsonUtils.AssertExist(coinTickerJToken, "metadata");
                    JToken coinTickerMetadataJToken = coinTickerJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerMetadataJToken);

                    assertNoErrorSpecifiedInResponse(coinTickerMetadataJToken, coinId);

                    // handle data fields
                    JsonUtils.AssertExist(coinTickerJToken, "data");
                    JToken coinTickerDataJToken = coinTickerJToken["data"];

                    // parse coin ticker using coinTickerDataJToken
                    CoinTicker CoinTicker = parse(coinTickerDataJToken, unixTimestamp);

                    return CoinTicker;
                }

                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException
                        || exception is JsonPropertyParseException
                        || exception is InvalidCastException
                        || exception is InvalidOperationException)
                    {
                        throw new CoinTickerJsonParseException("Invalid JSON string.", exception);
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

            /// <summary>
            /// fills <paramref name="CoinTickerList"/> with <see cref="CoinTicker"/>s parsed from
            /// <paramref name="coinTickerJArray"/>, each having <paramref name="unixTimestamp"/>.
            /// </summary>
            /// <seealso cref="parse(JToken, long)"/>
            /// <param name="CoinTickerList"></param>
            /// <param name="coinTickerJArray"></param>
            /// <param name="unixTimestamp"></param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// <seealso cref="parse(JToken, long)"/>
            /// </exception>
            /// <exception cref="JsonReaderException">
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// <seealso cref="parse(JToken, long)"/>
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// <seealso cref="parse(JToken, long)"/>
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// <seealso cref="parse(JToken, long)"/>
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// </exception>
            private static void fillCoinTickerList(
                List<CoinTicker> CoinTickerList,
                JArray coinTickerJArray,
                long unixTimestamp)
            {
                for (int i = 0; i < coinTickerJArray.Count; i++)
                {
                    // get JToken of the i'th item in coin tickre array
                    JsonUtils.AssertExist(coinTickerJArray, i);
                    JToken currentCoinTickerJToken = coinTickerJArray[i];

                    // parse i'th coin ticker from corresponding JToken
                    CoinTicker currentCoinTicker = parse(currentCoinTickerJToken, unixTimestamp);

                    // add coin ticker to list
                    CoinTickerList.Add(currentCoinTicker);
                }
            }

            /// <summary>
            /// parses <see cref="CoinTicker"/> having <paramref name="unixTimestamp"/> from
            /// <paramref name="coinTickerDataJToken"/>.
            /// </summary>
            /// <param name="coinTickerDataJToken"></param>
            /// <param name="unixTimestamp"></param>
            /// <returns>
            /// <see cref="CoinTicker"/> parsed from <paramref name="coinTickerDataJToken"/>,
            /// having <paramref name="unixTimestamp"/>
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="JsonReaderException">
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// </exception>
            /// <exception cref="InvalidCastException">
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// <seealso cref="JToken.Value{T}(object)"/>
            /// </exception>
            private static CoinTicker parse(JToken coinTickerDataJToken, long unixTimestamp)
            {
                // fetch coin ticker fields

                // handle "data" fields
                JsonUtils.AssertExist(
                    coinTickerDataJToken,
                    "id",
                    "name",
                    "symbol",
                    "rank",
                    "circulating_supply",
                    "total_supply",
                    "max_supply");

                int id = JsonUtils.GetPropertyValue<int>(coinTickerDataJToken, "id");
                string name = JsonUtils.GetPropertyValue<string>(coinTickerDataJToken, "name");
                string symbol = JsonUtils.GetPropertyValue<string>(coinTickerDataJToken, "symbol");
                int rank = JsonUtils.GetPropertyValue<int>(coinTickerDataJToken, "rank");
                double? circulatingSupply = JsonUtils.GetPropertyValue<double?>(coinTickerDataJToken, "circulating_supply");
                double? totalSupply = JsonUtils.GetPropertyValue<double?>(coinTickerDataJToken, "total_supply");
                double? maxSupply = JsonUtils.GetPropertyValue<double?>(coinTickerDataJToken, "max_supply");

                // handle "data.quotes" fields
                JsonUtils.AssertExist(coinTickerDataJToken, "quotes");
                JToken CoinTickerDataQuotesJToken = coinTickerDataJToken["quotes"];
                JsonUtils.AssertExist(CoinTickerDataQuotesJToken, "USD");
                JToken CoinTickerDataQuotesUsdJToken = CoinTickerDataQuotesJToken["USD"];

                // handle data.quotes.USD fields
                JsonUtils.AssertExist(CoinTickerDataQuotesUsdJToken, "price", "volume_24h", "market_cap", "percent_change_24h");

                double priceUsd = JsonUtils.GetPropertyValue<double>(CoinTickerDataQuotesUsdJToken, "price");
                double? volume24hUsd = JsonUtils.GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "volume_24h");
                double? marketCapUsd = JsonUtils.GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "market_cap");
                double percentChange24hUsd = JsonUtils.GetPropertyValue<double>(
                    CoinTickerDataQuotesUsdJToken,
                    "percent_change_24h");

                // init a new coin ticker
                CoinTicker CoinTicker = new CoinTicker(
                    id,
                    name,
                    symbol,
                    rank,
                    circulatingSupply,
                    totalSupply,
                    maxSupply,
                    priceUsd,
                    volume24hUsd,
                    marketCapUsd,
                    percentChange24hUsd,
                    unixTimestamp);

                return CoinTicker;
            }

            /// <summary>
            /// parses unix timestamp <paramref name="metadataJToken"/>.
            /// </summary>
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// <param name="metadataJToken"></param>
            /// <returns>
            /// unix timestamp parsed from <paramref name="metadataJToken"/>
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException>
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// </exception>
            private static long parseUnixTimestamp(JToken metadataJToken)
            {
                JsonUtils.AssertExist(metadataJToken, "timestamp");

                long unixTimestamp = JsonUtils.GetPropertyValue<int>(metadataJToken, "timestamp");

                return unixTimestamp;
            }

            /// <summary>
            /// asserts that server has not specified in <paramref name="metadataJToken"/> that an error occurred
            /// while processing request.
            /// </summary>
            /// <param name="metadataJToken"></param>
            /// <param name="coinIndex">
            /// index of first (or only) <see cref="CoinTicker"/> requested from server.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// <seealso cref="JsonUtils.AssertExist(JToken, object[])"/>
            /// <seealso cref="JsonUtils.GetPropertyValue{T}(JToken, string)"/>
            /// </exception>
            /// <exception cref="CoinIndexNotFoundException">
            /// thrown if server specified that <paramref name="startingCoinIndex"/> does not exist in server
            /// </exception>
            /// <exception cref="CoinTickerJsonParseException">
            /// thrown if server specified an unhandled error
            /// </exception>
            private static void assertNoErrorSpecifiedInResponse(JToken metadataJToken, int startingCoinIndex)
            {
                // assert that error field exists in meta data
                JsonUtils.AssertExist(metadataJToken, "error");

                // error field value is not null - server reported an error in its response
                if (!JsonUtils.IsPropertyNull(metadataJToken, "error")) 
                {
                    // get error field value
                    string errorFieldValue = JsonUtils.GetPropertyValue<string>(metadataJToken, "error");

                    // coin id does not exist in server
                    if (errorFieldValue == RESPONSE_COIN_INDEX_NOT_FOUND_ERROR_FIELD_VALUE)
                    {
                        throw new CoinIndexNotFoundException(startingCoinIndex);
                    }
                    else // unhandled error 
                    {
                        throw new CoinTickerJsonParseException(errorFieldValue);
                    }
                }
            }
        }
    }
}