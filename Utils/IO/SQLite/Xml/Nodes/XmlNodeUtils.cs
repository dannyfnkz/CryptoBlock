using CryptoBlock.Utils.IO.SQLite.Xml.Nodes.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Xml.Nodes
    {
        /// <summary>
        /// contains utility methods for <see cref="XmlNode"/>.
        /// </summary>
        public static class XmlNodeUtils
        {
            /// <summary>
            /// asserts that <paramref name="xmlNode"/> contained the specified required
            /// <paramref name="attributeNames"/>.
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="attributeNames"></param>
            /// <exception cref="MissingAttributeXmlNodeParseException">
            /// thrown if <paramref name="xmlNode"/> did not contain a specified required
            /// attribute name
            /// </exception>
            internal static void AssertContainsAttributes(
                XmlNode xmlNode,
                params string[] attributeNames)
            {
                foreach (string attributeName in attributeNames)
                {
                    if (!xmlNode.ContainsAttribute(attributeName))
                    {
                        throw new XmlNodeMissingAttributeException(attributeName);
                    }
                }
            }

            /// <exception cref="MissingNodeException">
            /// thrown if <paramref name="xmlNode"/> does not contain node(s) specified by
            /// <paramref name="xPath"/>
            /// </exception>
            internal static void AssertContainsNode(
                XmlNode xmlNode,
                string xPath)
            {
                if(!xmlNode.ContainsNodes(xPath))
                {
                    throw new XmlNodeMissingNodeException(xPath);
                }
            }
        }
    }
}