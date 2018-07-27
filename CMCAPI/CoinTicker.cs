using System;
using System.Collections.Generic;
using System.Text;
using CryptoBlock.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static CryptoBlock.Utils.JsonUtils;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinTicker : CoinData
        {
            public class CoinTickerParseException : Exception
            {
                public CoinTickerParseException(string message, Exception innerException)
                    : base(message, innerException)
                {

                }

                public CoinTickerParseException(string message)
                    : base(message)
                {

                }
            }

            public class InvalidCoinIndexException : Exception
            {
                private int coinIndex;

                public InvalidCoinIndexException(int coinIndex)
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

            private const string RESPONSE_COIN_ID_NOT_FOUND_ERROR_FIELD_VALUE = "id not found";

            private int rank;
            private double? circulatingSupply;
            private double? totalSupply;
            private double? maxSupply;
            private double? priceUsd;
            private double? volume24hUsd;
            private double? marketCapUsd;
            private double? percentChange24hUsd;

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
                double? percentChange24hUsd,
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
                this.percentChange24hUsd = percentChange24hUsd;
            }

            public int Rank
            {
                get { return rank; }
            }

            public double? CirculatingSupply
            {
                get { return circulatingSupply; }
            }

            public double? TotalSupply
            {
                get { return totalSupply; }
            }

            public double? MaxSupply
            {
                get { return maxSupply; }
            }

            public double? PriceUsd
            {
                get { return priceUsd; }
            }

            public double? Volume24hUsd
            {
                get { return volume24hUsd; }
            }

            public double? MarketCapUsd
            {
                get { return marketCapUsd; }
            }

            public double? PercentChange24hUsd
            {
                get { return percentChange24hUsd; }
            }

            public static CoinTicker[] ParseArray(
                string tickerArrayJSONString,
                int coinIndex,
                int CoinTickerArrayMaxSize)
            {
                try
                {
                    List<CoinTicker> coinTickerList = new List<CoinTicker>();

                    JToken coinTickerArrayJToken = (JToken)JsonConvert.DeserializeObject(tickerArrayJSONString);

                    // handle metadata fields
                    JsonUtils.AssertExist(coinTickerArrayJToken, "metadata");
                    JToken coinTickerArrayMetadataJToken = coinTickerArrayJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerArrayMetadataJToken);

                    assertNoErrorSpecifiedInResponse(coinTickerArrayMetadataJToken, coinIndex);

                    // handle data field, containing the ticker array
                    JsonUtils.AssertExist(coinTickerArrayJToken, "data");
                    JArray coinTickerDataJArray = (JArray)coinTickerArrayJToken["data"];

                    fillCoinTickerList(
                        coinTickerList,
                        coinTickerDataJArray,
                        unixTimestamp);

                    return coinTickerList.ToArray();
                }
                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException
                        || exception is JsonPropertyParseException
                        || exception is InvalidCastException)
                    {
                        throw new CoinTickerParseException("Invalid JSON string.", exception);
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

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

                    CoinTicker CoinTicker = parseCoinTicker(coinTickerDataJToken, unixTimestamp);

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
                        throw new CoinTickerParseException("Invalid JSON string.", exception);
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

            private static CoinTicker parseCoinTicker(JToken coinTickerDataJToken, long unixTimestamp)
            {
                // handle CoinData fields
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

                JsonUtils.AssertExist(coinTickerDataJToken, "quotes");
                JToken CoinTickerDataQuotesJToken = coinTickerDataJToken["quotes"];
                JsonUtils.AssertExist(CoinTickerDataQuotesJToken, "USD");
                JToken CoinTickerDataQuotesUsdJToken = CoinTickerDataQuotesJToken["USD"];

                // handle CoinData.quotes.USD fields
                JsonUtils.AssertExist(CoinTickerDataQuotesUsdJToken, "price", "volume_24h", "market_cap", "percent_change_24h");

                double priceUsd = JsonUtils.GetPropertyValue<double>(CoinTickerDataQuotesUsdJToken, "price");
                double? volume24hUsd = JsonUtils.GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "volume_24h");
                double? marketCapUsd = JsonUtils.GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "market_cap");
                double percentChange24hUsd = JsonUtils.GetPropertyValue<double>(
                    CoinTickerDataQuotesUsdJToken,
                    "percent_change_24h");

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

            private static void fillCoinTickerList(
                List<CoinTicker> CoinTickerList,
                JArray coinTickerDataJArray,
                long unixTimestamp)
            {
                for (int i = 0; i < coinTickerDataJArray.Count; i++)
                {
                    JsonUtils.AssertExist(coinTickerDataJArray, i);
                    JToken currentCoinTickerJToken = coinTickerDataJArray[i];

                    CoinTicker currentCoinTicker = parseCoinTicker(currentCoinTickerJToken, unixTimestamp);
                    CoinTickerList.Add(currentCoinTicker);
                }
            }

            private static long parseUnixTimestamp(JToken metadataJToken)
            {
                JsonUtils.AssertExist(metadataJToken, "timestamp");
                long unixTimestamp = JsonUtils.GetPropertyValue<int>(metadataJToken, "timestamp");

                return unixTimestamp;
            }

            public override string ToString()
            {
                return StringUtils.ToString(this);
            }

            private static void assertNoErrorSpecifiedInResponse(JToken metadataJToken, int coinIndex)
            {
                JsonUtils.AssertExist(metadataJToken, "error");

                if (!JsonUtils.IsPropertyNull(metadataJToken, "error")) // server reported an error in its response
                {
                    string errorMessage = JsonUtils.GetPropertyValue<string>(metadataJToken, "error");

                    // coin id does not exit in server
                    if (errorMessage == RESPONSE_COIN_ID_NOT_FOUND_ERROR_FIELD_VALUE)
                    {
                        throw new InvalidCoinIndexException(coinIndex);
                    }
                    else // unhandled error 
                    {
                        throw new CoinTickerParseException(errorMessage);
                    }
                }
            }
        }
    }
}