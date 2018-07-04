using CryptoBlock.CMCAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace TableDisplay
    {
        internal static class CoinDataDisplay
        {
            public enum eDisplayProperty
            {
                Id,
                Name,
                Symbol
            }

            internal static readonly Type COLUMN_PROPERTY_TYPE = typeof(CoinData);

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

            internal static PropertyTable.Property[] Properties
            {
                get { return properties; }
            }

            internal static PropertyTable.Property GetProperty(eDisplayProperty displayProperty)
            {
                return displayPropertyToProperty[displayProperty];
            }


            internal static PropertyTable.PropertyColumn GetPropertyColumn(eDisplayProperty displayProperty)
            {
                return displayPropertyToPropertyColumn[displayProperty];
            }
        }
    }
}

