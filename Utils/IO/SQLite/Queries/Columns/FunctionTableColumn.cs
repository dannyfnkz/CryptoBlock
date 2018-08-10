using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class FunctionTableColumn : TableColumn
        {
            // name of enum instance determines string representation in query as follows:
            // enumInstanceString => enumInstanceString.ToSnakeCase().ToUpper()
            public enum eFunctionType
            {
                Count, LastInsertRowid
            }

            private readonly eFunctionType functionType;

            public FunctionTableColumn(
                eFunctionType functionType,
                string name = null, 
                string tableName = null)
                : base(name, tableName)
            {
                this.functionType = functionType;
            }

            public eFunctionType FunctionType
            {
                get { return functionType; }
            }

            public override string QueryString
            {
                get
                {
                    return string.Format(
                    "{0}({1})",
                    FunctionTypeToString(FunctionType),
                    FullyQualifiedName);
                }
            }

            public string FunctionTypeToString(eFunctionType functionType)
            {
                string functiontypeString = functionType.ToString();

                return StringUtils.PascalCaseToSnakeCase(functiontypeString).ToUpper();
            }
        }
    }
}
