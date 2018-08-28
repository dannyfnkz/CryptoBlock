namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
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