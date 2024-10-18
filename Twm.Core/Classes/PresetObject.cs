using System.Runtime.Serialization;
using Twm.Core.Enums;

namespace Twm.Core.Classes
{
    [DataContract]
    public class PresetObject<T>
    {
        [DataMember]
        public PresetType PresetType { get; set; }

        [DataMember]
        public T Object { get; set; }
    }
}