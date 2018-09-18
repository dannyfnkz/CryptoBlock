﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SystemManagement.Commands.Settings.Get
    {
        internal abstract class SettingGetCommand : SettingCommand
        {
            private const string PREFIX = "get";

            internal SettingGetCommand(
                string inheritingCommandPrefix,
                int minNumberOfArguments,
                int maxNumberOfArguments)
                : base(formatPrefix(inheritingCommandPrefix), minNumberOfArguments, maxNumberOfArguments)
            {

            }

            /// <summary>
            /// returns command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
            /// <see cref="Command.Prefix"/>.
            /// </summary>
            /// <param name="inheritingCommandPrefix"></param>
            /// <returns>
            /// command prefix formulated by concatenating <paramref name="inheritingCommandPrefix"/> to
            /// <see cref="Command.Prefix"/>.
            /// </returns>
            private static string formatPrefix(string inheritingCommandPrefix)
            {
                return PREFIX + " " + inheritingCommandPrefix;
            }
        }
    }
}