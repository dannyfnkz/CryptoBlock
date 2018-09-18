using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SettingsManagement.SavedCommands
    {
        /// <summary>
        /// represents a user defined command. when <see cref="Alias"/> is entered by the user,
        /// the command executor handles the associated <see cref="CommandString"/>.
        /// </summary>
        public class UserDefinedCommand
        {
            private readonly string alias;
            private readonly string commandString;

            [JsonConstructor]
            public UserDefinedCommand(string alias, string commandString)
            {
                this.alias = alias;
                this.commandString = commandString;
            }

            /// <summary>
            /// alias associated with command which is invoked by entering <see cref="commandString"/>.
            /// </summary>
            [JsonProperty]
            public string Alias
            {
                get { return alias; }
            }

            /// <summary>
            /// string representation of the command, handled by the command executor.
            /// </summary>
            [JsonProperty]
            public string CommandString
            {
                get { return commandString; }
            }
        }
    }
}