﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.SchemaQueries.Write
    {
        public class DropTableQuery : SchemaWriteQuery
        {
            private readonly string tableName;
            private readonly bool existsConstraint;

            public DropTableQuery(string tableName, bool existsConstraint = false)
            {
                this.tableName = tableName;
                this.existsConstraint = existsConstraint;
            }

            public string TableName
            {
                get { return tableName; }
            }

            public bool ExistsConstraint
            {
                get { return existsConstraint; }
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                queryStringBuilder.Append("DROP TABLE");

                if (this.ExistsConstraint)
                {
                    queryStringBuilder.Append(" IF EXISTS");
                }

                queryStringBuilder.AppendFormat(" {0}", this.TableName);

                return queryStringBuilder.ToString();
            }
        }
    }
}