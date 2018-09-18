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
        internal class UserDefinedCommandContainer
        {
            internal class UserDefinedCommandNotFoundException : Exception
            {
                private readonly string userDefinedCommandAlias;

                internal UserDefinedCommandNotFoundException(string userDefinedCommandAlias)
                {
                    this.userDefinedCommandAlias = userDefinedCommandAlias;
                }

                internal string UserDefinedCommandAlias
                {
                    get { return userDefinedCommandAlias; }
                }

                private static string formatExceptionMessage(string userDefinedCommandAlias)
                {
                    return string.Format(
                        "User Defined Command with specified alias '{0}' not found.",
                        userDefinedCommandAlias);
                }
            }

            private Dictionary<string, UserDefinedCommand> userDefinedCommandAliasToUserDefinedCommand;
                
            internal UserDefinedCommandContainer()
            {
                this.userDefinedCommandAliasToUserDefinedCommand =
                    new Dictionary<string, UserDefinedCommand>();
            }

            [JsonConstructor]
            internal UserDefinedCommandContainer(UserDefinedCommand[] UserDefinedCommands)
            {
                this.userDefinedCommandAliasToUserDefinedCommand =
                    buildUserDefinedCommandAliasToUserDefinedCommandDictionaryFromArray(
                        UserDefinedCommands);
            }

            [JsonProperty]
            internal UserDefinedCommand[] UserDefinedCommands
            {
                get { return userDefinedCommandAliasToUserDefinedCommand.Values.ToArray(); }
            }

            internal UserDefinedCommandContainer(IEnumerable<UserDefinedCommand> userDefinedCommands)
            {
                foreach(UserDefinedCommand userDefinedCommand in userDefinedCommands)
                {
                    userDefinedCommandAliasToUserDefinedCommand[userDefinedCommand.Alias] =
                        userDefinedCommand;
                }
            }

            internal void AddUserDefinedCommand(UserDefinedCommand userDefinedCommand)
            {
                this.userDefinedCommandAliasToUserDefinedCommand[userDefinedCommand.Alias] =
                    userDefinedCommand;
            }

            internal bool UserDefinedCommandExists(string savedCommandAlias)
            {
                return this.userDefinedCommandAliasToUserDefinedCommand.ContainsKey(savedCommandAlias);
            }

            internal UserDefinedCommand GetUserDefinedCommand(string userDefinedCommandAlias)
            {
                assertUserDefinedCommandExists(userDefinedCommandAlias);
                return this.userDefinedCommandAliasToUserDefinedCommand[userDefinedCommandAlias];
            }

            internal void RemoveUserDefinedCommand(string savedCommandAlias)
            {
                assertUserDefinedCommandExists(savedCommandAlias);
                this.userDefinedCommandAliasToUserDefinedCommand.Remove(savedCommandAlias);
            }

            internal void ClearUserDefinedCommands()
            {
                this.userDefinedCommandAliasToUserDefinedCommand.Clear();
            }

            private static Dictionary<string, UserDefinedCommand>
                buildUserDefinedCommandAliasToUserDefinedCommandDictionaryFromArray(
                UserDefinedCommand[] userDefinedCommands)
            {
                Dictionary<string, UserDefinedCommand> userDefinedCommandAliasToUserDefinedCommand
                    = new Dictionary<string, UserDefinedCommand>();

                foreach (UserDefinedCommand userDefinedCommand in userDefinedCommands)
                {
                    userDefinedCommandAliasToUserDefinedCommand.Add(
                        userDefinedCommand.Alias,
                        userDefinedCommand);
                }

                return userDefinedCommandAliasToUserDefinedCommand;
            }

            private void assertUserDefinedCommandExists(string userDefinedCommandAlias)
            {
                if(!UserDefinedCommandExists(userDefinedCommandAlias))
                {
                    throw new UserDefinedCommandNotFoundException(userDefinedCommandAlias);
                }
            }
        }
    }
}