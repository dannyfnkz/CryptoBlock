using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings
    {
        /// <summary>
        /// represents a <see cref="SystemCommand"/> which handles the app settings.
        /// </summary>
        internal abstract class SettingCommand : SystemCommand
        {
            private const string BASE_PREFIX = "settings";

            internal SettingCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(
                      FormatPrefix(BASE_PREFIX,
                      inheritingCommandPrefix),
                      minNumberOfArguments,
                      maxNumberOfArguments)
            {

            }
        }
    }
}
