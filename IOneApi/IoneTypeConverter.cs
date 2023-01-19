using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi
{
    public static class IoneTypeConverter
    {
        public static decimal GetDecimal(this string textValue, string cultureName = "de-DE")
        {
            return Convert.ToDecimal(textValue, System.Globalization.CultureInfo.GetCultureInfo(cultureName));
        }

        public static string GetDecimalString(this decimal decimalValue, string cultureName = "de-DE")
        {
            return string.Format(System.Globalization.CultureInfo.GetCultureInfo(cultureName), "{0:0.00}", decimalValue);
        }

        public static DateTime GetDateTime(this string textValue, string cultureName = "de-DE")
        {
            return Convert.ToDateTime(textValue, System.Globalization.CultureInfo.GetCultureInfo(cultureName));
        }
    }
}
