﻿using CryptoBlock.CommandHandling;
using CryptoBlock.CommandHandling.Arguments;
using CryptoBlock.IOManagement;
using CryptoBlock.ServerDataManagement.Commands;
using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections.List;
using System;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace ServerDataManagement
    {
        /// <summary>
        /// handles executing server data commands.
        /// </summary>
        public class ServerDataCommandExecutor : CommandExecutor
        {
            private const string COMMAND_TYPE = "ServerData";

            public ServerDataCommandExecutor()
            {
                // add associations between commands and their prefixes
                AddCommandPrefixToCommandPair(
                    new CoinTickerCommmand(),
                    new CoinListingCommand());
            }

            /// <summary>
            /// returns <see cref="ServerDataCommandExecutor"/> command type.
            /// </summary>
            public override string CommandType
            {
                get { return COMMAND_TYPE; }
            }
        }
    }
}
