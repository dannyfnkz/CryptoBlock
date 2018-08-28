using System.Collections.Generic;
using System;
using System.Text;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;
using CryptoBlock.Utils.Strings;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        public class ColumnSchema : Schema
        {
            public enum eDataType
            {
                Integer, Varchar, Real, Char
            };

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

            public static ColumnSchema IdColumnSchema
            {
                get { return ID_COLUMN_SCHEMA; }
            }

            public string Name
            {
                get { return name; }
            }

            public eDataType DataType
            {
                get { return dataType; }
            }

            public bool NotNull
            {
                get { return notNull; }
            }

            public bool AutoIncrement
            {
                get { return autoIncrement; }
            }

            public bool Unique
            {
                get { return unique; }
            }

            public object DefaultValue
            {
                get { return defaultValue; }
            }

            public long? Length
            {
                get { return length; }
            }

            // throws NullReferenceException
            public static ColumnSchema Parse(XmlNode columnSchemaXmlNode)
            {
                // get required attributes
                string name = columnSchemaXmlNode.Attributes["name"].Value;
                string typeString = columnSchemaXmlNode.Attributes["type"].Value;

                try
                {
                    // parse typeString into valid enum format (first letter upper, all others lower)
                    typeString = typeString.ToLower();
                    typeString = typeString.CharactersAtIndicesToUpper(0);

                    eDataType dataType = (eDataType)Enum.Parse(typeof(eDataType), typeString);

                    // get optional attributes

                    bool notNull = false;
                    bool autoIncrement = false;
                    bool unique = false;
                    object defaultValue = null;
                    long? length = null;
                   
                    if (columnSchemaXmlNode.SelectNodes("not_null").Count > 0)
                    {
                        notNull = true;
                    }
                    if (columnSchemaXmlNode.SelectNodes("auto_increment").Count > 0)
                    {
                        autoIncrement = true;
                    }
                    if (columnSchemaXmlNode.SelectNodes("unique").Count > 0)
                    {
                        unique = true;
                    }
                    if (columnSchemaXmlNode.SelectNodes("default").Count > 0)
                    {
                        defaultValue = long.Parse(
                            columnSchemaXmlNode.SelectNodes("default")[0].FirstChild.Value);
                    }
                    if (columnSchemaXmlNode.SelectNodes("length").Count > 0)
                    {
                        XmlNode m = columnSchemaXmlNode.SelectNodes("length")[0];
                        length = long.Parse(columnSchemaXmlNode.SelectNodes("length")[0].FirstChild.Value);
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
                catch(ArgumentException argumentException) // invalid typeString (eDataType parse failed)
                {
                    string exceptionMessage = string.Format(
                        "(Column '{0}') Invalid column data type: '{1}'.",
                        name,
                        typeString);
                    throw new XmlNodeParseException(exceptionMessage, argumentException);
                }
            }

            public static string DataTypeToString(eDataType dataType)
            {
                return Enum.GetName(typeof(eDataType), dataType).ToUpper();
            }

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

            protected override string BuildQueryString()
            {
                string typeString = DataTypeToString(this.dataType);

                StringBuilder queryStringBuilder = new StringBuilder();

                queryStringBuilder.AppendFormat("{0} {1}", this.Name, typeString);

                if (this.length != null)
                {
                    queryStringBuilder.AppendFormat("({0})", this.length.GetValueOrDefault());
                }
                if (this.autoIncrement)
                {
                    queryStringBuilder.AppendFormat(" {0}", "AUTO_INCREMENT");
                }
                if (this.notNull)
                {
                    queryStringBuilder.AppendFormat(" {0}", "NOT NULL");
                }
                if (this.defaultValue != null)
                {
                    queryStringBuilder.AppendFormat(" DEFAULT '{0}'", this.defaultValue.ToString());
                }
                if (this.unique)
                {
                    queryStringBuilder.AppendFormat(" {0}", "UNIQUE");
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}