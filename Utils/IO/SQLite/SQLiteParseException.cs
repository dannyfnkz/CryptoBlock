using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite
    {
        /// <summary>
        /// thrown if parse of an SQLite object fails.
        /// </summary>
        public class SQLiteParseExcetion : Exception
        {
            private Type objectType;

            protected SQLiteParseExcetion(
                Type objectType,
                string additionalDetails = null,
                Exception innerException = null)
                : base(formatExceptionMessage(additionalDetails, objectType), innerException)
            {
                this.objectType = objectType;
            }

            public Type ObjectType
            {
                get { return objectType; }
            }

            private static string formatExceptionMessage(
                string additionalDetails,
                Type objectType)
            {
                string appendant = additionalDetails == null
                    ? "."
                    : string.Format(": {0}", additionalDetails);

                string exceptionMessage = string.Format(
                    "An exception occurred while trying to parse {0}{1}",
                    objectType.FullName,
                    appendant);

                return exceptionMessage;
            }
        }
    }
}