using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Twm.Core.Attributes;
using Twm.Core.Enums;

namespace Twm.Core.CustomProperties.Converters
{
    public class ExcludePriceTypeConverter : EnumConverter
    {
        public ExcludePriceTypeConverter() : base(typeof(PriceType))
        {
        }
        public override StandardValuesCollection GetStandardValues(
            ITypeDescriptorContext context)
        {
            var original = base.GetStandardValues(context);
            var exclude = context.PropertyDescriptor.Attributes
                              .OfType<ExcludePriceTypeAttribute>().FirstOrDefault()?.Exclude
                          ?? new PriceType[0];
            var excluded = new StandardValuesCollection(
                original.Cast<PriceType>().Except(exclude).ToList());
            Values = excluded;
            return excluded;
        }
    }
}