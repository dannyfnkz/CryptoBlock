﻿using CryptoBlock.CMCAPI;
using CryptoBlock.TableDisplay;
using CryptoBlock.Utils;
using CryptoBlock.Utils.CollectionUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public class CoinTickerTable
        {
            public enum eDisplayProperty
            {
                CirculatingSupply,
                PriceUsd,
                Volume24hUsd,
                PricePercentChange24hUsd
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

            private static readonly Type COIN_TICKER_PROPERTY_TYPE = typeof(CoinTicker);

            private static readonly Dictionary<eDisplayProperty, PropertyTable.Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, PropertyTable.Property>
                {
                    {
                        eDisplayProperty.CirculatingSupply,
                        new PropertyTable.Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "CirculatingSupply")
                    },
                    {
                        eDisplayProperty.PriceUsd,
                        new PropertyTable.Property(
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
                        new PropertyTable.Property(
                            COIN_TICKER_PROPERTY_TYPE,
                            "PricePercentChange24hUsd")
                    },
                };

            private static readonly PropertyTable.Property[] coinTickerProperties = 
                displayPropertyToProperty.Values.ToArray();

            private static readonly Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                {
                    {
                        eDisplayProperty.CirculatingSupply,
                        new PropertyTable.PropertyColumn(
                            "Circ. Supply",
                            15,
                            displayPropertyToProperty[eDisplayProperty.CirculatingSupply])
                    },
                    {
                        eDisplayProperty.PriceUsd,
                        new PropertyTable.PropertyColumn(
                            "Price USD",
                            14,
                            displayPropertyToProperty[eDisplayProperty.PriceUsd])
                    },
                    {
                        eDisplayProperty.Volume24hUsd,
                        new PropertyTable.PropertyColumn(
                            "Volume 24h (USD)",
                            18,
                            displayPropertyToProperty[eDisplayProperty.Volume24hUsd])
                    },
                    {
                        eDisplayProperty.PricePercentChange24hUsd,
                        new PropertyTable.PropertyColumn(
                            "% chg 24h",
                            11,
                            displayPropertyToProperty[eDisplayProperty.PricePercentChange24hUsd])
                    }
                };

            private PropertyTable propertyTable = new PropertyTable();

            public CoinTickerTable()
            {
                // initialize table columns
                initTableColumns();
            }

            public void AddCoinTickerRow(CoinTicker coinTicker)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(coinTicker);

                propertyTable.AddRow(propertyRow);
            }

            public bool RemoveCoinListingRow(CoinTicker coinTicker)
            {
                PropertyTable.PropertyRow propertyRow = getPropertyRow(coinTicker);

                return propertyTable.RemoveRow(propertyRow);
            }

            public string GetTableDisplayString()
            {
                return propertyTable.GetTableDisplayString();
            }

            public static PropertyTable.Property GetProperty(eDisplayProperty displayProperty)
            {
                return displayPropertyToProperty[displayProperty];
            }

            public static PropertyTable.PropertyColumn GetPropertyColumn(eDisplayProperty displayProperty)
            {
                return displayPropertyToPropertyColumn[displayProperty];
            }

            private void initTableColumns()
            {
                // add columns shared with CoinData
                initCoinDataPropertyColumns();

                // add columns unique to CoinTicker
                initCoinTickerPropertyColumns();
            }

            private void initCoinDataPropertyColumns()
            {
                propertyTable.AddColumnRange(COIN_DATA_PROPERTY_COLUMNS);
            }

            private void initCoinTickerPropertyColumns()
            {
                foreach (eDisplayProperty displayProperty in displayPropertyToPropertyColumn.Keys)
                {
                    PropertyTable.PropertyColumn propertyColumn =
                        displayPropertyToPropertyColumn[displayProperty];

                    propertyTable.AddColumn(propertyColumn);
                }
            }

            private PropertyTable.PropertyRow getPropertyRow(CoinTicker coinTicker)
            {
                // construct row property list

                // merge coin data properties with coin ticker properties
                PropertyTable.Property[] rowProperties = CollectionUtils.MergeToArray(
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
                return new PropertyTable.PropertyRow(rowObjects, rowProperties);
            }
        }
    }
}
