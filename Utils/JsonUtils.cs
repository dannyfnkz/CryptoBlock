using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CryptoBlock
{
    namespace Utils
    {
        /// <summary>
        /// contains utility methods for handling JSON.
        /// </summary>
        public static class JsonUtils
        {
            /// <summary>
            /// thrown if an error occurs while handling a JSON operation.
            /// </summary>
            public class JsonException : Exception
            {
                public JsonException(string message, Exception innerException)
                    : base(message, innerException)
                {

                }

                public JsonException(string message)
                    : base(message)
                {

                }
            }

            /// <summary>
            /// thrown if an exception occurs while trying to deserialize a JSON string.
            /// </summary>
            public class JsonSerializationException : JsonException
            {
                public JsonSerializationException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "An exception occurred while trying to serialize JSON object.";
                }
            }

            /// <summary>
            /// thrown if an exception occurs while trying to deserialize a JSON string.
            /// </summary>
            public class JsonDeserializationException : JsonException
            {
                public JsonDeserializationException(Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {

                }

                private static string formatExceptionMessage()
                {
                    return "An exception occurred while trying to deserialize JSON object.";
                }
            }

            /// <summary>
            /// thrown if a requested JSON property does not exist in specified JToken, or has a different
            /// type than expected.
            /// </summary>
            public class JsonPropertyParseException : JsonException
            {
                public JsonPropertyParseException(string propertyName)
                    : base(formatExceptionMessage(propertyName))
                {

                }

                public JsonPropertyParseException(string propertyName, Exception innerException)
                    : base(formatExceptionMessage(propertyName), innerException)
                {

                }

                private static string formatExceptionMessage(string propertyName)
                {
                    return string.Format(
                        "JSON Property does not exist in JToken or had an unexpected type: {0}.",
                        propertyName);
                }
            }

            /// <summary>
            /// serializess <paramref name="obj"/> into a JSON string.
            /// </summary>
            /// <seealso cref="JsonConvert.SerializeObject(object)"/>
            /// <param name="obj"></param>
            /// <returns>
            /// JSON string serialization of <paramref name="obj"/>
            /// </returns>
            /// <exception cref="JsonSerializationException">
            /// thrown if serialization was unsuccessful
            /// </exception>
            public static string SerializeObject(object obj)
            {
                try
                {
                    // try serializing obj into a json string
                    string jsonString = JsonConvert.SerializeObject(obj);

                    return jsonString;
                }
                catch(JsonException jsonException) // serialization unsuccessful
                {
                    throw new JsonSerializationException(jsonException);
                }
            }

            /// <summary>
            /// deserializes <paramref name="jsonObjectString"/> into an object of type <typeparamref name="T"/>,
            /// and returns said object. 
            /// </summary>
            /// <seealso cref="JsonConvert.DeserializeObject{T}(string)"/>
            /// <typeparam name="T"></typeparam>
            /// <param name="jsonObjectString">
            /// a string in JSON format representing an object of type <typeparamref name="T"/>.
            /// </param>
            /// <returns>
            /// object of type <typeparamref name="T"/> deserialized from <paramref name="jsonObjectString"/>
            /// </returns>
            /// <exception cref="JsonDeserializationException">
            /// thrown if deserialization was unsuccessful
            /// </exception>
            public static T DeserializeObject<T>(string jsonObjectString)
            {
                try
                {
                    // try deserializing jsonObjectString into an object of type T
                    T deserializeObject = JsonConvert.DeserializeObject<T>(jsonObjectString);

                    return deserializeObject;
                }
                catch(JsonException jsonException) // deserialization unsuccessful
                {
                    throw new JsonDeserializationException(jsonException);
                }
            }

            /// <summary>
            /// returns value of <paramref name="propertyName"/> in <paramref name="jToken"/>.
            /// <c>default(T)</c> is returned if value is null.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="jToken"></param>
            /// <param name="propertyName"></param>
            /// <returns>
            /// value of <paramref name="propertyName"/> in <paramref name="jToken"/>,
            /// <c>default(T)</c> if value is null
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="assertJTokenNotNull(JToken)"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// thrown if <paramref name="propertyName"/> does not exist in <paramref name="jToken"/>,
            /// or <see cref="System.Type"/> of value of <paramref name="propertyName"/>
            /// does not match <typeparamref name="T"/>.
            /// </exception>
            public static T GetPropertyValue<T>(JToken jToken, string propertyName)
            {
                assertJTokenNotNull(jToken);

                try
                {
                    // propertyName field exists in jToken, but its value is null
                    if (IsPropertyNull(jToken, propertyName))
                    {
                        return default(T);
                    }

                    T propertyValue = jToken.Value<T>(propertyName);

                    return propertyValue;
                }
                catch (Exception exception)
                {
                    // type of T is wrong (e.g jToken[propertyName] is string but T was int)
                    if (exception is FormatException || exception is InvalidCastException
                        || exception is NullReferenceException) // propertyName does not exist in jToken
                    {
                        throw new JsonPropertyParseException(propertyName, exception);
                    }
                    else
                    {
                        throw exception;
                    }      
                }
            }

            /// <summary>
            /// returns whether all <paramref name="properties"/> exist in <paramref name="jToken"/>.
            /// </summary>
            /// <param name="jToken"></param>
            /// <param name="properties"></param>
            /// <returns>
            /// true if all <paramref name="properties"/> exist in <paramref name="jToken"/>,
            /// else false
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="assertJTokenNotNull(JToken)"/>
            /// </exception>
            public static bool CheckExist(JToken jToken, params object[] properties)
            {
                assertJTokenNotNull(jToken);

                foreach (object property in properties)
                {
                    if (jToken[property] == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// asserts that all <paramref name="properties"/> exist in <paramref name="jToken"/>.
            /// </summary>
            /// <param name="jToken"></param>
            /// <param name="properties"></param>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="assertJTokenNotNull(JToken)"/>
            /// </exception>
            /// <exception cref="JsonPropertyParseException">
            /// thrown if one or more <paramref name="properties"/> do not exist in <paramref name="jToken"/>
            /// </exception>
            public static void AssertExist(JToken jToken, params object[] properties)
            {
                assertJTokenNotNull(jToken);

                foreach (object property in properties)
                {
                    if (jToken[property] == null)
                    {
                        throw new JsonPropertyParseException(property.ToString());
                    }
                }
            }

            /// <summary>
            /// returns whether <paramref name="propertyName"/> in <paramref name="jToken"/> is null.
            /// </summary>
            /// <param name="jToken"></param>
            /// <param name="propertyName"></param>
            /// <returns>
            /// true if <paramref name="propertyName"/> in <paramref name="jToken"/> is null.,
            /// else false
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="assertJTokenNotNull(JToken)"/>
            /// </exception>
            public static bool IsPropertyNull(JToken jToken, string propertyName)
            {
                assertJTokenNotNull(jToken);

                return jToken[propertyName].Type == JTokenType.Null;
            }

            /// <summary>
            /// asserts that <paramref name="jToken"/> is not null.
            /// </summary>
            /// <param name="jToken"></param>
            /// <exception cref="ArgumentNullException">thrown if <paramref name="jToken"/> is null.</exception>
            private static void assertJTokenNotNull(JToken jToken)
            {
                if(jToken == null)
                {
                    throw new ArgumentNullException("jToken");
                }
            }
        }
    }
}

