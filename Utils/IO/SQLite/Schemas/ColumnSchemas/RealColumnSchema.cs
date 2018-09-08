using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represents a <see cref="ColumnSchema"/> with Real <see cref="ColumnSchema.eDataType"/>.
        /// </summary>
        public class RealColumnSchema : ColumnSchema
        {
            public RealColumnSchema(
                string name,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                long? defaultValue = null)
                : base(name, eDataType.Real, notNull, autoIncrement, unique, defaultValue)
            {

            }
        }
    }
}
