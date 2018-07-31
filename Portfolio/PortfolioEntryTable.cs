using CryptoBlock.CMCAPI;
using CryptoBlock.ServerDataManagement;
using CryptoBlock.TableDisplay;
using CryptoBlock.Utils.CollectionUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using static CryptoBlock.TableDisplay.PropertyTable;

namespace CryptoBlock
{
    namespace PortfolioManagement
    {
        /// <summary>
        /// represents a table displaying <see cref="PortfolioEntry"/> data.
        /// </summary>
        public class PortfolioEntryTable
        {
            /// <summary>
            /// represents a row holding data of a single <see cref="PortfolioEntry"/>.
            /// </summary>
            private class PortfolioEntryPropertyRow : PropertyRow
            {
                internal PortfolioEntryPropertyRow(PortfolioEntry portfolioEntry)
                    : base(constructObjectArray(portfolioEntry), constructPropertyArray())
                {

                }

                /// <summary>
                /// returns <see cref="PortfolioEntryPropertyRow"/> <see cref="Property"/> array.
                /// </summary>
                /// <returns>
                /// row <see cref="Property"/> array
                /// </returns>
                private static Property[] constructPropertyArray()
                {
                    // merge coin data properties with coin ticker properties and portfolio entry properties
                    Property[] rowProperties = CollectionUtils.MergeToArray(
                        COIN_DATA_PROPERTIES,
                        COIN_TICKER_PROPERTIES,
                        portfolioEntryProperties);

                    return rowProperties;
                }

                /// <summary>
                /// returns <see cref="PortfolioEntryPropertyRow"/> <see cref="object"/> array.
                /// </summary>
                /// <param name="portfolioEntry"></param>
                /// <returns>
                /// row <see cref="object"/> array
                /// </returns>
                /// <seealso cref="constructCoinDataArray(PortfolioEntry)"/>
                /// <seealso cref="constructCoinTickerArray(PortfolioEntry)"/>
                /// <seealso cref="constructPortfolioEntryArray(PortfolioEntry)"/>
                private static object[] constructObjectArray(PortfolioEntry portfolioEntry)
                {
                    CoinData[] coinDataArray = constructCoinDataArray(portfolioEntry);
                    CoinTicker[] coinTickerArray = constructCoinTickerArray(portfolioEntry);
                    PortfolioEntry[] portfolioEntryArray = constructPortfolioEntryArray(portfolioEntry);

                    // merge CoinData, CoinTicker, PortfolioEntry arrays together
                    object[] objectArray = CollectionUtils.MergeToArray<object>(
                        coinDataArray,
                        coinTickerArray,
                        portfolioEntryArray);

                    return objectArray;
                }

                /// <summary>
                /// returns <see cref="CoinData"/> section of <see cref="PortfolioEntryPropertyRow"/>
                /// <see cref="object"/> array.
                /// </summary>
                /// <seealso cref="CollectionUtils.DuplicateToArray{T}(T, int)"/>
                /// <param name="portfolioEntry"></param>
                /// <returns>
                /// <see cref="CoinData"/> section of <see cref="PortfolioEntryPropertyRow"/> 
                /// <see cref="object"/> array.
                /// </returns>
                private static CoinData[] constructCoinDataArray(PortfolioEntry portfolioEntry)
                {
                    // get coin listing corresponding to PortfolioEntry coin id
                    CoinListing coinListing = CoinListingManager.Instance.GetCoinListing(portfolioEntry.CoinId);

                    // create a corresponding CoinData object
                    CoinData coinData = new CoinData(coinListing);

                    // construct CoinData array
                    CoinData[] coinDataArray = CollectionUtils.DuplicateToArray(
                        coinData,
                        COIN_DATA_PROPERTIES.Length);

                    return coinDataArray;
                }

