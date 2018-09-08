using CryptoBlock.Utils.Collections;
using CryptoBlock.Utils.Collections.List;
using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using CryptoBlock.Utils.IO.SQLite.Queries.Conditions;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Read;
using CryptoBlock.Utils.IO.SQLite.Queries.DataQueries.Write;
using CryptoBlock.Utils.IO.SQLite.Schema.Triggers;
using CryptoBlock.Utils.IO.SQLite.Schemas;
using CryptoBlock.Utils.IO.SQLite.Schemas.ColumnSchemas;
using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using CryptoBlock.Utils.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.IO.SQLite;
using static CryptoBlock.Utils.IO.SQLite.DatabaseStructure;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        internal static class AuditUtils
        {
            /// <summary>
            /// thrown if audit is not implemented for <see cref="Query"/>s
            /// of specified type.
            /// </summary>
            internal class UnsupportedAuditQueryTypeException : Exception
            {
                private readonly Query.eQueryType queryType;

                internal UnsupportedAuditQueryTypeException(Query.eQueryType queryType)
                    : base(formatExceptionMessage(queryType))
                {
                    this.queryType = queryType;
                }

                internal Query.eQueryType QueryType
                {
                    get { return queryType; }
                }

                private static string formatExceptionMessage(Query.eQueryType queryType)
                {
                    string exceptionMessage = string.Format(
                        "Audit is not supported for queries of type '{0}'.",
                        Query.QueryTypeToString(queryType));

                    return exceptionMessage;
                }
            }

            /// <summary>
            /// <see cref="TableSchema"/> data for a generic audit table.
            /// </summary>
            private static class AuditTableStructure
            {
                internal static string AUDIT_TABLE_NAME_PREFIX = "Audit";
                internal static readonly string AUDIT_TABLE_NAME_LIKE_EXPRESSION =
                    string.Format("{0}%", AUDIT_TABLE_NAME_PREFIX);

                internal static int QUERY_ID_COLUMN_INDEX = 1;
                internal static int AUDITED_TABLE_ROW_ID_COLUMN_INDEX = 2;
                internal static int AUDITED_TABLE_DATA_COLUMNS_START_INDEX = 3;
            }

            // SelectQuery for selecting names of all audit tables in database
            private static SelectQuery auditTableNamesSelectQuery;

            /// <summary>
            /// <see cref="SelectQuery"/> for selecting names of all audit tables in database.
            /// </summary>
            /// <seealso cref="buildAuditTableNamesSelectQuery"/>
            internal static SelectQuery AuditTableNamesSelectQuery
            {
                get
                {
                    if (auditTableNamesSelectQuery == null)
                    {
                        auditTableNamesSelectQuery = buildAuditTableNamesSelectQuery();
                    }

                    return auditTableNamesSelectQuery;
                }
            }

            /// <summary>
            /// returns undo <see cref="Query"/> array corresponding to queries performed on 
            /// <paramref name="auditedTableName"/>, where the i'th undo <see cref="Query"/> in result
            /// corresponds the i'th row in <paramref name="auditTableResultSet"/>.
            /// </summary>
            /// <seealso cref="getUndoQuery(ResultSet.Row, string, Dictionary{long, Query.eQueryType})"/>
            /// <param name="auditTableResultSet"></param>
            /// <param name="auditedTableName"></param>
            /// <param name="queryTypeIdToQueryType"></param>
            /// <returns>
            /// undo <see cref="Query"/> array corresponding to queries performed on 
            /// <paramref name="auditedTableName"/>, where the i'th undo <see cref="Query"/> in result
            /// corresponds the i'th row in <paramref name="auditTableResultSet"/>
            /// </returns>
            /// <exception cref="UnsupportedAuditQueryTypeException">
            /// <seealso cref="getUndoQuery(ResultSet.Row, string, Dictionary{long, Query.eQueryType})"/>
            /// </exception>
            internal static WriteQuery[] GetAuditTableUndoWriteQueries(
                ResultSet auditTableResultSet,
                string auditedTableName,
                Dictionary<long, Query.eQueryType> queryTypeIdToQueryType)
            {
                WriteQuery[] undoQueries = new WriteQuery[auditTableResultSet.RowCount];

                for(int i = 0; i < auditTableResultSet.RowCount; i++)
                {
                    ResultSet.Row auditTableRow = auditTableResultSet.Rows[i];
                    WriteQuery undoQuery = getUndoQuery(
                        auditTableRow,
                        auditedTableName,
                        queryTypeIdToQueryType);

                    undoQueries[i] = undoQuery;
                }

                return undoQueries;
            }

            /// <summary>
            /// returns undo <see cref="Query"/> corresponding to query performed on 
            /// <paramref name="auditedTableName"/>, where <paramref name="auditTableRow"/> contains
            /// the data required to undo a single row in <paramref name="auditedTableName"/>,
            /// which was affected by the query.
            /// </summary>
            /// <param name="auditTableRow"></param>
            /// <param name="auditedTableName"></param>
            /// <param name="queryTypeIdToQueryType"></param>
            /// <returns>
            /// undo <see cref="Query"/> corresponding to query performed on 
            /// <paramref name="auditedTableName"/>, where <paramref name="auditTableRow"/> contains
            /// the data required to undo a single row in <paramref name="auditedTableName"/>,
            /// which was affected by the query
            /// </returns>
            /// <exception cref="UnsupportedAuditQueryTypeException">
            /// thrown if <see cref="Query.eQueryType"/> specified in <paramref name="auditTableRow"/>
            /// is not supported for audit
            /// </exception>
            private static WriteQuery getUndoQuery(
                ResultSet.Row auditTableRow,
                string auditedTableName,
                Dictionary<long, Query.eQueryType> queryTypeIdToQueryType)
            {
                WriteQuery undoQuery;

                // type of query that was originally performed on audited table
                long queryTypeId = auditTableRow.GetColumnValue<long>(
                   AuditTableStructure.QUERY_ID_COLUMN_INDEX);
                Query.eQueryType queryType = queryTypeIdToQueryType[queryTypeId];

                // value of _id column of modified row in audit table
                long auditedTableRowId = auditTableRow.GetColumnValue<long>(
                    AuditTableStructure.AUDITED_TABLE_ROW_ID_COLUMN_INDEX);

                // get undo query based on query type
                if(queryType == Query.eQueryType.Insert)
                {
                    undoQuery = new DeleteQuery(
                        auditedTableName,
                        new BasicCondition(
                            new ValuedTableColumn(
                                DatabaseStructure.ID_COLUMN_NAME,
                                auditedTableName,
                                auditedTableRowId),
                            BasicCondition.eOperatorType.Equal
                        )
                    );
                }
                else if (queryType == Query.eQueryType.Update || queryType == Query.eQueryType.Delete)
                {
                    // ValuedColumn of _id column of modified row in audited table
                    ArrayRange<ValuedColumn> auditedTableRowIdArrayRange =
                      new ArrayRange<ValuedColumn>(
                          new ValuedColumn(DatabaseStructure.ID_COLUMN_NAME, auditedTableRowId)
                      );

                    // ValueColumn of columns other than _id of modified row in audited table 
                    ArrayRange<ValuedColumn> auditedTableRowValuesArrayRange =
                        new ArrayRange<ValuedColumn>(
                            ValuedColumn.Parse(auditTableRow.ColumnNames, auditTableRow.ColumnValues),
                            AuditTableStructure.AUDITED_TABLE_DATA_COLUMNS_START_INDEX);

                    // unified ValuedColumn list for undo query
                    List<ValuedColumn> queryValuedColumns
                        = ListUtils.ListFromArrayRanges(
                            auditedTableRowIdArrayRange,
                            auditedTableRowValuesArrayRange);

                    if(queryType == Query.eQueryType.Update)
                    {
                        undoQuery = new UpdateQuery(
                            auditedTableName,
                            queryValuedColumns,
                            new BasicCondition(
                                new ValuedTableColumn(
                                    DatabaseStructure.ID_COLUMN_NAME,
                                    auditedTableName,
                                    auditedTableRowId),
                                BasicCondition.eOperatorType.Equal
                            )
                        );
                    }
                    else // queryType == Query.eQueryType.Delete
                    {
                        undoQuery = new InsertQuery(auditedTableName, queryValuedColumns);
                    }
                }
                else
                {
                    throw new UnsupportedAuditQueryTypeException(queryType);
                }

                return undoQuery;
            }

            /// <summary>
            /// returns the audit <see cref="TableSchema"/> corresponding to 
            /// <paramref name="tableSchema"/>.
            /// </summary>
            /// <param name="tableSchema"></param>
            /// <returns>
            /// audit <see cref="TableSchema"/> corresponding to 
            /// <paramref name="tableSchema"/>
            /// </returns>
            internal static TableSchema GetAuditTableSchema(TableSchema tableSchema)
            {
                TableSchema auditTableSchema;

                string auditTableName = GetAuditTableName(tableSchema.Name);

                // init audit table ColumnSchema list with _id, queryType, and _id column of original table
                // named (this.tableName + "Id")
                List<ColumnSchema> auditColumnSchemas = new List<ColumnSchema>()
                {
                    ColumnSchema.IdColumnSchema,
                    new IntegerColumnSchema(
                        StringUtils.PascalCaseToCamelCase(
                            QueryTypeTableStructure.TABLE_NAME) + "Id",
                        true),
                    new IntegerColumnSchema(
                        StringUtils.PascalCaseToCamelCase(tableSchema.Name) + "Id",
                        true)
                };

                // add all columns from this table (except for _id which was already accounted for)
                // with their constraints stripped

                // get columns from this table
                ColumnSchema[] strippedColumnSchemas = tableSchema.ColumnSchemas.Subarray<ColumnSchema>(
                    1, tableSchema.ColumnSchemas.Length);

                // strip constraints
                strippedColumnSchemas.ConvertEachElement<ColumnSchema, ColumnSchema>(
                    columnSchemas => ColumnSchema.GetColumnSchemaWithConstraintsStripped(columnSchemas));

                // add stripped columns to audit column schema list
                auditColumnSchemas.AddRange(strippedColumnSchemas);

                // an audit table is non-auditable
                const bool auditable = false;

                // init audit table schema
                auditTableSchema = new TableSchema(
                    auditTableName,
                    auditColumnSchemas.ToArray(),
                    ColumnSchema.IdColumnSchema,
                    auditable);

                return auditTableSchema;
            }

            /// <summary>
            /// returns array of audited table names, where the i'th element corresponds to the
            /// i'th audit table name in <paramref name="auditTableNames"/>.
            /// </summary>
            /// <param name="auditTableNames"></param>
            /// <returns>
            /// array of audited table names, where the i'th element corresponds to the
            /// i'th audit table name in <paramref name="auditTableNames"/>
            /// </returns>
            internal static string[] GetAuditedTableNames(IList<string> auditTableNames)
            {
                string[] auditedTableNames = new string[auditTableNames.Count];

                // convert each audit table name into its corresponding audited table name
                for(int i = 0; i < auditTableNames.Count; i++)
                {
                    string auditTableName = auditTableNames[i];
                    auditedTableNames[i] = GetAuditedTableName(auditTableName);
                }

                return auditedTableNames;
            }

            /// <summary>
            /// returns the name of the audited table corresponding to the audit table having
            /// <paramref name="auditTableName"/>.
            /// </summary>
            /// <param name="auditTableName"></param>
            /// <returns>
            /// name of the audited table corresponding to the audit table having
            /// <paramref name="auditTableName"/>
            /// </returns>
            internal static string GetAuditedTableName(string auditTableName)
            {
                // cut Audit prefix from audit table name
                return auditTableName.GetSubstringAfterPrefix(
                    AuditTableStructure.AUDIT_TABLE_NAME_PREFIX);
            }

            /// <summary>
            /// returns the name of the audit table corresponding to table having
            /// <paramref name="tableName"/>.
            /// </summary>
            /// <param name="tableName"></param>
            /// <returns>
            /// name of the audit table corresponding to table having
            /// <paramref name="tableName"/>.
            /// </returns>
            internal static string GetAuditTableName(string tableName)
            {
                // add the Audit prefix to table name
                return AuditTableStructure.AUDIT_TABLE_NAME_PREFIX + tableName;
            }

            /// <summary>
            /// returns <see cref="TriggerSchema"/> corresponding to a query of <paramref name="queryType"/>
            /// performed on table corresponding to <paramref name="auditedTableSchema"/>.
            /// </summary>
            /// <param name="auditedTableSchema"></param>
            /// <param name="auditTableSchema"></param>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="TriggerSchema"/> corresponding to a query of <paramref name="queryType"/>
            /// performed on table corresponding to <paramref name="auditedTableSchema"/>
            /// </returns>
            internal static TriggerSchema GetAuditTriggerSchema(
                TableSchema auditedTableSchema,
                TableSchema auditTableSchema,
                Query.eQueryType queryType)
            {
                TriggerSchema auditTriggerSchema;

                InsertQuery onTriggerAuditInsertQuery =
                    getOnTriggerAuditInsertQuery(
                    auditedTableSchema,
                    auditTableSchema,
                    queryType);

                auditTriggerSchema = new TriggerSchema(
                    getTriggerName(auditedTableSchema, queryType),
                    getTriggerTime(queryType),
                    queryType,
                    auditedTableSchema.Name,
                    onTriggerAuditInsertQuery);

                return auditTriggerSchema;
            }

            /// <summary>
            /// returns <see cref="TriggerSchema.eTriggeredQueryTime"/> corresponding to 
            /// specified <paramref name="queryType"/>.
            /// </summary>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="TriggerSchema.eTriggeredQueryTime"/> corresponding to 
            /// specified <paramref name="queryType"/>
            /// </returns>
            private static TriggerSchema.eTriggeredQueryTime getTriggerTime(Query.eQueryType queryType)
            {
                TriggerSchema.eTriggeredQueryTime triggerTime = 0; // init to temp value

                if(queryType == Query.eQueryType.Insert)
                {
                    triggerTime = TriggerSchema.eTriggeredQueryTime.After;
                }
                else if(queryType == Query.eQueryType.Update || queryType == Query.eQueryType.Delete)
                {
                    triggerTime = TriggerSchema.eTriggeredQueryTime.Before;
                }

                return triggerTime;
            }

            /// <summary>
            /// returns the <see cref="TriggerSchema"/> name corresponding to a query of
            /// <paramref name="queryType"/> performed on <paramref name="auditedTableSchema"/>.
            /// </summary>
            /// <param name="auditedTableSchema"></param>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="TriggerSchema"/> name corresponding to a query of
            /// <paramref name="queryType"/> performed on <paramref name="auditedTableSchema"/>
            /// </returns>
            private static string getTriggerName(
                TableSchema auditedTableSchema,
                Query.eQueryType queryType)
            {
                string queryTypeString =
                    Query.QueryTypeToString(queryType).MakeOnlyCharactersAtIndicesUpper(0);

                String triggerName = string.Format(
                    "on{0}{1}",
                    auditedTableSchema.Name,
                    queryTypeString);

                return triggerName;
            }

            /// <summary>
            /// returns the <see cref="InsertQuery"/> performed on the instance of
            /// <paramref name="auditTableSchema"/> corresponding to <paramref name="auditedTableSchema"/>,
            /// when audit is triggered by query of <paramref name="queryType"/>, performed on
            /// <paramref name="auditedTableSchema"/>.
            /// </summary>
            /// <param name="auditedTableSchema"></param>
            /// <param name="auditTableSchema"></param>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="InsertQuery"/> performed on the instance of
            /// <paramref name="auditTableSchema"/> corresponding to <paramref name="auditedTableSchema"/>,
            /// when audit is triggered by query of <paramref name="queryType"/>, performed on
            /// <paramref name="auditedTableSchema"/>
            /// </returns>
            private static InsertQuery getOnTriggerAuditInsertQuery(
                TableSchema auditedTableSchema,
                TableSchema auditTableSchema,
                Query.eQueryType queryType)
            {
                InsertQuery onTriggerAuditInsertQuery;

                // init trigger InsertQuery ValuedColumn list with:
                // 0. ID corresponding to a query of type queryType,
                //    which was performed on triggeredTableSchema and triggered the audit
                // 1. ID of row which was triggered
                List<ValuedColumn> triggerInsertQueryValuedColumns = new List<ValuedColumn>()
                {
                    new ValuedColumn(
                        auditTableSchema.ColumnSchemas[1].Name,
                        getQueryTypeIdSelectQuery(queryType)),
                        getAuditedTableTriggeredRowIdValuedColumn(
                            auditedTableSchema,
                            auditTableSchema,
                            queryType)
                };

                // add values of rest of ValuedColumns of row which was triggered in
                // auditedTable

                // convert each (non-primary key) ValuedColumn (key, value) of triggered row in
                // auditedTable,
                // into a TriggerValuedColumn (key, [TriggerValuedColumn.eTime].key)
                // in the auditTable InsertQuery
                // (note that column names in auditTable starting from index 3 match column names in 
                // auditedTable starting from index 1)
                TriggerValuedColumn.ValueExpression.eTime triggerValuedColumnTime =
                    queryType == Query.eQueryType.Insert
                    ? TriggerValuedColumn.ValueExpression.eTime.New
                    : TriggerValuedColumn.ValueExpression.eTime.Old;

                TriggerValuedColumn[] auditTableTriggerValueColumns =
                    auditedTableSchema.ColumnSchemas.Subarray(1).Select(
                        columnSchema => new TriggerValuedColumn(
                            columnSchema.Name,
                            columnSchema.Name,
                            triggerValuedColumnTime)
                    ).ToArray();

                // add ValuedColumns to ValuedColumn list
                triggerInsertQueryValuedColumns.AddRange(auditTableTriggerValueColumns);

                // init InsertQuery
                onTriggerAuditInsertQuery = new InsertQuery(
                    auditTableSchema.Name,
                    triggerInsertQueryValuedColumns.ToArray()
                );

                return onTriggerAuditInsertQuery;
            }

            /// <summary>
            /// returns <see cref="ValuedColumn"/>, based on <paramref name="queryType"/>,
            /// corresponding to the id of the triggered row in <paramref name="auditedTableSchema"/>.
            /// </summary>
            /// <remarks>
            /// <see cref="ValuedColumn"/> value might reference the old / new row id, 
            /// before / after the triggering query took place (i.e (old/new).columnName), 
            /// or the last inserted row id in audited table.
            /// </remarks>
            /// <param name="auditedTableSchema"></param>
            /// <param name="auditTableSchema"></param>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="ValuedColumn"/>, based on <paramref name="queryType"/>,
            /// corresponding to the id of the triggered row in <paramref name="auditedTableSchema"/>.
            /// </returns>
            private static ValuedColumn getAuditedTableTriggeredRowIdValuedColumn(
                TableSchema auditedTableSchema,
                TableSchema auditTableSchema,
                Query.eQueryType queryType)
            {
                ValuedColumn auditedTableTriggeredRowIdValuedColumn = null;

                // name of column (in audit table) containing audited table triggered row id
                string columnName = auditTableSchema.ColumnSchemas
                    [AuditTableStructure.AUDITED_TABLE_ROW_ID_COLUMN_INDEX].Name;

                if (queryType == Query.eQueryType.Insert) // get the ID the row got after being inserted
                {
                    object columnValue = new FunctionTableColumn(
                        FunctionTableColumn.eFunctionType.LastInsertRowid);
                    auditedTableTriggeredRowIdValuedColumn = new ValuedColumn(columnName, columnValue);
                }
                else if(queryType == Query.eQueryType.Update || queryType == Query.eQueryType.Delete)
                {
                    // get the id the row had before being updated / deleted
                    auditedTableTriggeredRowIdValuedColumn = new TriggerValuedColumn(
                        columnName,
                        auditedTableSchema.PrimaryKeyColumnSchema.Name,
                        TriggerValuedColumn.ValueExpression.eTime.Old);
                }

                return auditedTableTriggeredRowIdValuedColumn;
            }

            /// <summary>
            /// returns <see cref="SelectQuery"/> for selecting the id of the specified
            /// <paramref name="queryType"/>.
            /// </summary>
            /// <param name="queryType"></param>
            /// <returns>
            /// <see cref="SelectQuery"/> for selecting the id of the specified
            /// <paramref name="queryType"/>
            /// </returns>
            private static SelectQuery getQueryTypeIdSelectQuery(Query.eQueryType queryType)
            {
                // select queryTypeId corresponding to queryType
                SelectQuery queryTypeIdSelectQuery = new SelectQuery(
                    QueryTypeTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                        new TableColumn(
                            DatabaseStructure.ID_COLUMN_NAME,
                            QueryTypeTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            QueryTypeTableStructure.NAME_COLUMN_NAME,
                            QueryTypeTableStructure.TABLE_NAME,
                            Query.QueryTypeToString(queryType)
                        ),
                        BasicCondition.eOperatorType.Equal
                    )
                );

                return queryTypeIdSelectQuery;
            }

            /// <summary>
            /// returns <see cref="SelectQuery"/> for selecting names of all audit tables in database.
            /// </summary>
            /// <returns>
            /// <see cref="SelectQuery"/> for selecting names of all audit tables in database
            /// </returns>
            private static SelectQuery buildAuditTableNamesSelectQuery()
            {
                SelectQuery auditTableNamesSelectQuery = new SelectQuery(
                    MasterTableStructure.TABLE_NAME,
                    new TableColumn[]
                    {
                            new TableColumn(
                                MasterTableStructure.NAME_COLUMN_NAME,
                                MasterTableStructure.TABLE_NAME)
                    },
                    null,
                    new BasicCondition(
                        new ValuedTableColumn(
                            MasterTableStructure.NAME_COLUMN_NAME,
                            MasterTableStructure.TABLE_NAME,
                            AuditTableStructure.AUDIT_TABLE_NAME_LIKE_EXPRESSION),
                        BasicCondition.eOperatorType.Like
                    )
                );

                return auditTableNamesSelectQuery;
            }
        }
    }
}

