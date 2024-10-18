using System;
using System.Linq;
using Twm.Core.DataProviders.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Twm.Core.DataProviders.Common
{
    public static class BaseEndpoints
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };



        public static string GenerateQueryStringFromData(IRequest request)
        {
            if (request == null)
            {
                throw new Exception("No request data provided - query string can't be created");
            }

            //TODO: Refactor to not require double JSON loop
            var obj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(request, Settings),
                Settings);

            return String.Join("&", obj.Children()
                .Cast<JProperty>()
                .Where(j => j.Value != null)
                .Select(j => j.Name + "=" + System.Net.WebUtility.UrlEncode(j.Value.ToString())));
        }
    }
}