using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;
using static CryptoBlock.CMCAPI.RequestHandler;
using CryptoBlock.Utils;
using System.Threading;

namespace CryptoBlock
{
    class Program
    {
        static void Main(string[] args)
        {
            new ProgramManager().StartProgram();

        }
    }
}
