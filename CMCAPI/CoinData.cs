namespace CryptoBlock
{
    namespace CMCAPI
    {
        /// <summary>
        /// encapsulates basic coin data which remains almost constant (changes very infrequently).
        /// </summary>
        public class CoinData
        {
            protected int id;
            protected string name;
            protected string symbol;
            protected long unixTimestamp;

            /// <summary>
            /// initializes a new <see cref="CoinData"/> object using <paramref name="coinTicker"/>.
            /// </summary>
            /// <param name="coinTicker"></param>
            public CoinData(CoinTicker coinTicker)
                : this(coinTicker.id, coinTicker.name, coinTicker.symbol, coinTicker.unixTimestamp)
            {

            }

            /// <summary>
            /// initializes a new <see cref="CoinData"/> object using <paramref name="coinListing"/>.
            /// </summary>
            /// <param name="coinListing"></param>
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

            /// <summary>
            /// coin id, as specified by CMC.
            /// </summary>
            public int Id
            {
                get { return id; }
            }

            /// <summary>
            /// coin name, e.g Bitcoin.
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// coin symbol used in tickers, e.g BTC.
            /// </summary>
            public string Symbol
            {
                get { return symbol; }
            }

            /// <summary>
            /// time when this <see cref="CoinData"/> was retrieved from server.
            /// </summary>
            public long UnixTimestamp
            {
                get { return unixTimestamp; }
            }

            //// table row value display strings
            //protected const string NULL_VALUE_TABLE_DISPLAY_STRING = "N/A";

            //protected static string GetTableDisplayString<T>(T? propertyValue) where T : struct
            //{
            //    return StringUtils.ToString(propertyValue, NULL_VALUE_TABLE_DISPLAY_STRING);
            //}

            //protected static string GetTableDisplayString(object propertyValue)
            //{
            //    return StringUtils.ToString(propertyValue, NULL_VALUE_TABLE_DISPLAY_STRING);
            //}
        }
    }
}
