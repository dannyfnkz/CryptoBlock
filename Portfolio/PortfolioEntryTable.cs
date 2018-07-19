using CryptoBlock.CMCAPI;
using CryptoBlock.PortfolioManagement;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.TableDisplay;
using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        public class PortfolioEntryTable
        {
            private enum eDisplayProperty
            {
                ProfitPercentageUsd
            }

            private static readonly PropertyTable.Property[] COIN_DATA_PROPERTIES =
                new PropertyTable.Property[]
            {
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Symbol)
            };
            private static readonly PropertyTable.PropertyColumn[] COIN_DATA_PROPERTY_COLUMNS =
                new PropertyTable.PropertyColumn[]
            {
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Symbol)
            };
            private static readonly PropertyTable.Property[] COIN_TICKER_PROPERTIES =
                new PropertyTable.Property[]
            {
                CoinTickerTable.GetProperty(CoinTickerTable.eDisplayProperty.PriceUsd),
                CoinTickerTable.GetProperty(CoinTickerTable.eDisplayProperty.PercentChange24hUsd)
            };
            private static readonly PropertyTable.PropertyColumn[] COIN_TICKER_PROPERTY_COLUMNS =
                new PropertyTable.PropertyColumn[]
            {
                CoinTickerTable.GetPropertyColumn(CoinTickerTable.eDisplayProperty.PriceUsd),
                CoinTickerTable.GetPropertyColumn(CoinTickerTable.eDisplayProperty.PercentChange24hUsd)
            };

            private static readonly Type PORTFOLIO_ENTRY_PROPERTY_TYPE = typeof(PortfolioEntry);

            private static readonly Dictionary<eDisplayProperty, PropertyTable.Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, PropertyTable.Property>
                {
                    {
                        eDisplayProperty.ProfitPercentageUsd,
                        new PropertyTable.Property(
                            PORTFOLIO_ENTRY_PROPERTY_TYPE,
                            "ProfitPercentageUsd")
                    }
                };

            private static readonly PropertyTable.Property[] portfolioEntryProperties =
                displayPropertyToProperty.Values.ToArray();

            private static readonly Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                {
                    {
                        eDisplayProperty.ProfitPercentageUsd,
                        new PropertyTable.PropertyColumn(
                            "% Profit (USD)",
                            17,
                            displayPropertyToProperty[eDisplayProperty.ProfitPercentageUsd])
                    }
                };

            private PropertyTable propertyTable = new PropertyTable();

            public PortfolioEntryTable()
            {
                // initialize table columns
                initTableColumns();
            }

            public void AddPortfolioEntryRow(PortfolioEntry portfolioEntry)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(portfolioEntry);

                propertyTable.AddRow(propertyRow);
            }

            public bool RemovePortfolioEntryRow(PortfolioEntry portfolioEntry)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(portfolioEntry);

                return propertyTable.RemoveRow(propertyRow);
            }

            public string GetTableDisplayString()
            {
                return propertyTable.GetTableString();
            }

            private void initTableColumns()
            {
                // add columns shared with CoinData
                initCoinDataPropertyColumns();

                // add columns shared with CoinTicker
                initCoinTickerPropertyColumns();

                // add columns unique to CoinTicker
                initPortfolioEntryPropertyColumns();
            }

            private void initCoinDataPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_DATA_PROPERTY_COLUMNS);
            }

            private void initCoinTickerPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_TICKER_PROPERTY_COLUMNS);
            }

            private void initPortfolioEntryPropertyColumns()
            {
                foreach (eDisplayProperty displayProperty in displayPropertyToPropertyColumn.Keys)
                {
                    PropertyTable.PropertyColumn propertyColumn =
                        displayPropertyToPropertyColumn[displayProperty];

                    propertyTable.AddColumn(propertyColumn);
                }
            }

            private PropertyTable.PropertyRow getPropertyRow(PortfolioEntry portfolioEntry)
            {
                // construct row property list

                // merge coin data properties with coin ticker properties and portfolio entry properties
                PropertyTable.Property[] rowProperties = CollectionUtils.MergeToArray(
                    COIN_DATA_PROPERTIES,
                    COIN_TICKER_PROPERTIES,
                    portfolioEntryProperties);


                // construct row object list
                // get coin data object array
                CoinData coinData = new CoinData(portfolioEntry.CoinTicker);
                CoinData[] coinDataObjectArray = CollectionUtils.DuplicateToArray(
                    coinData,
                    COIN_DATA_PROPERTIES.Length);

                // get coin ticker object array
                CoinTicker[] coinTickerObjectArray = CollectionUtils.DuplicateToArray(
                    portfolioEntry.CoinTicker,
                    COIN_TICKER_PROPERTIES.Length);

                // get portfolio entry object array
                PortfolioEntry[] portfolioEntryObjectArray = CollectionUtils.DuplicateToArray(
                    portfolioEntry,
                    portfolioEntryProperties.Length);

                // merge CoinData, CoinTicker, PortfolioEntry object arrays together
                object[] rowObjects = CollectionUtils.MergeToArray<object>(
                    coinDataObjectArray,
                    coinTickerObjectArray,
                    portfolioEntryObjectArray);

                // construct property row
                return new PropertyTable.PropertyRow(rowObjects, rowProperties);
            }

        }
    }
}
