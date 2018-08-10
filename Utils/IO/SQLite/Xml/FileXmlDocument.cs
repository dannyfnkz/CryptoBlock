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
        public class FileXmlDocument : XmlDocument
        {
            private string filePath;

            // throws XmlException if XML file was corrupt
            public FileXmlDocument(string filePath)
            {
                this.filePath = filePath;

                string xmlFileContent = FileIO.FileIOUtils.ReadTextFromFile(filePath);
                base.LoadXml(xmlFileContent);
            }

            public string FilePath
            {
                get { return filePath; }
            }
        }
    }

}