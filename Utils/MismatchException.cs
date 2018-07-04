using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
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

            public string FirstPropertyName
            {
                get { return firstPropertyName; }
            }

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

