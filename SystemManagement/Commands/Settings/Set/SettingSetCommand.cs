using CryptoBlock.CommandHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings.Set
    {
        /// <summary>
        /// represents a <see cref="SettingCommand"/> which lets the user to set the value
        /// of a particular <see cref="SettingCommand"/>.
        /// </summary>
        internal abstract class SettingSetCommand : SettingCommand
        {
            private const string PREFIX = "set";

            internal SettingSetCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(
                      Command.FormatPrefix(PREFIX, inheritingCommandPrefix), 
                      minNumberOfArguments, 
                      maxNumberOfArguments)
            {

            }
        }
    }
}