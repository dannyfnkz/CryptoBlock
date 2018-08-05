using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries
    {
        public class SelectQuery
        {
            public class Join
            {
                public enum eJoinType
                {
                    InnerJoin, LeftJoin, RightJoin, FullOuterJoin
                }

                private static readonly Dictionary<eJoinType, string> eTypeToString
                    = new Dictionary<eJoinType, string>()
                {
                    { eJoinType.InnerJoin, "INNER JOIN" },
                    { eJoinType.LeftJoin, "LEFT JOIN" },
                    { eJoinType.RightJoin, "RIGHT JOIN" },
                    { eJoinType.FullOuterJoin, "FULL OUTER JOIN" }
                };

                private readonly eJoinType joinType;
                private readonly string joinedTableName;
                private readonly TableColumn leftAnchorTableColumn;
                private readonly TableColumn rightAnchorTableColumn;
                private readonly string queryString;

                public Join(   
                    string joinedTableName,
                    TableColumn leftAnchorTableColumn,
                    TableColumn rightAnchorTableColumn,
                    eJoinType joinType)
                {       
                    this.joinedTableName = joinedTableName;
                    this.leftAnchorTableColumn = leftAnchorTableColumn;
                    this.rightAnchorTableColumn = rightAnchorTableColumn;
                    this.joinType = joinType;

                    this.queryString = buildQueryString(
                        joinedTableName,
                        leftAnchorTableColumn,
                        rightAnchorTableColumn,
                        joinType);
                }

                public string JoinedTableName
                {
                    get { return joinedTableName; }
                }

                public Column LeftAnchorColumn
                {
                    get { return leftAnchorTableColumn; }
                }

                public Column RightAnchorColumn
                {
                    get { return rightAnchorTableColumn; }
                }

                public eJoinType JoinType
                {
                    get { return joinType; }
                }

                public string QueryString
                {
                    get { return queryString; }
                }

                private static string buildQueryString(
                    string joinedTableName,
                    TableColumn leftAnchorTableColumn,
                    TableColumn rightAnchorTableColumn,
                    eJoinType joinType)
                {
                    StringBuilder queryStringBuilder = new StringBuilder();

                    string joinTypeString = joinTypeToString(joinType);

                    return string.Format(
                        "{0} {1} ON {2} = {3}",
                        joinTypeString,
                        joinedTableName,
                        leftAnchorTableColumn.FullyQualifiedName,
                        rightAnchorTableColumn.FullyQualifiedName);
                }

                private static string joinTypeToString(eJoinType joinType)
                {
                    return eTypeToString[joinType];
                }
            }

            private readonly string sourceTableName;
            private readonly TableColumn[] tableColumns;
            private readonly Join[] joins;
            private readonly Condition queryCondition;

            private readonly string queryStrying;

            public SelectQuery(
                string sourceTableName,
                TableColumn[] tableColumns,
                Join[] joins = null,
                Condition queryCondition = null)
            {
                this.sourceTableName = sourceTableName;
                this.tableColumns = tableColumns;
                this.joins = joins;
                this.queryCondition = queryCondition;

                this.queryStrying = buildQueryString(
                    sourceTableName,
                    tableColumns,
                    joins,
                    queryCondition);
            }

            public string SourceTableName
            {
                get { return sourceTableName; }
            }

            public TableColumn[] TableColumns
            {
                get { return tableColumns; }
            }

            public Join[] Joins
            {
                get { return joins; }
            }

            public Condition QueryCondition
            {
                get { return queryCondition; }
            }

            public string QueryString
            {
                get { return queryStrying; }
            }

            private static string buildQueryString(
                string sourceTableName,
                TableColumn[] tableColumns,
                Join[] joins,
                Condition queryCondition)
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append command prefix
                queryStringBuilder.Append("SELECT ");

                for (int i = 0; i < tableColumns.Length; i++)
                {
                    // append Column fully qualified name
                    TableColumn tableColumn = tableColumns[i];
                    queryStringBuilder.Append(tableColumn.FullyQualifiedName);

                    if(i < tableColumns.Length - 1)
                    {
                        queryStringBuilder.Append(", ");
                    }
                }

                // append source table name
                queryStringBuilder.AppendFormat(" FROM {0}", sourceTableName) ;

                // append joins
                if(joins != null)
                {
                    foreach (Join join in joins)
                    {
                        queryStringBuilder.AppendFormat(" {0}", join.QueryString);
                    }
                }

                // append comparison
                if(queryCondition != null)
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", queryCondition.QueryString);
                }

                return queryStringBuilder.ToString();
            }
        }
    }
}
