namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas
    {
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