using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.DataQueries.Write
    {
        /// <summary>
        ///  represents a <see cref="DataWriteQuery"/> which performs an UPDATE operation.
        /// </summary>
        public class UpdateQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly ValuedColumn[] valuedColumns;
            private readonly ICondition queryCondition;

            public UpdateQuery(
                string tableName,
                IList<ValuedColumn> valuedColumns,
                ICondition queryCondition = null)
            {
                this.tableName = tableName;
                this.valuedColumns = valuedColumns.ToArray();
                this.queryCondition = queryCondition;
            }

            public string TableName
            {
                get { return tableName; }
            }

            public ValuedColumn[] ValuedColumns
            {
                get { return valuedColumns; }
            }

            public ICondition QueryCondition
            {
                get { return queryCondition; }
            }


            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append command header
                queryStringBuilder.AppendFormat("UPDATE {0} SET ", this.TableName);

                // append ValuedColumns names and new values
                for(int i = 0; i < valuedColumns.Length; i++)
                {
                    ValuedColumn valuedColumn = valuedColumns[i];
                    queryStringBuilder.AppendFormat("{0} = '{1}'", valuedColumn.Name, valuedColumn.Value);

                    // append seperator
                    if(i < valuedColumns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                // append condition
                if(queryCondition != null)
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", this.QueryCondition.ExpressionString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}

