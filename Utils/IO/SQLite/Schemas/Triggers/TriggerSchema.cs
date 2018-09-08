using CryptoBlock.Utils.IO.SQLite.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Schemas.Triggers
    {
        /// <summary>
        /// represents a trigger <see cref="Schema"/>.
        /// </summary>
        public class TriggerSchema : Schema
        {
            /// <summary>
            /// time relative to triggering query, when <see cref="TriggeredQuery"/> is triggered.
            /// </summary>
            public enum eTriggeredQueryTime
            {
                Before, After
            }

            private readonly string name;
            private readonly eTriggeredQueryTime triggeredQueryTime;
            private readonly Query.eQueryType triggeringQueryType;
            private readonly string triggeredTableName;
            private readonly Query triggeredQuery;

            public TriggerSchema(
                string name,
                eTriggeredQueryTime triggeredQueryTime,
                Query.eQueryType triggeringQueryType,
                string triggeredTableName,
                Query triggeredQuery)
            {
                this.name = name;
                this.triggeredQueryTime = triggeredQueryTime;
                this.triggeringQueryType = triggeringQueryType;
                this.triggeredTableName = triggeredTableName;
                this.triggeredQuery = triggeredQuery;
            }

            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// time relative to triggering query, when <see cref="TriggeredQuery"/> is triggered.
            /// </summary>
            public eTriggeredQueryTime TriggeredQueryTime
            {
                get { return triggeredQueryTime; }
            }

            /// <summary>
            /// type of triggering query.
            /// </summary>
            public Query.eQueryType TriggeringQueryType
            {
                get { return triggeringQueryType; }
            }

            
            public string TriggeredTableName
            {
                get { return triggeredTableName; }
            }

            public Query TriggeredQuery
            {
                get { return triggeredQuery; }
            }

            public static string TriggeredQueryTimeToString(eTriggeredQueryTime triggeredQueryTime)
            {
                string triggeredQueryTimeString = triggeredQueryTime.ToString().ToUpper();

                return triggeredQueryTimeString;
            }

            protected override string BuildExpressionString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append trigger name, time, type and triggered table name
                queryStringBuilder.AppendFormat("TRIGGER {0}", this.Name);
                queryStringBuilder.AppendFormat(
                    " {0} {1} ON {2} FOR EACH ROW",
                    TriggeredQueryTimeToString(this.TriggeredQueryTime),
                    Query.QueryTypeToString(this.TriggeringQueryType),
                    this.triggeredTableName);

                // append query to be performed for each row
                queryStringBuilder.Append(" BEGIN");
                queryStringBuilder.AppendFormat(" {0};", this.TriggeredQuery.QueryString);
                queryStringBuilder.Append(" END");

                return queryStringBuilder.ToString();
            }
        }
    }
}

