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
        /// contains extension methods for <see cref="XmlNode"/>.
        /// </summary>
        public static class XmlNodeExtensionMethods
        {
            /// <exception cref="XmlNodeMissingNodeException">
            /// <seealso cref="XmlNodeUtils.AssertContainsNode(XmlNode, string)"/>
            /// </exception>
            public static XmlNodeList GetNodes(this XmlNode xmlNode, string xPath)
            {
                XmlNodeUtils.AssertContainsNode(xmlNode, xPath);

                return xmlNode.SelectNodes(xPath);
            }

            /// <summary>
            /// returns whether <paramref name="xmlNode"/> contains nodes with specified
            /// <paramref name="xPath"/>.
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="xPath"></param>
            /// <returns>
            /// true if <paramref name="xmlNode"/> contains nodes with specified
            /// <paramref name="xPath"/>,
            /// else false
            /// </returns>
            public static bool ContainsNodes(this XmlNode xmlNode, string xPath)
            {
                return xmlNode.SelectNodes(xPath).Count > 0;
            }


            /// <exception cref="XmlNodeMissingAttributeException">
            /// <seealso cref="GetAttributeValue(XmlNode, string)"/>
            /// </exception>
            /// <exception cref="InvalidAttributeTypeException">
            /// thrown if conversion of attribute value to <typeparamref name="T"/> failed
            /// </exception>
            public static T GetAttributeValue<T>(this XmlNode xmlNode, string attributeName)
            {
                try
                {
                    string attributeValueString = xmlNode.GetAttributeValue(attributeName);
                    T convertedAttributeValue = (T)Convert.ChangeType(attributeValueString, typeof(T));

                    return convertedAttributeValue;
                }
                catch(Exception exception)
                {
                    // converting from string to T failed
                    if(
                        exception is InvalidCastException
                        || exception is FormatException
                        || exception is OverflowException
                        || exception is ArgumentNullException)
                    {
                        throw new InvalidAttributeTypeException(
                            attributeName,
                            typeof(T),
                            exception);
                    }
                    else // unhandled exception
                    {
                        throw exception;
                    }
                }
            }

            /// <summary>
            /// returns value of <paramref name="xmlNode"/> attribute having
            /// <paramref name="attributeName"/>.
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="attributeName"></param>
            /// <returns>
            /// value of <paramref name="xmlNode"/> attribute having
            /// <paramref name="attributeName"/>
            /// </returns>
            /// <exception cref="XmlNodeMissingAttributeException">
            /// <seealso cref="XmlNodeUtils.AssertContainsAttributes(XmlNode, string[])"/>
            /// </exception>
            public static string GetAttributeValue(this XmlNode xmlNode, string attributeName)
            {
                XmlNodeUtils.AssertContainsAttributes(xmlNode, attributeName);

                return xmlNode.Attributes[attributeName].Value;
            }

            /// <summary>
            /// returns whether <paramref name="xmlNode"/> contains an attribute with specified
            /// <paramref name="attributeName"/>.
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="attributeName"></param>
            /// <returns>
            /// true if <paramref name="xmlNode"/> contains an attribute with specified
            /// <paramref name="attributeName"/>,
            /// else false
            /// </returns>
            public static bool ContainsAttribute(this XmlNode xmlNode, string attributeName)
            {
                return xmlNode.Attributes != null
                    && xmlNode.Attributes[attributeName] != null;
            }

            /// <summary>
            /// returns whether <paramref name="xmlNode"/> contains an attribute with specified
            /// <paramref name="attributeIndex"/>.
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="attributeIndex"></param>
            /// <returns>
            /// true if <paramref name="xmlNode"/> contains an attribute with specified
            /// <paramref name="attributeIndex"/>,
            /// else false
            /// </returns>
            public static bool ContainsAttribute(this XmlNode xmlNode, int attributeIndex)
            {
                return xmlNode.Attributes != null
                    && xmlNode.Attributes[attributeIndex] != null;
            }
        }
    }
}