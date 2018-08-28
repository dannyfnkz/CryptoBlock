namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
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
