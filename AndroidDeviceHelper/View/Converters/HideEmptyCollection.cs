using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace AndroidDeviceHelper.View.Converters
{
    class HideEmptyCollection : IValueConverter
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
            if (((int)value) == 0) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
