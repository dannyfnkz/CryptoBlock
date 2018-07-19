using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoBlock.CMCAPI;
using static CryptoBlock.CMCAPI.RequestHandler;
using CryptoBlock.Utils;
using System.Threading;
using CryptoBlock.TableDisplay;
using System.Reflection;
using CryptoBlock.IOManagement;

namespace CryptoBlock
{

    class Program
    {
        static void Main(string[] args)
        {
                     new ProgramManager().StartProgram();

    //        PropertyTable table = new PropertyTable();

    //        PropertyTable.PropertyColumn propertyColumn1 = new PropertyTable.PropertyColumn(
    //            "A",
    //            7,
    //            new PropertyTable.Property(test1.GetType(), "A"));
    //            PropertyTable.PropertyColumn propertyColumn2 = new PropertyTable.PropertyColumn(
    //            "B",
    //            7,
    //            new PropertyTable.Property(test1.GetType(), "B"));

    //        PropertyTable.PropertyRow propertyRow = new PropertyTable.PropertyRow(
    //            new object[] { test1, test2 },
    //            new PropertyTable.Property[]
    //            {
    //                new PropertyTable.Property(test1.GetType(), "A"),
    //                new PropertyTable.Property(test2.GetType(), "B")
    //            });
    //        PropertyTable.PropertyRow propertyRow2 = new PropertyTable.PropertyRow(
    //new object[] { test2, test2 },
    //new PropertyTable.Property[]
    //{
    //                new PropertyTable.Property(test1.GetType(), "A"),
    //                new PropertyTable.Property(test2.GetType(), "B")
    //});

    //        table.AddColumn(propertyColumn1);
    //        table.AddColumn(propertyColumn2);

    //        table.AddRowRange(new PropertyTable.PropertyRow[] { propertyRow, propertyRow2 });

    //        Console.Write(table.GetTableString());
        }
    }
}
