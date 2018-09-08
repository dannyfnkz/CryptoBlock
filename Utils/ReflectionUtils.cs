
using System;
using System.Reflection;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains methods which provide additional utility for <see cref="System.Reflection"/>.
        /// </summary>
        public static class ReflectionUtils
        {
            /// <summary>
            /// returns value of property in <paramref name="obj"/> with <paramref name="propertyName"/>,
            /// or null if property with <paramref name="propertyName"/> does not exist in <paramref name="obj"/>.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="propertyName"></param>
            /// <returns>
            /// value of property in <paramref name="obj"/>with <paramref name="propertyName"/>,
            /// or null if property with <paramref name="propertyName"/> does not exist in <paramref name="obj"/>.
            /// </returns>
            public static object GetPropertyValue(object obj, string propertyName)
            {
                if(HasPublicProperty(obj, propertyName))
                {
                    return obj.GetType().GetProperty(propertyName).GetValue(obj);
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// returns whether <paramref name="obj"/>'s <see cref="Type"/> has a public property
            /// whose name is <paramref name="propertyName"/>.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="propertyName"></param>
            /// <returns>
            /// true if <paramref name="obj"/>'s <see cref="Type"/> has a public property 
            /// whose name is <paramref name="propertyName"/>,
            /// else false
            /// </returns>
            /// <seealso cref="HasPublicProperty(Type, string)"/>
            public static bool HasPublicProperty(object obj, string propertyName)
            {
                return HasPublicProperty(obj.GetType(), propertyName);
            }

            /// <summary>
            /// returns whether <paramref name="type"/> has a public property
            /// whose name is <paramref name="propertyName"/>.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="propertyName"></param>
            /// <returns>
            /// true if <paramref name="type"/> has a public property whose name is <paramref name="propertyName"/>,
            /// else false
            /// </returns>
            /// <seealso cref="System.Type.GetProperty(string)"/>
            public static bool HasPublicProperty(Type type, string propertyName)
            {
                return type.GetProperty(propertyName) != null;
            }

            // returns all instance FieldInfo[] of type. this does not include fields of base class,
            // in case type derives from another class.

            /// <summary>
            /// returns all instance <see cref="FieldInfo"/>s of specified <paramref name="type"/>.
            /// </summary>
            /// <remarks>
            /// base class fields are not included.
            /// </remarks>
            /// <seealso cref="FieldInfo"/>
            /// <seealso cref="Type.GetFields(BindingFlags)"/>
            /// <param name="type"></param>
            /// <returns>
            /// all instance <see cref="FieldInfo"/>s of <paramref name="type"/>
            /// </returns>
            public static FieldInfo[] GetInstanceFieldInfo(Type type)
            {
                FieldInfo[] instanceFields = type.GetFields(
                     BindingFlags.Public |
                     BindingFlags.NonPublic |
                     BindingFlags.Instance);

                return instanceFields;
            }
        }
    }
}
