using System;
using System.Globalization;
using System.Windows.Controls;

namespace Twm.Core.Classes.Validators
{
    public class EmptyStringRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Should select the symbol");
            }
            var strValue = value.ToString();
            if (strValue != null)
            {
                if (strValue.Equals("Select Instrument", StringComparison.OrdinalIgnoreCase))
                {
                    strValue = "";
                }
            }
            return string.IsNullOrEmpty(strValue) ? new ValidationResult(false, "Should select the symbol") : ValidationResult.ValidResult;
        }
    }
}
