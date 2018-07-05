using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling
    {
        public class CommandExecutionException : Exception
        {
            public CommandExecutionException(string message, Exception innerException)
                 : base(message, innerException)
            {

            }

            public CommandExecutionException(string message)
                : base(message)
            {

            }

            public CommandExecutionException(Exception innerException)
                 : base(string.Empty, innerException)
            {

            }
        }
    }
}