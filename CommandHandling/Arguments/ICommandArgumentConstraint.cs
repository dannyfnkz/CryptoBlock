using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling.Arguments
    {
        /// <summary>
        /// represents a constraint on command argument array passed to <see cref="Command"/> as parameter
        /// on <see cref="Command.Execute(string[])"/>.
        /// </summary>
        public interface ICommandArgumentConstraint
        {
            /// <summary>
            /// returns whether <paramref name="commandArgumentArray"/> is valid.
            /// </summary>
            /// <param name="commandArgumentArray"></param>
            /// <returns>
            /// true if <paramref name="commandArgumentArray"/> is valid,
            /// else false
            /// </returns>
            bool IsValid(String[] commandArgumentArray);

            /// <summary>
            /// action to be performed in case <paramref name="commandArgumentArray"/>
            /// is found to be invalid.
            /// </summary>
            /// <param name="commandArgumentArray"></param>
            void OnInvalidCommandArgumentArray(string[] commandArgumentArray);
        }
    }
}

