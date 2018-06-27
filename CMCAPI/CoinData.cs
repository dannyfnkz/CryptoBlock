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
        public class CoinData : Data
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

            private int id;
            private string name;
            private string symbol;
            private int rank;
            private double? circulatingSupply;
            private double? totalSupply;
            private double? maxSupply;
            private double? priceUsd;
            private double? volume24hUsd;
            private double? marketCapUsd;
            private double? percentChange24hUsd;
            private long unixTimestamp;

            public CoinData(
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
            {
                this.id = id;
                this.name = name;
                this.symbol = symbol;
                this.rank = rank;
                this.circulatingSupply = circulatingSupply;
                this.totalSupply = totalSupply;
                this.maxSupply = maxSupply;
                this.priceUsd = priceUsd;
                this.volume24hUsd = volume24hUsd;
                this.marketCapUsd = marketCapUsd;
                this.percentChange24hUsd = percentChange24hUsd;
                this.unixTimestamp = unixTimestamp;
            }

            public int ID
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

            public long UnixTimestamp
            {
                get { return unixTimestamp; }
            }

            public static CoinData[] ParseArray(
                string tickerArrayJSONString,
                int coinIndex,
                int coinDataArrayMaxSize)
            {
                try
                {
                    List<CoinData> coinDataList = new List<CoinData>();

                    JToken coinTickerArrayJToken = (JToken)JsonConvert.DeserializeObject(tickerArrayJSONString);

                    // handle metadata fields
                    AssertExist(coinTickerArrayJToken, "metadata");
                    JToken coinTickerArrayMetadataJToken = coinTickerArrayJToken["metadata"];
                    long unixTimestamp = parseUnixTimestamp(coinTickerArrayMetadataJToken);

                    assertNoErrorSpecifiedInResponse(coinTickerArrayMetadataJToken, coinIndex);

                    AssertExist(coinTickerArrayJToken, "data");
                    JToken coinTickerArrayDataJToken = coinTickerArrayJToken["data"];

                    fillCoinDataList(
                        coinDataList,
                        coinTickerArrayDataJToken,
                        unixTimestamp,
                        coinDataArrayMaxSize);

                    return coinDataList.ToArray();
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

            public static CoinData Parse(string tickerJSONString, int coinId)
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

                    CoinData CoinData = parseCoinData(coinTickerDataJToken, unixTimestamp);

                    return CoinData;
                }

                catch (Exception exception)
                {
                    if (
                        exception is JsonReaderException 
                        || exception is InvalidCastException
                        || exception is InvalidOperationException)
                    {
                        throw new DataParseException("Invalid JSON string.", exception);
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

            private static CoinData parseCoinData(JToken coinTickerDataJToken, long unixTimestamp)
            {
                // handle data fields
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
                double? circulatingSupply = GetPropertyValue<double>(coinTickerDataJToken, "circulating_supply");
                double? totalSupply = GetPropertyValue<double>(coinTickerDataJToken, "total_supply");
                double? maxSupply = GetPropertyValue<double>(coinTickerDataJToken, "max_supply");

                AssertExist(coinTickerDataJToken, "quotes");
                JToken coinDataDataQuotesJToken = coinTickerDataJToken["quotes"];
                AssertExist(coinDataDataQuotesJToken, "USD");
                JToken coinDataDataQuotesUsdJToken = coinDataDataQuotesJToken["USD"];

                // handle data.quotes.USD fields
                AssertExist(coinDataDataQuotesUsdJToken, "price", "volume_24h", "market_cap", "percent_change_24h");

                double priceUsd = GetPropertyValue<double>(coinDataDataQuotesUsdJToken, "price");
                double? volume24hUsd = GetPropertyValue<double>(coinDataDataQuotesUsdJToken, "volume_24h");
                double? marketCapUsd = GetPropertyValue<double>(coinDataDataQuotesUsdJToken, "market_cap");
                double percentChange24hUsd = GetPropertyValue<double>(
                    coinDataDataQuotesUsdJToken,
                    "percent_change_24h");

                CoinData coinData = new CoinData(
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

                return coinData;
            }

            private static void fillCoinDataList(
                List<CoinData> coinDataList,
                JToken coinTickerArrayDataJToken,
                long unixTimestamp,
                int coinDataArrayMaxSize)
            {
                for (int i = 0; i < coinDataArrayMaxSize; i++)
                {
                    AssertExist(coinTickerArrayDataJToken, i);
                    JToken currentCoinDataJToken = coinTickerArrayDataJToken[i];

                    CoinData currentCoinData = parseCoinData(currentCoinDataJToken, unixTimestamp);
                    coinDataList.Add(currentCoinData);
                }
            }

            private static long parseUnixTimestamp(JToken metadataJToken)
            {
                AssertExist(metadataJToken, "timestamp");
                long unixTimestamp = GetPropertyValue<int>(metadataJToken, "timestamp");

                return unixTimestamp;
            }

            public static string GetTableColumnHeaderString()
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("Name".PadRight(NAME_COLUMN_WIDTH));
                stringBuilder.Append("Symbol".PadRight(SYMBOL_COLUMN_WIDTH));
                stringBuilder.Append("Circ. Supply".PadRight(CIRCULATING_SUPPLY_COLUMN_WIDTH));
                stringBuilder.Append("Price USD".PadRight(PRICE_USD_COLUMN_WIDTH));
                stringBuilder.Append("Volume 24h (USD)".PadRight(VOLUME_COLUMN_WIDTH));
                stringBuilder.Append("% chg 24h".PadRight(PERCENT_CHANGE_WIDTH));

                return stringBuilder.ToString();
            }

            public string ToTableRowString()
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(name.PadRight(NAME_COLUMN_WIDTH));
                stringBuilder.Append(symbol.PadRight(SYMBOL_COLUMN_WIDTH));

                string circulatingSupplyString = GetTableDisplayString(circulatingSupply);
                stringBuilder.Append(circulatingSupplyString.PadRight(CIRCULATING_SUPPLY_COLUMN_WIDTH));

                string priceUsdString = GetTableDisplayString(priceUsd);
                stringBuilder.Append(priceUsdString.PadRight(PRICE_USD_COLUMN_WIDTH));

                string volume24hUsdString = GetTableDisplayString(volume24hUsd);
                stringBuilder.Append(volume24hUsdString.PadRight(VOLUME_COLUMN_WIDTH));

                string percentChange24hUsdString = GetTableDisplayString(percentChange24hUsd);
                stringBuilder.Append(percentChange24hUsdString.PadRight(PERCENT_CHANGE_WIDTH));

                return stringBuilder.ToString();
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
                        throw new DataParseException(errorMessage);
                    }
                }
            }
        }
    }
}