using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AndroidDeviceHelper.View.Converters
{
    public class InverseVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                switch (visibility)
                {
                    case Visibility.Visible:
                        return Visibility.Collapsed;
                    case Visibility.Collapsed:
                        return Visibility.Visible;
                    case Visibility.Hidden:
                        return Visibility.Visible;
                    default:
                        return Visibility.Collapsed;
                }
            }
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
