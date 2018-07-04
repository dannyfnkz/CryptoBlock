using System;
using System.Collections.Generic;
using System.Text;
using CryptoBlock.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoBlock
{
    namespace CMCAPI
    {
        public class CoinTicker : CoinData
        {
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
                    AssertExist(coinTickerArrayJToken, "metadata");
                    JToken coinTickerArrayMetadataJToken = coinTickerArrayJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerArrayMetadataJToken);

                    assertNoErrorSpecifiedInResponse(coinTickerArrayMetadataJToken, coinIndex);

                    // handle data field, containing the ticker array
                    AssertExist(coinTickerArrayJToken, "data");
                    JArray coinTickerDataJArray = (JArray)coinTickerArrayJToken["data"];

                    fillCoinTickerList(
                        coinTickerList,
                        coinTickerDataJArray,
                        unixTimestamp);

                    return coinTickerList.ToArray();
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

            public static CoinTicker Parse(string tickerJSONString, int coinId)
            {
                try
                {
                    JToken coinTickerJToken = (JToken)JsonConvert.DeserializeObject(tickerJSONString);

                    // handle metadata fields
                    AssertExist(coinTickerJToken, "metadata");
                    JToken coinTickerMetadataJToken = coinTickerJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerMetadataJToken);

                    assertNoErrorSpecifiedInResponse(coinTickerMetadataJToken, coinId);

                    // handle data fields
                    AssertExist(coinTickerJToken, "data");
                    JToken coinTickerDataJToken = coinTickerJToken["data"];

                    CoinTicker CoinTicker = parseCoinTicker(coinTickerDataJToken, unixTimestamp);

                    return CoinTicker;
                }

                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException
                        || exception is InvalidCastException
                        || exception is InvalidOperationException)
                    {
                        throw new CoinDataParseException("Invalid JSON string.", exception);
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
                AssertExist(
                    coinTickerDataJToken,
                    "id",
                    "name",
                    "symbol",
                    "rank",
                    "circulating_supply",
                    "total_supply",
                    "max_supply");

                int id = GetPropertyValue<int>(coinTickerDataJToken, "id");
                string name = GetPropertyValue<string>(coinTickerDataJToken, "name");
                string symbol = GetPropertyValue<string>(coinTickerDataJToken, "symbol");
                int rank = GetPropertyValue<int>(coinTickerDataJToken, "rank");
                double? circulatingSupply = GetPropertyValue<double?>(coinTickerDataJToken, "circulating_supply");
                double? totalSupply = GetPropertyValue<double?>(coinTickerDataJToken, "total_supply");
                double? maxSupply = GetPropertyValue<double?>(coinTickerDataJToken, "max_supply");

                AssertExist(coinTickerDataJToken, "quotes");
                JToken CoinTickerDataQuotesJToken = coinTickerDataJToken["quotes"];
                AssertExist(CoinTickerDataQuotesJToken, "USD");
                JToken CoinTickerDataQuotesUsdJToken = CoinTickerDataQuotesJToken["USD"];

                // handle CoinData.quotes.USD fields
                AssertExist(CoinTickerDataQuotesUsdJToken, "price", "volume_24h", "market_cap", "percent_change_24h");

                double priceUsd = GetPropertyValue<double>(CoinTickerDataQuotesUsdJToken, "price");
                double? volume24hUsd = GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "volume_24h");
                double? marketCapUsd = GetPropertyValue<double?>(CoinTickerDataQuotesUsdJToken, "market_cap");
                double percentChange24hUsd = GetPropertyValue<double>(
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
                    AssertExist(coinTickerDataJArray, i);
                    JToken currentCoinTickerJToken = coinTickerDataJArray[i];

                    CoinTicker currentCoinTicker = parseCoinTicker(currentCoinTickerJToken, unixTimestamp);
                    CoinTickerList.Add(currentCoinTicker);
                }
            }

            private static long parseUnixTimestamp(JToken metadataJToken)
            {
                AssertExist(metadataJToken, "timestamp");
                long unixTimestamp = GetPropertyValue<int>(metadataJToken, "timestamp");

                return unixTimestamp;
            }

            public override string ToString()
            {
                return StringUtils.ToString(this);
            }

            private static void assertNoErrorSpecifiedInResponse(JToken metadataJToken, int coinIndex)
            {
                AssertExist(metadataJToken, "error");

                if (!IsNull(metadataJToken, "error")) // error in response
                {
                    string errorMessage = GetPropertyValue<string>(metadataJToken, "error");

                    if (errorMessage == RESPONSE_COIN_ID_NOT_FOUND_ERROR_FIELD_VALUE)
                    {
                        throw new InvalidCoinIndexException(coinIndex);
                    }
                    else // unhandled error 
                    {
                        throw new CoinDataParseException(errorMessage);
                    }
                }
            }
        }
    }
}