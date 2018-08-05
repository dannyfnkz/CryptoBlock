namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schema
    {
        public class VarcharColumnSchema : ColumnSchema
        {
            public VarcharColumnSchema(
                string name,
                int length,
                bool notNull = false,
                bool autoIncrement = false,
                bool unique = false,
                string defaultValue = null)
                : base(name, eType.Varchar, notNull, autoIncrement, unique, defaultValue, length)
            {

            }
        }
    }
}