using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml
    {
        public static class XmlNodeExtensionMethods
        {
            public static bool ContainsElement(this XmlNode xmlNode, string xPath)
            {
                return xmlNode.SelectNodes(xPath).Count > 0;
            }
        }
    }
}