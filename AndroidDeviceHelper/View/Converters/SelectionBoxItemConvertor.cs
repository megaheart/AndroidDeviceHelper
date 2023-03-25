//using System;
//using System.Globalization;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Markup;

//namespace AndroidDeviceHelper.View.Converters
//{
//    public class SelectionBoxItemConvertor : IValueConverter
//    {
//        ContentPresenter p = new ContentPresenter() { VerticalAlignment = VerticalAlignment.Center };
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value == null) return null;
//            string xaml = XamlWriter.Save(value);
//            var x = XamlReader.Parse(xaml);
//            p.Content = x;
//            p.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
//            p.MinHeight = p.DesiredSize.Height;
//            p.MinWidth = p.DesiredSize.Width;
//            Console.WriteLine(p.DesiredSize);
//            return x;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
