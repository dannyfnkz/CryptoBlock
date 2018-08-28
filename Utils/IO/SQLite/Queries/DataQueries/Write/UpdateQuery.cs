﻿using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
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
        public class UpdateQuery : DataWriteQuery
        {
            private readonly string tableName;
            private readonly ValuedColumn[] valuedColumns;
            private readonly Condition queryCondition;

            public UpdateQuery(
                string tableName,
                IList<ValuedColumn> valuedColumns,
                Condition queryCondition = null)
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

            public Condition QueryCondition
            {
                get { return queryCondition; }
            }


            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append command header
                queryStringBuilder.AppendFormat("UPDATE {0} SET ", this.tableName);

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
                    queryStringBuilder.AppendFormat(" WHERE {0}", queryCondition.QueryString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}
