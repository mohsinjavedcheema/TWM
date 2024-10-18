using Newtonsoft.Json;

namespace Twm.Core.Helpers
{
    public static class JsonHelper
    {
        public static T ToObject<T>(string json)
        {
            var jss = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            return JsonConvert.DeserializeObject<T>(json, jss);
        }


        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}