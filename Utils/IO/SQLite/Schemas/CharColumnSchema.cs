namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
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