                /// <summary>
                /// returns <see cref="CoinTicker"/> section of <see cref="PortfolioEntryPropertyRow"/>
                /// <see cref="object"/> array.
                /// </summary>
                /// <seealso cref="CollectionUtils.DuplicateToArray{T}(T, int)"/>
                /// <param name="portfolioEntry"></param>
                /// <returns>
                /// <see cref="CoinTicker"/> section of <see cref="PortfolioEntryPropertyRow"/>
                /// <see cref="object"/> array.
                /// </returns>
                private static CoinTicker[] constructCoinTickerArray(PortfolioEntry portfolioEntry)
                {
                    // if coin ticker is not available, its properties are displaye as 'N\A'
                    CoinTicker coinTicker = CoinTickerManager.Instance.HasCoinTicker(portfolioEntry.CoinId)
                        ? CoinTickerManager.Instance.GetCoinTicker(portfolioEntry.CoinId)
                        : null;

                    // construct coin ticker array
                    CoinTicker[] coinTickerArray = CollectionUtils.DuplicateToArray(
                        coinTicker,
                        COIN_TICKER_PROPERTIES.Length);

                    return coinTickerArray;
                }

                /// <summary>
                /// returns <see cref="PortfolioEntry"/> section of <see cref="PortfolioEntryPropertyRow"/>
                /// <see cref="object"/> array. 
                /// </summary>
                /// <seealso cref="CollectionUtils.DuplicateToArray{T}(T, int)"/>
                /// <param name="portfolioEntry"></param>
                /// <returns>
                /// <see cref="PortfolioEntry"/> section of <see cref="PortfolioEntryPropertyRow"/>
                /// <see cref="object"/> array. 
                /// </returns>
                private static PortfolioEntry[] constructPortfolioEntryArray(PortfolioEntry portfolioEntry)
                {
                    // construct PortfolioEntry array
                    PortfolioEntry[] portfolioEntryArray = CollectionUtils.DuplicateToArray(
                        portfolioEntry,
                        portfolioEntryProperties.Length);

                    return portfolioEntryArray;
                }
            }

            /// <summary>
            /// represents a column in <see cref="PortfolioEntryTable"/>, corresponding to a property of
            /// <see cref="PortfolioEntry"/>.
            /// </summary>
            private enum eDisplayProperty
            {
                ProfitPercentageUsd,
                Holdings
            }

