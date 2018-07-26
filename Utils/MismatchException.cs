using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// thrown if two properties which were supposed to be equal, were in fact not equal.
        /// </summary>
        public class MismatchException : Exception
        {
            private string firstPropertyName;
            private string secondPropertyName;

            public MismatchException(string firstPropertyName, string secondPropertyName)
                : base(formatExceptionMessage(firstPropertyName, secondPropertyName))
            {
                this.firstPropertyName = firstPropertyName;
                this.secondPropertyName = secondPropertyName;
            }

            /// <summary>
            /// name of first property.
            /// </summary>
            public string FirstPropertyName
            {
                get { return firstPropertyName; }
            }

            /// <summary>
            /// name of second property.
            /// </summary>
            public string SecondPropertyName
            {
                get { return secondPropertyName; }
            }

            private static string formatExceptionMessage(string firstPropertyName, string secondPropertyName)
            {
                return string.Format(
                    "{0} must be the same as {1}.",
                    firstPropertyName,
                    secondPropertyName);
            }
        }
    }
}

