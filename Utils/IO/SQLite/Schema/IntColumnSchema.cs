namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class IntegerColumnSchema : ColumnSchema
        {
            public IntegerColumnSchema(
                string name,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                int? defaultValue = null)
                : base(name, eType.Integer, notNull, autoIncrement, unique, defaultValue)
            {

            }
        }
    }
}