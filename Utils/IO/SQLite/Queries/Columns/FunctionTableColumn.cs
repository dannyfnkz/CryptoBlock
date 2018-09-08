using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        /// <summary>
        /// represents a <see cref="TableColumn"/> having an SQL function applied to it.
        /// </summary>
        public class FunctionTableColumn : TableColumn
        {
            /// <summary>
            /// type of SQL function.
            /// </summary>
            public enum eFunctionType
            {
                Count, LastInsertRowid
            }

            private readonly eFunctionType functionType;

            private readonly string fullyQualifiedName;

            public FunctionTableColumn(
                eFunctionType functionType,
                string name = null, 
                string tableName = null)
                : base(name, tableName)
            {
                this.functionType = functionType;

                this.fullyQualifiedName = buildFullyQualifiedName();
            }

            public eFunctionType FunctionType
            {
                get { return functionType; }
            }

            public override string FullyQualifiedName
            {
                get { return fullyQualifiedName; }
            }

            private string buildFullyQualifiedName()
            {
                string fullyQualifiedName = string.Format(
                    "{0}({1})",
                    FunctionTypeToString(FunctionType),
                    base.FullyQualifiedName);

                return fullyQualifiedName;
            }

            /// <summary>
            /// returns the string representation of <paramref name="functionType"/>.
            /// </summary>
            /// <remarks>
            /// conversion is as follows:
            /// functionType => functionType.ToString().ToSnakeCase().ToUpper()
            /// </remarks>
            /// <param name="functionType"></param>
            /// <returns>
            /// string representation of <paramref name="functionType"/>
            /// </returns>
            public static string FunctionTypeToString(eFunctionType functionType)
            {
                string functiontypeString = functionType.ToString();

                return StringUtils.PascalCaseToSnakeCase(functiontypeString).ToUpper();
            }
        }
    }
}
