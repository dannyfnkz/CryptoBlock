using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils
    {
        public static class JsonUtils
        {
            // only for reference
            // returns false both in cases in which JToken does not exist (i.e null is passed as [jToken])
            // and in cases where JToken exists (not null) but its value is null.
            //public static bool IsNullOrEmpty(JToken jToken)
            //{
            //    return (jToken == null) ||
            //           (jToken.Type == JTokenType.Array && !jToken.HasValues) ||
            //           (jToken.Type == JTokenType.Object && !jToken.HasValues) ||
            //           (jToken.Type == JTokenType.String && jToken.ToString() == String.Empty) ||
            //           (jToken.Type == JTokenType.Null);
            //}
        }
    }
}

