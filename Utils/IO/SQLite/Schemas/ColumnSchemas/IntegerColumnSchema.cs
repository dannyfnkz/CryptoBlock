using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represents a <see cref="ColumnSchema"/> with Integer <see cref="ColumnSchema.eDataType"/>.
        /// </summary>
        public class IntegerColumnSchema : ColumnSchema
        {
            public IntegerColumnSchema(
                string name,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                long? defaultValue = null)
                : base(name, eDataType.Integer, notNull, autoIncrement, unique, defaultValue)
            {

            }
        }
    }
}