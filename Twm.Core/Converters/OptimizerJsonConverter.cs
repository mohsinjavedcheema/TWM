using System;
using System.Windows.Media.Animation;
using Twm.Core.Classes;
using Twm.Core.DataCalc.Optimization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Twm.Core.Converters
{
    public class OptimizerJsonConverter : JsonConverter
    {
        static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new OptimizerContractResolver() };


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);
            var optimizerType = jo["TypeName"].Value<string>();

            Type type = Session.GetTypeByName("Twm.Custom.Optimizers." + optimizerType);
             var obj = JsonConvert.DeserializeObject(jo.ToString(),type, JsonSerializerSettings);

             return obj;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Optimizer);
        }
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
        }
    }

    public class OptimizerContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Optimizer).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; 
            // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }


}