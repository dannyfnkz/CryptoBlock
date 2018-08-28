using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.DataQueries.Read
    {
        public class SelectQuery : DataReadQuery
        {
            public class Join
            {
                public enum eJoinType
                {
                    InnerJoin, LeftJoin, RightJoin, FullOuterJoin
                }

                private static readonly Dictionary<eJoinType, string> typeToString
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
                    return typeToString[joinType];
                }
            }

            public class OrderBy
            {
                public class TableColumn
                {
                    public enum eType
                    {
                        Ascending, Descending
                    }

                    private Dictionary<eType, string> typeToString = new Dictionary<eType, string>()
                    {
                        {eType.Ascending, "ASC" },
                        {eType.Descending, "DESC" }
                    };

                    private readonly string name;
                    private readonly string tableName;
                    private readonly eType type;

                    public TableColumn(string name, string tableName, eType type)
                    {
                        this.name = name;
                        this.tableName = tableName;
                        this.type = type;
                    }

                    public string Name
                    {
                        get { return name; }
                    }

                    public string TableName
                    {
                        get { return tableName; }
                    }

                    public eType Type
                    {
                        get { return type; }
                    }

                    public string QueryString
                    {
                        get
                        {
                            return string.Format(
                            "{0}.{1} {2}",
                            TableName,
                            Name,
                            typeToString[Type]);
                        }
                    }
                }

                public class EmptyTableColumnListException : Exception
                {
                    public EmptyTableColumnListException()
                        : base(formatExceptionMessage())
                    {

                    }

                    private static string formatExceptionMessage()
                    {
                        return "Table column list must contain at least one item.";
                    }
                }

                private readonly TableColumn[] tableColumnArray;

                private string queryString;

                public OrderBy(IList<TableColumn> tableColumnList)
                {
                    this.tableColumnArray = tableColumnList.ToArray();
                }

                public string QueryString
                {
                    get
                    {
                        if (queryString == null)
                        {
                            queryString = buildQueryString();
                        }

                        return queryString;
                    }
                }

                private string buildQueryString()
                {
                    StringBuilder queryStringBuilder = new StringBuilder();

                    // append header
                    queryStringBuilder.Append("ORDER BY ");

                    for (int i = 0; i < tableColumnArray.Length; i++)
                    {
                        TableColumn tableColumn = tableColumnArray[i];
                        queryStringBuilder.Append(tableColumn.QueryString);

                        if (i < tableColumnArray.Length - 1)
                        {
                            queryStringBuilder.Append(", ");
                        }
                    }

                    return queryStringBuilder.ToString();
                }
            }

            private const char SELECT_ALL_COLUMNS_WILDCARD = '*';

            private readonly string sourceTableName;
            private readonly TableColumn[] tableColumns;
            private readonly Join[] joins;
            private readonly Condition queryCondition;
            private readonly OrderBy orderByClause;

            private readonly string queryStrying;

            public SelectQuery(
                string sourceTableName = null,
                TableColumn[] tableColumns = null,
                Join[] joins = null,
                Condition queryCondition = null,
                OrderBy orderByClause = null
                )
            {
                this.sourceTableName = sourceTableName;
                this.tableColumns = tableColumns;
                this.joins = joins;
                this.queryCondition = queryCondition;
                this.orderByClause = orderByClause;

                this.queryStrying = BuildQueryString();
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

            public OrderBy OrderByClause
            {
                get { return orderByClause; }
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append command prefix
                queryStringBuilder.Append("SELECT ");

                // append TableColumns

                if(this.TableColumns == null) // if null select all columns in table
                {
                    queryStringBuilder.Append(SELECT_ALL_COLUMNS_WILDCARD);
                }
                else // tableColumns != null
                {
                    // append fully qualified names of specified columns
                    for (int i = 0; i < this.TableColumns.Length; i++)
                    {
                        // append Column fully qualified name
                        TableColumn tableColumn = this.TableColumns[i];
                        queryStringBuilder.Append(tableColumn.FullyQualifiedName);

                        if (i < this.tableColumns.Length - 1)
                        {
                            queryStringBuilder.Append(", ");
                        }
                    }
                }

                // append source table name, if exists
                if(this.SourceTableName != null)
                {
                    queryStringBuilder.AppendFormat(" FROM {0}", this.SourceTableName);
                }
                
                // append joins
                if(this.Joins != null)
                {
                    foreach (Join join in joins)
                    {
                        queryStringBuilder.AppendFormat(" {0}", join.QueryString);
                    }
                }

                // append query condition, if exists
                if(this.QueryCondition != null)
                {
                    queryStringBuilder.AppendFormat(" WHERE {0}", this.QueryCondition.QueryString);
                }

                // append OrderBy, if exists
                if(this.OrderByClause != null)
                {
                    queryStringBuilder.AppendFormat(" {0}", this.OrderByClause.QueryString);
                }                

                return queryStringBuilder.ToString();
            }
        }
    }
}
