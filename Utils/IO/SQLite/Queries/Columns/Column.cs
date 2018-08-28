using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Columns
    {
        public class Column : IExpression
        {
            private readonly string name;

            public Column(string name)
            {
                this.name = name;
            }

            string IExpression.ExpressionString
            {
                get
                {
                    return FullyQualifiedName;
                }
            }

            public string Name
            {
                get { return name; }
            }

            public virtual string FullyQualifiedName
            {
                get { return Name; }
            }

        }
    }
}