using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represents a <see cref="ColumnSchema"/> with Char <see cref="ColumnSchema.eDataType"/>.
        /// </summary>
        public class CharColumnSchema : ColumnSchema
        {
            public CharColumnSchema(
                string name,
                long length,
                bool notNull = false,
                bool unique = false,
                string defaultValue = null)
                : base(name, eDataType.Char, notNull, false, unique, defaultValue, length)
            {

            }
        }
    }
}