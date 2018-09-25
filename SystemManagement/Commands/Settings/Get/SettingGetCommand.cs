using CryptoBlock.CommandHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings.Get
    {
        /// <summary>
        /// represents a <see cref="SettingCommand"/> which logs to console information
        /// regarding a particular setting.
        /// </summary>
        internal abstract class SettingGetCommand : SettingCommand
        {
            private const string PREFIX = "get";

            internal SettingGetCommand(
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