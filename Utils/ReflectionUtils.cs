
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

          //  // if derivedClassObject is a subclass of T,
          //  // converts derivedClassObject into an object of type T.
          //  // else, returns null
          //  public static T ForceType<T>(object derivedClassObject) where T : class
          //  {
          //      if(!(derivedClassObject is T))
          //      {
          //          return null;
          //      }

          //      T baseClassObject;

          //      // creates a new object of type T without calling a constructor
          //      baseClassObject = (T)System.Runtime.Serialization.FormatterServices
          //.GetUninitializedObject(typeof(T));

          //      Type derivedClassType = derivedClassObject.GetType();
          //      Type baseClassType = baseClassObject.GetType();

          //      FieldInfo[] baseClassTypeFieldInfos = GetInstanceFieldInfo(baseClassType);

          //      foreach (FieldInfo baseClassTypeFieldInfo in baseClassTypeFieldInfos)
          //      {
          //          FieldInfo derivedClassTypeFieldInfo = derivedClassType.GetField(baseClassTypeFieldInfo.Name);

          //          if (derivedClassTypeFieldInfo != null)
          //          {
          //              baseClassTypeFieldInfo.SetValue(
          //                  baseClassObject,
          //                  derivedClassTypeFieldInfo.GetValue(derivedClassObject));
          //          }
          //      }

          //      return baseClassObject;
          //  }

            // returns all instance FieldInfo[] of type. this does not include fields of base class,
            // in case type derives from another class.
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
