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
            internal class UnsupportedAuditQueryTypeException : Exception
            {
                public UnsupportedAuditQueryTypeException(Query.eType queryType)
                    : base(formatExceptionMessage(queryType))
                {

                }

                private static string formatExceptionMessage(Query.eType queryType)
                {
                    string exceptionMessage = string.Format(
                        "Audit is not supported for queries of type '{0}'.",
                        Query.QueryTypeToString(queryType));

                    return exceptionMessage;
                }
            }

            private static class AuditTableStructure
            {
                internal static string AUDIT_TABLE_NAME_PREFIX = "Audit";
                internal static readonly string AUDIT_TABLE_NAME_LIKE_EXPRESSION =
                    string.Format("{0}%", AUDIT_TABLE_NAME_PREFIX);

                internal static int QUERY_ID_COLUMN_INDEX = 1;
                internal static int AUDITED_TABLE_ROW_ID_COLUMN_INDEX = 2;
                internal static int AUDITED_TABLE_DATA_COLUMNS_START_INDEX = 3;

                //internal static string GetAuditedTableRowIdColumnName(string auditedTableName)
                //{
                //    string auditedTableNameInCamelCase = StringUtils.PascalCaseToCamelCase(
                //        auditedTableName);

                //    return auditedTableNameInCamelCase + "Id";
                //}
            }

            private static SelectQuery auditTableNamesSelectQuery;

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

            internal static WriteQuery[] GetAuditTableUndoWriteQueries(
                ResultSet auditTableResultSet,
                string auditedTableName,
                Dictionary<long, Query.eType> queryTypeIdToQueryType)
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

            private static WriteQuery getUndoQuery(
                ResultSet.Row auditTableRow,
                string auditedTableName,
                Dictionary<long, Query.eType> queryTypeIdToQueryType)
            {
                WriteQuery undoQuery;

                // type of query that was originally performed on audited table
                long queryTypeId = auditTableRow.GetColumnValue<long>(
                   AuditTableStructure.QUERY_ID_COLUMN_INDEX);
                Query.eType queryType = queryTypeIdToQueryType[queryTypeId];

                // value of _id column of modified row in audit table
                long auditedTableRowId = auditTableRow.GetColumnValue<long>(
                    AuditTableStructure.AUDITED_TABLE_ROW_ID_COLUMN_INDEX);

                // get undo query based on query type
                if(queryType == Query.eType.Insert)
                {
                    undoQuery = new DeleteQuery(
                        auditedTableName,
                        new BasicCondition(
                            new ValuedTableColumn(
                                DatabaseStructure.ID_COLUMN_NAME,
                                auditedTableName,
                                auditedTableRowId),
                            BasicCondition.eComparisonType.Equal
                        )
                    );
                }
                else if (queryType == Query.eType.Update || queryType == Query.eType.Delete)
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

                    if(queryType == Query.eType.Update)
                    {
                        undoQuery = new UpdateQuery(
                            auditedTableName,
                            queryValuedColumns,
                            new BasicCondition(
                                new ValuedTableColumn(
                                    DatabaseStructure.ID_COLUMN_NAME,
                                    auditedTableName,
                                    auditedTableRowId),
                                BasicCondition.eComparisonType.Equal
                            )
                        );
                    }
                    else // queryType == Query.eType.Delete
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
                ColumnSchema[] strippedColumnSchemas = tableSchema.ColumnSchemas.SubArray<ColumnSchema>(
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

            internal static string GetAuditedTableName(string auditTableName)
            {
                // cut the Audit prefix from audit table name
                return auditTableName.GetSubstringAfterPrefix(
                    AuditTableStructure.AUDIT_TABLE_NAME_PREFIX);
            }

            internal static string GetAuditTableName(string tableName)
            {
                // add the Audit prefix to table name
                return AuditTableStructure.AUDIT_TABLE_NAME_PREFIX + tableName;
            }

            internal static TriggerSchema GetAuditTriggerSchema(
                TableSchema triggeredTableSchema,
                TableSchema auditTableSchema,
                Query.eType queryType)
            {
                TriggerSchema auditTriggerSchema;

                InsertQuery onTriggerAuditInsertQuery =
                    getOnTriggerAuditInsertQuery(
                    triggeredTableSchema,
                    auditTableSchema,
                    queryType);

                auditTriggerSchema = new TriggerSchema(
                    getTriggerName(triggeredTableSchema, queryType),
                    getTriggerTime(queryType),
                    queryType,
                    triggeredTableSchema.Name,
                    onTriggerAuditInsertQuery);

                return auditTriggerSchema;
            }

            private static TriggerSchema.eTime getTriggerTime(Query.eType queryType)
            {
                TriggerSchema.eTime triggerTime = 0; // init to temp value

                if(queryType == Query.eType.Insert)
                {
                    triggerTime = TriggerSchema.eTime.After;
                }
                else if(queryType == Query.eType.Update || queryType == Query.eType.Delete)
                {
                    triggerTime = TriggerSchema.eTime.Before;
                }

                return triggerTime;
            }

            private static string getTriggerName(TableSchema triggeredTableSchema, Query.eType queryType)
            {
                string queryTypeString =
                    Query.QueryTypeToString(queryType).UppercaseOnlyCharactersInIndices(0);

                String triggerName = string.Format(
                    "on{0}{1}",
                    triggeredTableSchema.Name,
                    queryTypeString);

                return triggerName;
            }

            private static InsertQuery getOnTriggerAuditInsertQuery(
                TableSchema triggeredTableSchema,
                TableSchema auditTableSchema,
                Query.eType queryType)
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
                        getTriggeredTableIdValuedColumn(
                            triggeredTableSchema,
                            auditTableSchema,
                            queryType)
                };

                // add values of rest of ValuedColumns of row which was triggered in
                // triggeredTable

                // convert each (non-primary key) ValuedColumn (key, value) of triggered row in
                // triggeredTable,
                // into a TriggerValuedColumn (key, [TriggerValuedColumn.eTime].key)
                // in the auditTable InsertQuery
                // (note that column names in auditTable starting from index 3 match column names in 
                // triggeredTable starting from index 1)
                TriggerValuedColumn.eTime triggerValuedColumnTime = queryType == Query.eType.Insert
                    ? TriggerValuedColumn.eTime.New
                    : TriggerValuedColumn.eTime.Old;

                TriggerValuedColumn[] auditTableTriggerValueColumns =
                    triggeredTableSchema.ColumnSchemas.SubArray(1).Select(
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

            private static ValuedColumn getTriggeredTableIdValuedColumn(
                TableSchema triggeredTableSchema,
                TableSchema auditTableSchema,
                Query.eType queryType)
            {
                ValuedColumn triggeredTableIdValuedColumn = null;

                // trigger tabled id column is third in audit table
                string columnName = auditTableSchema.ColumnSchemas[2].Name;

                if (queryType == Query.eType.Insert) // get the ID the row got after being inserted
                {
                    object columnValue = new FunctionTableColumn(
                        FunctionTableColumn.eFunctionType.LastInsertRowid);
                    triggeredTableIdValuedColumn = new ValuedColumn(columnName, columnValue);
                }
                else if(queryType == Query.eType.Update || queryType == Query.eType.Delete)
                {
                    // get the id the row had before being updated / deleted
                    triggeredTableIdValuedColumn = new TriggerValuedColumn(
                        columnName,
                        triggeredTableSchema.PrimaryKeyColumnSchema.Name,
                        TriggerValuedColumn.eTime.Old);
                }

                return triggeredTableIdValuedColumn;
            }

            private static SelectQuery getQueryTypeIdSelectQuery(Query.eType queryType)
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
                        BasicCondition.eComparisonType.Equal
                    )
                );

                return queryTypeIdSelectQuery;
            }


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
                        BasicCondition.eComparisonType.Like
                    )
                );

                return auditTableNamesSelectQuery;
            }
        }
    }
}

