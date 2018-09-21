using CryptoBlock.CMCAPI;
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
        /// handles table display functionality for <see cref="CoinData"/> properties. 
        /// </summary>
        public static class CoinDataDisplay
        {
            public enum eDisplayProperty
            {
                Id,
                Name,
                Symbol
            }

            // common Type of CoinData column properties
            public static readonly Type COLUMN_PROPERTY_TYPE = typeof(CoinData);

            // eDisplayProperty to Property mapping
            private static readonly Dictionary<eDisplayProperty, Property>
                displayPropertyToProperty = new Dictionary<eDisplayProperty, Property>
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

            // array containing all properties associated with CoinData 
            private static readonly PropertyTable.Property[] properties =
                displayPropertyToProperty.Values.ToArray();

            // eDisplayProperty to PropertyColumn mapping 
            private static Dictionary<eDisplayProperty, PropertyColumn>
                displayPropertyToPropertyColumn = new Dictionary<eDisplayProperty, PropertyColumn>
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
                            6,
                            displayPropertyToProperty[eDisplayProperty.Symbol])
                    }
                };

            /// <summary>
            ///  array containing all <see cref="CoinData"/> properties.
            /// </summary>
            public static Property[] Properties
            {
                get { return properties; }
            }

            /// <summary>
            /// returns <see cref="Property"/> corresponding to <paramref name="displayProperty"/>.
            /// </summary>
            /// <param name="displayProperty"></param>
            /// <returns>
            /// <see cref="Property"/> corresponding to <paramref name="displayProperty"/>
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
            /// <see cref="PropertyColumn"/> corresponding to <paramref name="displayProperty"/>
            /// </returns>
            public static PropertyColumn GetPropertyColumn(eDisplayProperty displayProperty)
            {
                return displayPropertyToPropertyColumn[displayProperty];
            }
        }
    }
}

