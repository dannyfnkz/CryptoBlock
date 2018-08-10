namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class RealColumnSchema : ColumnSchema
        {
            public RealColumnSchema(
                string name,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                int? defaultValue = null)
                : base(name, eDataType.Real, notNull, autoIncrement, unique, defaultValue)
            {

            }
        }
    }
}
