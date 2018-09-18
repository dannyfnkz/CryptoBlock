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

            [JsonProperty]
            public string Alias
            {
                get { return alias; }
            }

            [JsonProperty]
            public string CommandString
            {
                get { return commandString; }
            }
        }
    }
}