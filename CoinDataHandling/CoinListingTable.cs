using CryptoBlock.CMCAPI;
using CryptoBlock.Utils.Tables;
using static CryptoBlock.Utils.Tables.PropertyTable;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// represents a table displaying <see cref="CoinListing"/> data.
        /// </summary>
        public class CoinListingTable
        {
            // array containing all Properties associated with CoinListing
            private static Property[] PROPERTIES = new PropertyTable.Property[]
            {
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Id),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // array containing all PropertyColumns associated with CoinListing
            private static PropertyColumn[] PROPERTY_COLUMNS = new PropertyTable.PropertyColumn[]
            {
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Id),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // underlying PropertyTable
            private PropertyTable propertyTable = new PropertyTable();

            public CoinListingTable()
            {
                // initialize table columns
                initTableColumns();
            }

            /// <summary>
            /// adds row corresponding to <paramref name="coinListing"/> to table.
            /// </summary>
            /// <seealso cref="PropertyTable.AddRow(PropertyRow)"/>
            /// <param name="coinListing"></param>
            public void AddRow(CoinListing coinListing)
            {
                PropertyRow propertyRow = getPropertyRow(coinListing);
                propertyTable.AddRow(propertyRow);
            }

            /// <summary>
            /// <para>
            /// removes row corresponding to <paramref name="coinListing"/> from table.
            /// </para>
            /// <para>
            /// returns whether row existed in table prior to being removed.
            /// </para>
            /// </summary>
            /// <param name="coinListing"></param>
            /// <returns>
            /// true if row corresponding to <paramref name="coinListing"/> existed in table prior to being removed,
            /// else false
            /// </returns>
            public bool RemoveRow(CoinListing coinListing)
            {
                PropertyRow propertyRow = getPropertyRow(coinListing);

                return propertyTable.RemoveRow(propertyRow);
            }

            /// <summary>
            /// returns a string representation of the table.
            /// </summary>
            /// <seealso cref="PropertyTable.GetTableDisplayString"/>
            /// <returns>
            /// string representation of the table
            /// </returns>
            public string GetTableDisplayString()
            {
                return propertyTable.GetTableDisplayString();
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="CoinListing"/> to table.
            /// </summary>
            private void initTableColumns()
            {
                propertyTable.AddColumnRange(PROPERTY_COLUMNS);
            }

            /// <summary>
            /// returns a new <see cref="PropertyRow"/> corresponding to <paramref name="coinListing"/>.
            /// </summary>
            /// <param name="coinListing"></param>
            /// <returns>
            /// <see cref="PropertyRow"/> corresponding to <paramref name="coinListing"/>
            /// </returns>
            private PropertyRow getPropertyRow(CoinListing coinListing)
            {
                // convert coinListing to a CoinData to get a CoinData type object
                CoinData coinData = new CoinData(coinListing);

                return new PropertyRow(coinData, PROPERTIES);
            }
        }
    }
}
