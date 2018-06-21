﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;
using static CryptoBlock.CMCAPI.RequestHandler;
using CryptoBlock.Utils;

namespace CryptoBlock
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                ProgramManager programManager = new ProgramManager();
                programManager.StartProgram();
            }
            catch(DataRequestException e)
            {
                Console.WriteLine(e.Message);
            }
           
        }
    }
}
