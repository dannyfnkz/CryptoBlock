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
        public class TriggerSchema : Schema
        {
            public enum eTime
            {
                Before, After
            }

            private readonly string name;
            private readonly eTime time;
            private readonly Query.eType queryType;
            private readonly string triggeredTableName;
            private readonly Query triggeredQuery;

            public TriggerSchema(
                string name,
                eTime time,
                Query.eType queryType,
                string triggeredTableName,
                Query triggeredQuery)
            {
                this.name = name;
                this.time = time;
                this.queryType = queryType;
                this.triggeredTableName = triggeredTableName;
                this.triggeredQuery = triggeredQuery;
            }

            public string Name
            {
                get { return name; }
            }

            public eTime Time
            {
                get { return time; }
            }

            public Query.eType QueryType
            {
                get { return queryType; }
            }

            public string TriggeredTableName
            {
                get { return triggeredTableName; }
            }

            public Query TriggeredQuery
            {
                get { return triggeredQuery; }
            }

            public static string TimeToString(eTime time)
            {
                string timeString = time.ToString().ToUpper();

                return timeString;
            }

            public static string TypeToString(Query.eType queryType)
            {
                string queryTypeString = queryType.ToString().ToUpper();

                return queryTypeString;
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append trigger name, time, type and triggered table name
                queryStringBuilder.AppendFormat("TRIGGER {0}", this.Name);
                queryStringBuilder.AppendFormat(
                    " {0} {1} ON {2} FOR EACH ROW",
                    TimeToString(this.Time),
                    TypeToString(this.QueryType),
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

