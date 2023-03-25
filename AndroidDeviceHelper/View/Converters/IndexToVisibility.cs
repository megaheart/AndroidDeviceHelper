using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AndroidDeviceHelper.View.Converters
{

    class IndexToVisibility : IValueConverter
    {
        /// <summary>
        /// Convert index to enum visibility
        /// </summary>
        /// <param name="value">Index of combobox or selector, etc... which has selectedIndex property</param>
        /// <param name="parameter">Index of element to display it</param>
        /// <returns>
        /// if value = index -> visible, else -> collapsed
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((int)value) == int.Parse(parameter.ToString())) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
