using CryptoBlock.CMCAPI;
using CryptoBlock.TableDisplay;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class CoinListingTable
        {
            private static PropertyTable.Property[] PROPERTIES = new PropertyTable.Property[]
            {
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Id),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Symbol)
            };
            private static PropertyTable.PropertyColumn[] PROPERTY_COLUMNS = new PropertyTable.PropertyColumn[]
            {
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Id),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            private PropertyTable propertyTable = new PropertyTable();

            public CoinListingTable()
            {
                // initialize table columns
                initTableColumns();
            }

            public void AddCoinListingRow(CoinListing coinListing)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(coinListing);
                propertyTable.AddRow(propertyRow);
            }

            public bool RemoveCoinListingRow(CoinListing coinListing)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(coinListing);

                return propertyTable.RemoveRow(propertyRow);
            }

            public string GetTableDisplayString()
            {
                return propertyTable.GetTableDisplayString();
            }

            private void initTableColumns()
            {
                propertyTable.AddColumnRange(PROPERTY_COLUMNS);
            }

            private PropertyTable.PropertyRow getPropertyRow(CoinListing coinListing)
            {
                // convert coinListing to a CoinData object as type of PROPERTIES is CoinData
                CoinData coinData = new CoinData(coinListing);

                return new PropertyTable.PropertyRow(coinData, PROPERTIES);
            }
        }
    }
}
