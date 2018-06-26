using System;
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
            //try
            //{

            //    ProgramManager programManager = new ProgramManager();
            //    programManager.StartProgram();
            //}
            //catch(DataRequestException e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            string[] names = new string[] { "limit1", "sort1" };
            string[] values = new string[] { "10", "id" };
            string uri = @"https://api.coinmarketcap.com/v2/ticker/";

            HttpGetRequestHandler.GetRequestParameter[] parameters =
                HttpGetRequestHandler.GetRequestParameter.ToGetRequestParameterArray(names, values);

            HttpGetRequestHandler handler = new HttpGetRequestHandler(uri);
            handler.SendRequest();

            string response = handler.Response;

        }
    }
}
