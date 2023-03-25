using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace AndroidDeviceHelper.View.Converters
{
    public class CurrencyToString : IValueConverter
    {
        /// <summary>
        /// convert from integer (currency value) to string (integer with dot)
        /// unsupport floating-point number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder newValue = new StringBuilder(value.ToString());
            for (int i = newValue.Length - 3; i > 0; i -= 3)
            {
                newValue.Insert(i, '.');
            }
            newValue.Append(".000 VND");
            return newValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
