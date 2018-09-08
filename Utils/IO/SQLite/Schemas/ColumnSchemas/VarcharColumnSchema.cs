using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
        /// <summary>
        /// represents a <see cref="ColumnSchema"/> with Varchar <see cref="ColumnSchema.eDataType"/>.
        /// </summary>
        public class VarcharColumnSchema : ColumnSchema
        {
            public VarcharColumnSchema(
                string name,
                long length,
                bool notNull = false,
                bool unique = false,
                string defaultValue = null)
                : base(name, eDataType.Varchar, notNull, false, unique, defaultValue, length)
            {

            }
        }
    }
}