using System;

namespace Twm.Core.Classes
{
    public class TempObjectType
    {
        public string DisplayName
        {
            get { return Type.Name; }

        }

        public Type Type { get; private set; }

        public TempObjectType(Type type)
        {
            Type = type;
        }
    }
}