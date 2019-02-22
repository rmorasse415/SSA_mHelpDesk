using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SSA_mHelpDesk.Domain
{
    public class NumericValidationRule : NotEmptyValidationRule
    {
        private int? _min = null;
        private int? _max = null;
        private bool _required = true;

        public int? Min { get => _min; set => _min = value; }
        public int? Max { get => _max; set => _max = value; }
        public bool Required { get => _required; set => _required = value; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Required)
            {
                ValidationResult baseResult = base.Validate(value, cultureInfo);
                if (!baseResult.IsValid)
                    return baseResult;
            }

            int number;
            try
            {
                number = Int32.Parse((String)value);
            }
            catch(Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if (Min.HasValue && (number < Min) || 
                Max.HasValue && (number > Max))
            {
                string msg = "Please enter a number ";

                if (Min.HasValue && Max.HasValue)
                    msg += "in the range [" + Min + "," + Max + "]"; // inclusive
                else if (Min.HasValue)
                    msg += ">= " + Min;
                else // Max must have value
                    msg += "<= " + Max;

                return new ValidationResult(false, msg);
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
