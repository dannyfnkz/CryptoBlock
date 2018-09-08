using CryptoBlock.Utils.IO.FileIO;
using CryptoBlock.Utils.IO.SQLite.Xml.Documents.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml.Documents
    {
        /// <summary>
        /// represents an <see cref="XmlDocument"/>, loaded from a file.
        /// </summary>
        public class FileXmlDocument : XmlDocument
        {
            private string filePath;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="filePath"></param>
            /// <exception cref="FileXmlDocumentInitializationException">
            /// <seealso cref="initiailize"/>
            /// </exception>
            public FileXmlDocument(string filePath)
            {
                this.filePath = filePath;
                initiailize();
            }

            public string FilePath
            {
                get { return filePath; }
            }

            /// <summary>
            /// initializes this <see cref="FileXmlDocument"/>.
            /// </summary>
            /// <exception cref="FileXmlDocumentInitializationException">
            /// thrown if <see cref="FileXmlDocument"/> initialization failed.
            /// </exception>
            private void initiailize()
            {
                try
                {
                    string xmlFileContent = FileIO.FileIOUtils.ReadTextFromFile(filePath);
                    base.LoadXml(xmlFileContent);
                }
                catch(Exception exception)
                {
                    if(exception is FileReadException || exception is XmlException)
                    {
                        throw new FileXmlDocumentInitializationException(this.FilePath, exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }
        }
    }

}