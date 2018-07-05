using CryptoBlock.CMCAPI;
using CryptoBlock.TableDisplay;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        public static class CoinDataDisplay
        {
            public enum eDisplayProperty
            {
                Id,
                Name,
                Symbol
            }

            public static readonly Type COLUMN_PROPERTY_TYPE = typeof(CoinData);

            private static readonly Dictionary<eDisplayProperty, PropertyTable.Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, PropertyTable.Property>
                {
                    {
                        eDisplayProperty.Id,
                        new PropertyTable.Property(
                            COLUMN_PROPERTY_TYPE,
                            "Id")
                    },
                    {
                        eDisplayProperty.Name,
                        new PropertyTable.Property(
                            COLUMN_PROPERTY_TYPE,
                            "Name")
                    },
                    {
                        eDisplayProperty.Symbol,
                        new PropertyTable.Property(
                            COLUMN_PROPERTY_TYPE,
                            "Symbol")
                    }
                };

            private static readonly PropertyTable.Property[] properties =
                displayPropertyToProperty.Values.ToArray();

            private static Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyTable.PropertyColumn>
                {
                    {
                        eDisplayProperty.Id,
                        new PropertyTable.PropertyColumn(
                            "ID",
                            8,
                            displayPropertyToProperty[eDisplayProperty.Id])
                    },
                    {
                        eDisplayProperty.Name,
                        new PropertyTable.PropertyColumn(
                            "Name",
                            13,
                            displayPropertyToProperty[eDisplayProperty.Name])
                    },
                    {
                        eDisplayProperty.Symbol,
                        new PropertyTable.PropertyColumn(
                            "Symbol",
                            8,
                            displayPropertyToProperty[eDisplayProperty.Symbol])
                    }
                };

            public static PropertyTable.Property[] Properties
            {
                get { return properties; }
            }

            public static PropertyTable.Property GetProperty(eDisplayProperty displayProperty)
            {
                return displayPropertyToProperty[displayProperty];
            }


            public static PropertyTable.PropertyColumn GetPropertyColumn(eDisplayProperty displayProperty)
            {
                return displayPropertyToPropertyColumn[displayProperty];
            }
        }
    }
}

