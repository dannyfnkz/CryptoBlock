using System.Collections.Generic;
using System;
using System.Text;
using System.Xml;
using CryptoBlock.Utils.Strings;
using CryptoBlock.Utils.IO.SQLite.Xml;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes;
using static CryptoBlock.Utils.EnumUtils;
using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas.ColumnSchemas
    {
        /// <summary>
        /// represents an SQLite column schema.
        /// </summary>
        public class ColumnSchema : Schema
        {
            /// <summary>
            /// thrown in case a <see cref="ColumnSchema"/> parse fails.
            /// </summary>
            public class ColumnSchemaParseExcetion : SQLiteParseExcetion
            {
                public ColumnSchemaParseExcetion(
                    string additionalDetails = null,
                    Exception innerException = null)
                    : base(typeof(ColumnSchemaParseExcetion), additionalDetails, innerException)
                {

                }
            }

            // type of data column holds
            public enum eDataType
            {
                Integer, Varchar, Real, Char
            };

            // ColumnSchema for the id (or ROWID) column
            private static readonly ColumnSchema ID_COLUMN_SCHEMA = new ColumnSchema(
                "_id",
                eDataType.Integer);

            private readonly string name;
            private readonly eDataType dataType;
            private readonly long? length;
            private readonly bool notNull;
            private readonly bool unique;
            private readonly bool autoIncrement;
            private readonly object defaultValue;

            internal ColumnSchema(
                string name,
                eDataType dataType,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                object defaultValue = null,
                long? length = null)
            {
                this.name = name;
                this.dataType = dataType;
                this.notNull = notNull;
                this.autoIncrement = autoIncrement;
                this.unique = unique;
                this.defaultValue = defaultValue;
                this.length = length;
            }

            /// <summary>
            /// ColumnSchema for the id (or ROWID) column.
            /// </summary>
            public static ColumnSchema IdColumnSchema
            {
                get { return ID_COLUMN_SCHEMA; }
            }

            /// <summary>
            /// column name.
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// type of data associated with column.
            /// </summary>
            public eDataType DataType
            {
                get { return dataType; }
            }

            /// <summary>
            /// whether column value must not be null.
            /// </summary>
            public bool NotNull
            {
                get { return notNull; }
            }

            /// <summary>
            /// whether column value should be automatically calculated, getting a value
            /// larger than the corresonding column value of the previously inserted row.
            /// </summary>
            public bool AutoIncrement
            {
                get { return autoIncrement; }
            }

            /// <summary>
            /// whether column value must be unique for each row in table.
            /// </summary>
            public bool Unique
            {
                get { return unique; }
            }

            /// <summary>
            /// default column value, when a column value is not specified in the INSERT query.
            /// </summary>
            public object DefaultValue
            {
                get { return defaultValue; }
            }

            /// <summary>
            /// length of column <see cref="DataType"/>.
            /// </summary>
            public long? Length
            {
                get { return length; }
            }

            /// <summary>
            /// parses a <see cref="ColumnSchema"/> from <paramref name="columnSchemaXmlNode"/>.
            /// </summary>
            /// <param name="columnSchemaXmlNode"></param>
            /// <returns>
            /// <see cref="ColumnSchema"/> parsed from <paramref name="columnSchemaXmlNode"/>
            /// </returns>
            /// <exception cref="ColumnSchemaParseExcetion">
            /// thrown if <see cref="ColumnSchema"/> parse failed
            /// </exception>
            public static ColumnSchema Parse(XmlNode columnSchemaXmlNode)
            {
                assertContainsRequiredAttributes(columnSchemaXmlNode, "name", "type");

                // get required attributes
                string name = columnSchemaXmlNode.GetAttributeValue("name");
                string typeString = columnSchemaXmlNode.GetAttributeValue("type");

                try
                {
                    // parse typeString into valid enum format
                    // convert typeString from a single-word string having all letters in uppercase
                    // to a single word in lowercase (which is equivalent to camelCase)
                    // and then to an enum value name format
                    typeString = EnumUtils.camelCaseStringToEnumValueNameFormat(typeString.ToLower());
                    eDataType dataType = EnumUtils.ParseEnum<eDataType>(typeString);

                    // get optional attributes
                    bool notNull = false;
                    bool autoIncrement = false;
                    bool unique = false;
                    object defaultValue = null;
                    long? length = null;
                   
                    if (columnSchemaXmlNode.ContainsNodes("not_null"))
                    {
                        notNull = true;
                    }
                    if (columnSchemaXmlNode.ContainsNodes("auto_increment"))
                    {
                        autoIncrement = true;
                    }
                    if (columnSchemaXmlNode.ContainsNodes("unique"))
                    {
                        unique = true;
                    }
                    if (columnSchemaXmlNode.ContainsNodes("default"))
                    {
                        XmlNode defaultValueXmlNode = columnSchemaXmlNode.SelectNodes("default")[0];
                        defaultValue = defaultValueXmlNode.FirstChild.Value;
                    }
                    if (columnSchemaXmlNode.ContainsNodes("length"))
                    {
                        XmlNode lengthXmlNode = columnSchemaXmlNode.SelectNodes("length")[0];
                        length = long.Parse(lengthXmlNode.FirstChild.Value);
                    }

                    ColumnSchema columnSchema = new ColumnSchema(
                        name,
                        dataType,
                        notNull,
                        autoIncrement,
                        unique,
                        defaultValue,
                        length);

                    return columnSchema;
                }
                // a required attribute was missing from XmlNode
                catch (XmlNodeMissingAttributeException missingAttributeException)
                {
                    string additionalDetails = string.Format(
                        "Required attribute '{0}' was missing from XmlNode.",
                        missingAttributeException.AttributeName);
                    throw new ColumnSchemaParseExcetion(additionalDetails, missingAttributeException);
                }
                catch(EnumParseException enumParseException) // invalid typeString (eDataType parse failed)
                {
                    string additionalDetails = string.Format(
                        "(Column '{0}') had an Invalid data type value: '{1}'.",
                        name,
                        typeString);
                    throw new ColumnSchemaParseExcetion(additionalDetails, enumParseException);
                }
            }

            public static string DataTypeToString(eDataType dataType)
            {
                return Enum.GetName(typeof(eDataType), dataType).ToUpper();
            }

            /// <summary>
            /// returns a copy of <paramref name="columnSchema"/>, having no constraints.
            /// </summary>
            /// <param name="columnSchema"></param>
            /// <returns>
            /// copy of <paramref name="columnSchema"/>, having no constraints
            /// </returns>
            internal static ColumnSchema GetColumnSchemaWithConstraintsStripped(ColumnSchema columnSchema)
            {
                ColumnSchema columnSchemaWithConstraintsStripped = new ColumnSchema(
                    columnSchema.Name,
                    columnSchema.DataType,
                    false,
                    false,
                    false,
                    false,
                    columnSchema.Length);

                return columnSchemaWithConstraintsStripped;
            }

            protected override string BuildExpressionString()
            {
                string typeString = DataTypeToString(this.dataType);

                StringBuilder queryStringBuilder = new StringBuilder();

                // append column name and type
                queryStringBuilder.AppendFormat("{0} {1}", this.Name, typeString);

                if (this.length != null) // append column length if not null
                {
                    queryStringBuilder.AppendFormat("({0})", this.length.GetValueOrDefault());
                }
                if (this.autoIncrement) // append auto-increment constraint if enabled
                {
                    queryStringBuilder.AppendFormat(" {0}", "AUTO_INCREMENT");
                }
                if (this.notNull) // append not null constraint if enabled
                {
                    queryStringBuilder.AppendFormat(" {0}", "NOT NULL");
                }
                if (this.defaultValue != null) // append default value constraint if not null
                {
                    queryStringBuilder.AppendFormat(" DEFAULT '{0}'", this.defaultValue.ToString());
                }
                if (this.unique) // append unique constraint if enabled
                {
                    queryStringBuilder.AppendFormat(" {0}", "UNIQUE");
                }

                return queryStringBuilder.ToString();
            }

            /// <summary>
            /// asserts that <paramref name="columnSchemaXmlNode"/> contains required attributes
            /// for parsing a <see cref="ColumnSchema"/>.
            /// from <paramref name="columnSchemaXmlNode"/>.
            /// </summary>
            /// <param name="columnSchemaXmlNode"></param>
            /// <param name="requiredAttributeNames"></param>
            /// <exception cref="MissingAttributeXmlNodeParseException">
            /// thrown if <paramref name="requiredAttributeNames"/> does not have a required attributes
            /// for parsing a <see cref="ColumnSchema"/>
            /// </exception>
            private static void assertContainsRequiredAttributes(
                XmlNode columnSchemaXmlNode, params string[] requiredAttributeNames)
            {
                foreach(string requiredAttributeName in requiredAttributeNames)
                {
                    if(!columnSchemaXmlNode.ContainsAttribute(requiredAttributeName))
                    {
                        throw new XmlNodeMissingAttributeException(requiredAttributeName);
                    }
                }
            }
        }
    }
}