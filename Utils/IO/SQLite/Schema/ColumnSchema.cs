using System.Collections.Generic;
using System;
using System.Text;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class ColumnSchema
        {
            public enum eType
            {
                Integer, Varchar
            };

            private string name;
            private eType type;
            private int? length;
            private bool notNull;
            private bool unique;
            private bool autoIncrement;
            private object defaultValue;

            internal ColumnSchema(
                string name,
                eType type,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                object defaultValue = null,
                int? length = null)
            {
                this.name = name;
                this.type = type;
                this.notNull = notNull;
                this.autoIncrement = autoIncrement;
                this.unique = unique;
                this.defaultValue = defaultValue;
                this.length = length;
            }

            public string Name
            {
                get { return name; }
            }

            public eType Type
            {
                get { return type; }
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

            public int? Length
            {
                get { return length; }
            }

            static internal string GetTypeString(eType type)
            {
                return Enum.GetName(typeof(eType), type);
            }

            public string GetQueryString()
            {
                string typeString = GetTypeString(type);

                StringBuilder representationStringBuilder = new StringBuilder();

                representationStringBuilder.AppendFormat("{0} {1}", name, typeString);

                if(length != null)
                {
                    representationStringBuilder.AppendFormat("({0})", length.GetValueOrDefault());
                }
                if(autoIncrement)
                {
                    representationStringBuilder.AppendFormat(" {0}", "AUTO_INCREMENT");
                }
                if(notNull)
                {
                    representationStringBuilder.AppendFormat(" {0}", "NOT NULL");
                }
                if(defaultValue != null)
                {
                    representationStringBuilder.AppendFormat(" DEFAULT '{0}'", defaultValue.ToString());
                }
                if(unique)
                {
                    representationStringBuilder.AppendFormat(" {0}", "UNIQUE");
                }

                return representationStringBuilder.ToString();
            }
        }
    }
}