using System;
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

            public static CoinData Parse(string tickerJSONString)
            {
                try
                {
                    JToken coinDataJToken = (JToken)JsonConvert.DeserializeObject(tickerJSONString);

                    AssertExist(coinDataJToken, "data");
                    JToken coinDataDataJToken = coinDataJToken["data"];

                    // handle data fields
                    AssertExist(
                        coinDataDataJToken,
                        "id",
                        "name",
                        "symbol",
                        "rank",
                        "circulating_supply",
                        "total_supply",
                        "max_supply");

                    int id = GetPropertyValue<int>(coinDataDataJToken, "id");
                    string name = GetPropertyValue<string>(coinDataDataJToken, "name");
                    string symbol = GetPropertyValue<string>(coinDataDataJToken, "symbol");
                    int rank = GetPropertyValue<int>(coinDataDataJToken, "rank");
                    double? circulatingSupply = GetPropertyValue<double>(coinDataDataJToken, "circulating_supply");
                    double? totalSupply = GetPropertyValue<double>(coinDataDataJToken, "total_supply");
                    double? maxSupply = GetPropertyValue<double>(coinDataDataJToken, "max_supply");

                    AssertExist(coinDataDataJToken, "quotes");
                    JToken coinDataDataQuotesJToken = coinDataDataJToken["quotes"];
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

                    // handle metadata fields
                    AssertExist(coinDataJToken, "metadata");
                    JToken coinDataMetadataJToken = coinDataJToken["metadata"];

                    AssertExist(coinDataMetadataJToken, "timestamp");
                    int unixTimestamp = GetPropertyValue<int>(coinDataMetadataJToken, "timestamp");

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
                catch (Exception exception)
                {
                    if (exception is JsonReaderException || exception is InvalidCastException)
                    {
                        throw new DataParseException("Invalid JSON string.", exception);
                    }
                    else
                    {
                        throw exception;
                    }
                }
            }

            //// set to work with CMC public API V2
            //public static CoinData Parse(string coinDataJSONString)
            //{
            //    CoinData coinData = null;

            //    dynamic coinDataJSONObject = JsonConvert.DeserializeObject(coinDataJSONString);

            //    if(coinDataJSONObject.data == null)
            //    {
            //        throw new DataParseException("data");
            //    }

            //    try
            //    {
            //        int id = coinDataJSONObject.data.id;
            //        string name = coinDataJSONObject.data.name;
            //        string symbol = coinDataJSONObject.data.symbol;
            //        int rank = coinDataJSONObject.data.rank;
            //        double circulatingSupply = coinDataJSONObject.data.circulating_supply;
            //        double totalSupply = coinDataJSONObject.data.total_supply;
            //        double maxSupply = coinDataJSONObject.data.max_supply;
            //        double priceUsd = coinDataJSONObject.data.quotes.USD.price;
            //        double volume24hUsd = coinDataJSONObject.data.quotes.USD.volume_24h;
            //        double marketCapUsd = coinDataJSONObject.data.quotes.USD.market_cap;
            //        double percentChange24hUsd = coinDataJSONObject.data.quotes.USD.percent_change_24h;
            //        int unixTimestamp = coinDataJSONObject.metadata.timestamp;

            //        coinData = new CoinData(
            //            id,
            //            name,
            //            symbol,
            //            rank,
            //            circulatingSupply,
            //            totalSupply,
            //            maxSupply,
            //            priceUsd,
            //            volume24hUsd,
            //            marketCapUsd,
            //            percentChange24hUsd,
            //            unixTimestamp);
            //    }

            //    catch(ArgumentNullException argumentNullException)
            //    {
            //        throw new DataParseException(argumentNullException.ParamName);
            //    }

            //    return coinData;
            //}

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
                return Utils.StringUtils.ToString(this);
            }

        }
    }

}