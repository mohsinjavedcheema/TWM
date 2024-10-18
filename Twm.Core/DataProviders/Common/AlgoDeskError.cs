using Newtonsoft.Json;

namespace Twm.Core.DataProviders.Common
{
    public class TwmError
    {
        public int Code { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        public string RequestMessage { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Message}";
        }
    }
}