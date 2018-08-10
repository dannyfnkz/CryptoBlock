using System.Collections.Generic;
using System;
using System.Text;
using System.Xml;
using static CryptoBlock.Utils.IO.SQLite.Xml.XMLParser;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class ColumnSchema : ISchema
        {
            public enum eDataType
            {
                Integer, Varchar, Real
            };

            private readonly string name;
            private readonly eDataType dataType;
            private readonly long? length;
            private readonly bool notNull;
            private readonly bool unique;
            private readonly bool autoIncrement;
            private readonly object defaultValue;

            private string queryString;

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

            public string QueryString
            {
                get
                {
                    if(this.queryString == null)
                    {
                        this.queryString = buildQueryString(
                            this.name,
                            this.dataType,
                            this.notNull,
                            this.autoIncrement,
                            this.unique,
                            this.defaultValue,
                            this.length);
                    }

                    return this.queryString;
                }
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
                    typeString = StringUtils.CharactersAtIndicesToUpper(typeString, 0);

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
                        typeString);
                    throw new XmlNodeParseException(exceptionMessage, argumentException);
                }
            }

            public static string GetTypeString(eDataType dataType)
            {
                return Enum.GetName(typeof(eDataType), dataType).ToUpper();
            }

            private static string buildQueryString(string name,
                eDataType dataType,
                bool notNull,
                bool autoIncrement,
                bool unique,
                object defaultValue,
                long? length)
            {
                string typeString = GetTypeString(dataType);

                StringBuilder queryStringBuilder = new StringBuilder();

                queryStringBuilder.AppendFormat("{0} {1}", name, typeString);

                if (length != null)
                {
                    queryStringBuilder.AppendFormat("({0})", length.GetValueOrDefault());
                }
                if (autoIncrement)
                {
                    queryStringBuilder.AppendFormat(" {0}", "AUTO_INCREMENT");
                }
                if (notNull)
                {
                    queryStringBuilder.AppendFormat(" {0}", "NOT NULL");
                }
                if (defaultValue != null)
                {
                    queryStringBuilder.AppendFormat(" DEFAULT '{0}'", defaultValue.ToString());
                }
                if (unique)
                {
                    queryStringBuilder.AppendFormat(" {0}", "UNIQUE");
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}