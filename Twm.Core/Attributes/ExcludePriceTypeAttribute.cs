using System;
using Twm.Core.Enums;

namespace Twm.Core.Attributes
{
    public class ExcludePriceTypeAttribute : Attribute
    {
        public PriceType[] Exclude { get; private set; }
        public ExcludePriceTypeAttribute(params PriceType[] exclude)
        {
            Exclude = exclude;
        }
    }
}