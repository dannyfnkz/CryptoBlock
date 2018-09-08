using CryptoBlock.Utils.IO.SQLite.Schemas.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.SchemaQueries.Write
    {
        /// <summary>
        /// represents a <see cref="SchemaWriteQuery"/> which creates a trigger.
        /// </summary>
        public class CreateTriggerQuery : WriteQuery
        {
            private readonly TriggerSchema triggerSchema;

            public CreateTriggerQuery(TriggerSchema triggerSchema)
            {
                this.triggerSchema = triggerSchema;
            }

            public TriggerSchema TriggerSchema
            {
                get { return triggerSchema; }
            }

            protected override string BuildQueryString()
            {
                StringBuilder queryStringBuilder = new StringBuilder();

                // append header
                queryStringBuilder.Append("CREATE ");

                // append table schema query string
                queryStringBuilder.Append(this.TriggerSchema.ExpressionString);

                return queryStringBuilder.ToString();
            }
        }
    }

}
