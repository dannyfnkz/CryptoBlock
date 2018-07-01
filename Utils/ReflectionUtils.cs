
using System;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains methods which provide additional utility for <see cref="System.Reflection"/>.
        /// </summary>
        public static class ReflectionUtils
        {
            // gets property with specified name from obj
            // if property exists in obj returns property, else returns null
            /// <summary>
            /// returns value of property in <paramref name="obj"/>with <paramref name="propertyName"/>,
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
                //var site = 
                //    System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>.Create
                //    (Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                //        0,
                //        propertyName,
                //        obj.GetType(), 
                //        new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
                //return site.Target(site, obj);
                return obj.GetType().GetProperty(propertyName).GetValue(obj);
            }
        }
    }
}
