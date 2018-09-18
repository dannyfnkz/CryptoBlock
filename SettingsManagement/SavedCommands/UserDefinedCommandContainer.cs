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
        /// contains and handles operations on saved <see cref="UserDefinedCommand"/>s.
        /// </summary>
        internal class UserDefinedCommandContainer
        {
            /// <summary>
            /// thrown if a <see cref="UserDefinedCommand"/> associated with the specified alias
            /// was not found.
            /// </summary>
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

            /// <summary>
            /// array containing all saved <see cref="UserDefinedCommand"/>s.
            /// </summary>
            [JsonProperty]
            internal UserDefinedCommand[] UserDefinedCommands
            {
                get { return userDefinedCommandAliasToUserDefinedCommand.Values.ToArray(); }
            }

            /// <summary>
            /// adds specified <see cref="UserDefinedCommand"/>.
            /// </summary>
            /// <param name="userDefinedCommand"></param>
            internal void AddUserDefinedCommand(UserDefinedCommand userDefinedCommand)
            {
                this.userDefinedCommandAliasToUserDefinedCommand[userDefinedCommand.Alias] =
                    userDefinedCommand;
            }

            /// <summary>
            /// returns whether <see cref="UserDefinedCommand"/> associated with
            /// specified <paramref name="savedCommandAlias"/> exists in container.
            /// </summary>
            /// <param name="savedCommandAlias"></param>
            /// <returns>
            /// true if <see cref="UserDefinedCommand"/> associated with
            /// specified <paramref name="savedCommandAlias"/> exists in container,
            /// else false
            /// </returns>
            internal bool UserDefinedCommandExists(string savedCommandAlias)
            {
                return this.userDefinedCommandAliasToUserDefinedCommand.ContainsKey(savedCommandAlias);
            }

            /// <summary>
            /// returns <see cref="UserDefinedCommand"/> associated with 
            /// <paramref name="userDefinedCommandAlias"/>.
            /// </summary>
            /// <param name="userDefinedCommandAlias"></param>
            /// <returns>
            /// <see cref="UserDefinedCommand"/> associated with 
            /// <paramref name="userDefinedCommandAlias"/>
            /// </returns>
            /// <exception cref="UserDefinedCommandNotFoundException">
            /// <seealso cref="assertUserDefinedCommandExists(string)"/>
            /// </exception>
            internal UserDefinedCommand GetUserDefinedCommand(string userDefinedCommandAlias)
            {
                assertUserDefinedCommandExists(userDefinedCommandAlias);
                return this.userDefinedCommandAliasToUserDefinedCommand[userDefinedCommandAlias];
            }

            /// <summary>
            /// removes <see cref="UserDefinedCommand"/> 
            /// associated with <paramref name="savedCommandAlias"/> from container.
            /// </summary>
            /// <param name="savedCommandAlias"></param>
            /// <exception cref="UserDefinedCommandNotFoundException">
            /// <seealso cref="assertUserDefinedCommandExists(string)"/>
            /// </exception>
            internal void RemoveUserDefinedCommand(string savedCommandAlias)
            {
                assertUserDefinedCommandExists(savedCommandAlias);
                this.userDefinedCommandAliasToUserDefinedCommand.Remove(savedCommandAlias);
            }

            /// <summary>
            /// removes all <see cref="UserDefinedCommand"/>s from container.
            /// </summary>
            internal void ClearUserDefinedCommands()
            {
                this.userDefinedCommandAliasToUserDefinedCommand.Clear();
            }

            /// <summary>
            /// build an alias-to-commandString <see cref="Dictionary{string, UserDefinedCommand}"/>
            /// from specified <paramref name="userDefinedCommands"/> array.
            /// </summary>
            /// <param name="userDefinedCommands"></param>
            /// <returns>
            /// Alias-to-CommandString <see cref="Dictionary{string, UserDefinedCommand}"/>
            /// built from specified <paramref name="userDefinedCommands"/> array
            /// </returns>
            private static Dictionary<string, UserDefinedCommand>
                buildUserDefinedCommandAliasToUserDefinedCommandDictionaryFromArray(
                UserDefinedCommand[] userDefinedCommands)
            {
                Dictionary<string, UserDefinedCommand> userDefinedCommandAliasToUserDefinedCommand
                    = new Dictionary<string, UserDefinedCommand>();

                // associate each UserDefinedCommand's Alias with its CommandString
                foreach (UserDefinedCommand userDefinedCommand in userDefinedCommands)
                {
                    userDefinedCommandAliasToUserDefinedCommand.Add(
                        userDefinedCommand.Alias,
                        userDefinedCommand);
                }

                return userDefinedCommandAliasToUserDefinedCommand;
            }

            /// <summary>
            /// asserts that there exists a <see cref="UserDefinedCommand"/> associated with
            /// specified <paramref name="userDefinedCommandAlias"/>.
            /// </summary>
            /// <seealso cref="UserDefinedCommandExists(string)"/>
            /// <param name="userDefinedCommandAlias"></param>
            /// <exception cref="UserDefinedCommandNotFoundException">
            /// thrown if a <see cref="UserDefinedCommand"/> associated with
            /// specified <paramref name="userDefinedCommandAlias"/> does not exist
            /// </exception>
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