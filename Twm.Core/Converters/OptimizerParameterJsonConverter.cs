using System;
using Twm.Core.DataCalc.Optimization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Twm.Core.Converters
{
    public class OptimizerParameterJsonConverter : JsonConverter
    {
        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new OptimizerParameterContractResolver() };


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            switch (jo["TypeNum"].Value<int>())
            {
                case 1:
                    return JsonConvert.DeserializeObject<DoubleOptimizerParameter>(jo.ToString(), JsonSerializerSettings);
                case 2:
                    return JsonConvert.DeserializeObject<IntegerOptimizerParameter>(jo.ToString(), JsonSerializerSettings);
                case 3:
                    return JsonConvert.DeserializeObject<EnumOptimizerParameter>(jo.ToString(), JsonSerializerSettings);
                case 4:
                    return JsonConvert.DeserializeObject<BoolOptimizerParameter>(jo.ToString(), JsonSerializerSettings);
                default:
                    throw new Exception();
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(OptimizerParameter));
        }
        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
    }

    public class OptimizerParameterContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(OptimizerParameter).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
}