            // properties of CoinData which represent a column in table
            private static readonly Property[] COIN_DATA_PROPERTIES =
                new Property[]
            {
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // property columns corresponding to CoinData properties
            private static readonly PropertyColumn[] COIN_DATA_PROPERTY_COLUMNS =
                new PropertyColumn[]
            {
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // properties of CoinTicker which represent a column in table
            private static readonly Property[] COIN_TICKER_PROPERTIES =
                new Property[]
            {
                CoinTickerTable.GetProperty(CoinTickerTable.eDisplayProperty.PriceUsd),
                CoinTickerTable.GetProperty(CoinTickerTable.eDisplayProperty.PricePercentChange24hUsd)
            };

            // property columns corresponding to CoinTicker properties
            private static readonly PropertyColumn[] COIN_TICKER_PROPERTY_COLUMNS =
                new PropertyColumn[]
            {
                CoinTickerTable.GetPropertyColumn(CoinTickerTable.eDisplayProperty.PriceUsd),
                CoinTickerTable.GetPropertyColumn(CoinTickerTable.eDisplayProperty.PricePercentChange24hUsd)
            };

            // type of PortfolioEntry properties
            private static readonly Type PORTFOLIO_ENTRY_PROPERTY_TYPE = typeof(PortfolioEntry);

            // eDisplayProperty to Property mapping
            private static readonly Dictionary<eDisplayProperty, Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, Property>
                {
                    {
                        eDisplayProperty.Holdings,
                        new Property(
                            PORTFOLIO_ENTRY_PROPERTY_TYPE,
                            "Holdings")
                    },
                    {
                        eDisplayProperty.ProfitPercentageUsd,
                        new Property(
                            PORTFOLIO_ENTRY_PROPERTY_TYPE,
                            "ProfitPercentageUsd")
                    }
                };

            // array of PortfolioEntry properties, each representing a column in the table
            private static readonly Property[] portfolioEntryProperties =
                displayPropertyToProperty.Values.ToArray();

            // eDisplayProperty to PropertyColumn mapping
            private static readonly Dictionary<eDisplayProperty, PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyColumn>
                {
                    {
                        eDisplayProperty.Holdings,
                        new PropertyColumn(
                            "Holdings",
                            13,
                            displayPropertyToProperty[eDisplayProperty.Holdings])
                    },
                    {
                        eDisplayProperty.ProfitPercentageUsd,
                        new PropertyColumn(
                            "% Profit (USD)",
                            17,
                            displayPropertyToProperty[eDisplayProperty.ProfitPercentageUsd])
                    }
                };

            // underlying PropertyTable
            private PropertyTable propertyTable = new PropertyTable();

            public PortfolioEntryTable()
            {
                // add property columns
                addPropertyColumns();
            }

            /// <summary>
            /// adds a new <see cref="Row"/> with <paramref name="portfolioEntry"/>'s data to table.
            /// </summary>
            /// <seealso cref="PropertyTable.AddRow(PropertyRow)"/>
            /// <param name="portfolioEntry"></param>
            public void AddRow(PortfolioEntry portfolioEntry)
            {
                   PortfolioEntryPropertyRow portfolioEntryPropertyRow =
                    new PortfolioEntryPropertyRow(portfolioEntry);

                propertyTable.AddRow(portfolioEntryPropertyRow);
            }

            /// <summary>
            /// <para>
            /// removes <see cref="Row"/> corresponding to <paramref name="portfolioEntry"/> from 
            /// table, if it exists there.
            /// </para>
            /// <para>
            /// returns whether <see cref="Row"/> corresponding to <paramref name="portfolioEntry"/> existed in table
            /// prior to being removed.
            /// </para>
            /// </summary>
            /// <param name="portfolioEntry"></param>
            /// <returns>
            /// true if <see cref="Row"/> with <paramref name="portfolioEntry"/>'s data existed in table
            /// prior to being removed,
            /// else false
            /// </returns>
            public bool RemoveRow(PortfolioEntry portfolioEntry)
            {
                PortfolioEntryPropertyRow portfolioEntryPropertyRow =
                   new PortfolioEntryPropertyRow(portfolioEntry);

                return propertyTable.RemoveRow(portfolioEntryPropertyRow);
            }

            /// <summary>
            /// returns a string representation of the table.
            /// </summary>
            /// <seealso cref="PropertyTable.GetTableDisplayString"/>
            /// <returns>
            /// a string representation of the table
            /// </returns>
            public string GetTableDisplayString()
            {
                return propertyTable.GetTableDisplayString();
            }

            /// <summary>
            /// adds all <see cref="PropertyColumn"/>s to table.
            /// </summary>
            private void addPropertyColumns()
            {
                // add property columns corresponding to CoinData properties
                addCoinDataPropertyColumns();

                // add property columns corresponding to CoinTicker properties
                addCoinTickerPropertyColumns();

                // add property columns corresponding to PortfolioEntry properties
                addPortfolioEntryPropertyColumns();
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="CoinData"/> properties
            /// to table.
            /// </summary>
            private void addCoinDataPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_DATA_PROPERTY_COLUMNS);
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="CoinTicker"/> properties
            /// to table.
            /// </summary>
            private void addCoinTickerPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_TICKER_PROPERTY_COLUMNS);
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="PortfolioEntry"/> properties
            /// to table.
            /// </summary>
            private void addPortfolioEntryPropertyColumns()
            {
                // add each property column corresponding to PortfolioEntry
                foreach (eDisplayProperty displayProperty in displayPropertyToPropertyColumn.Keys)
                {
                    PropertyColumn propertyColumn =
                        displayPropertyToPropertyColumn[displayProperty];

                    propertyTable.AddColumn(propertyColumn);
                }
            }
        }
    }
}
