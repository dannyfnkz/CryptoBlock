using CryptoBlock.CMCAPI;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections;
using CryptoBlock.Utils.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using static CryptoBlock.Utils.Tables.PropertyTable;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// represents a table displaying <see cref="CoinTicker"/> data.
        /// </summary>
        public class CoinTickerTable
        {
            public enum eDisplayProperty
            {
                CirculatingSupply,
                PriceUsd,
                Volume24hUsd,
                PricePercentChange24hUsd
            }

            // Properties associated with CoinData
            private static readonly Property[] COIN_DATA_PROPERTIES =
                new Property[]
            {
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetProperty(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // PropertyColumns associated with CoinData
            private static readonly PropertyColumn[] COIN_DATA_PROPERTY_COLUMNS =
                new PropertyColumn[]
            {
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Name),
                CoinDataDisplay.GetPropertyColumn(CoinDataDisplay.eDisplayProperty.Symbol)
            };

            // type of CoinTicker class
            private static readonly Type COIN_TICKER_PROPERTY_TYPE = typeof(CoinTicker);

            // eDisplayProperty to Property mapping
            private static readonly Dictionary<eDisplayProperty, Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, Property>
                {
                    {
                        eDisplayProperty.CirculatingSupply,
                        new Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "CirculatingSupply")
                    },
                    {
                        eDisplayProperty.PriceUsd,
                        new Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "PriceUsd")
                    },
                    {
                        eDisplayProperty.Volume24hUsd,
                        new PropertyTable.Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "Volume24hUsd")
                    },
                    {
                        eDisplayProperty.PricePercentChange24hUsd,
                        new Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "PricePercentChange24hUsd")
                    },
                };

            // array of Properties associated with CoinTicker
            private static readonly Property[] coinTickerProperties =
                Enumerable.ToArray(displayPropertyToProperty.Values);

            // eDisplayProperty to PropertyColumn mapping
            private static readonly Dictionary<eDisplayProperty, PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                {
                    {
                        eDisplayProperty.CirculatingSupply,
                        new PropertyColumn(
                            "Circ. Supply",
                            12,
                            displayPropertyToProperty[eDisplayProperty.CirculatingSupply],
                            string.Empty)
                    },
                    {
                        eDisplayProperty.PriceUsd,
                        new PropertyColumn(
                            "Price (USD)",
                            11,
                            displayPropertyToProperty[eDisplayProperty.PriceUsd],
                            string.Empty)
                    },
                    {
                        eDisplayProperty.Volume24hUsd,
                        new PropertyColumn(
                            "Volume 24h (USD)",
                            16,
                            displayPropertyToProperty[eDisplayProperty.Volume24hUsd],
                            string.Empty)
                    },
                    {
                        eDisplayProperty.PricePercentChange24hUsd,
                        new PropertyTable.PropertyColumn(
                            "% chg 24h",
                            9,
                            displayPropertyToProperty[eDisplayProperty.PricePercentChange24hUsd],
                            string.Empty)
                    }
                };

            // underlying PropertyTable
            private PropertyTable propertyTable = new PropertyTable();

            public CoinTickerTable()
            {
                // initialize table columns
                initTableColumns();
            }

            /// <summary>
            /// adds a new <see cref="Row"/> with <paramref name="coinTicker"/>'s data to table.
            /// </summary>
            /// <seealso cref="PropertyTable.AddRow(PropertyRow)"/>
            /// <param name="coinTicker"></param>
            public void AddRow(CoinTicker coinTicker)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(coinTicker);

                propertyTable.AddRow(propertyRow);
            }

            /// <para>
            /// removes <see cref="Row"/> corresponding to <paramref name="coinTicker"/> from 
            /// table, if it exists there.
            /// </para>
            /// <para>
            /// returns whether <see cref="Row"/> corresponding to <paramref name="coinTicker"/> existed in table
            /// prior to being removed.
            /// </para>
            /// <param name="coinTicker"></param>
            /// <returns>
            /// true if <see cref="Row"/> corresponding to <paramref name="coinTicker"/> existed in table
            /// prior to being removed,
            /// else false
            /// </returns>
            public bool RemoveRow(CoinTicker coinTicker)
            {
                PropertyRow propertyRow = getPropertyRow(coinTicker);

                return propertyTable.RemoveRow(propertyRow);
            }

            /// <summary>
            /// returns a string representation of the table.
            /// </summary>
            /// <returns>
            /// string representation of the table
            /// </returns>
            public string GetTableDisplayString()
            {
                return propertyTable.GetTableDisplayString();
            }

            /// <summary>
            /// returns <see cref="Property"/> corresponding to <paramref name="displayProperty"/>.
            /// </summary>
            /// <param name="displayProperty"></param>
            /// <returns>
            /// <see cref="Property"/> corresponding to <paramref name="displayProperty"/>.
            /// </returns>
            public static Property GetProperty(eDisplayProperty displayProperty)
            {
                return displayPropertyToProperty[displayProperty];
            }

            /// <summary>
            /// returns <see cref="PropertyColumn"/> corresponding to <paramref name="displayProperty"/>.
            /// </summary>
            /// <param name="displayProperty"></param>
            /// <returns>
            /// <see cref="PropertyColumn"/> corresponding to <paramref name="displayProperty"/>.
            /// </returns>
            public static PropertyColumn GetPropertyColumn(eDisplayProperty displayProperty)
            {
                return displayPropertyToPropertyColumn[displayProperty];
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s to table.
            /// </summary>
            private void initTableColumns()
            {
                // add columns shared with CoinData
                initCoinDataPropertyColumns();

                // add columns unique to CoinTicker
                initCoinTickerPropertyColumns();
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="CoinData"/> to table.
            /// </summary>
            private void initCoinDataPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_DATA_PROPERTY_COLUMNS);
            }

            /// <summary>
            /// adds <see cref="PropertyColumn"/>s corresponding to <see cref="CoinTicker"/> to table.
            /// </summary>
            private void initCoinTickerPropertyColumns()
            {
                // add PropertyColumns corresponding to CoinTicker to table
                foreach (eDisplayProperty displayProperty in displayPropertyToPropertyColumn.Keys)
                {
                    PropertyColumn propertyColumn =
                        displayPropertyToPropertyColumn[displayProperty];

                    propertyTable.AddColumn(propertyColumn);
                }
            }

            /// <summary>
            /// returns a new <see cref="PropertyRow"/> constructed from <paramref name="coinTicker"/>.
            /// </summary>
            /// <param name="coinTicker"></param>
            /// <returns>
            /// new <see cref="PropertyRow"/> constructed from <paramref name="coinTicker"/>
            /// </returns>
            private PropertyRow getPropertyRow(CoinTicker coinTicker)
            {
                // construct row property list

                // merge coin data properties with coin ticker properties
                Property[] rowProperties = CollectionUtils.MergeToArray(
                    COIN_DATA_PROPERTIES,
                    coinTickerProperties);


                // construct row object list
                // get coin data object array
                CoinData coinData = new CoinData(coinTicker);
                CoinData[] coinDataObjectArray = CollectionUtils.DuplicateToArray(
                    coinData,
                    COIN_DATA_PROPERTIES.Length);

                // get coin ticker object array
                CoinTicker[] coinTickerObjectArray = CollectionUtils.DuplicateToArray(
                    coinTicker,
                    coinTickerProperties.Length);

                // merge coin data object array with coin ticker object array
                object[] rowObjects = CollectionUtils.MergeToArray(coinDataObjectArray, coinTickerObjectArray);

                // construct property row
                return new PropertyRow(rowObjects, rowProperties);
            }
        }
    }
}
