using System.Windows;

namespace AndroidDeviceHelper.View.CustomizedModels
{
    public class MaterialIconSelection : DependencyObject
    {
        public static readonly DependencyProperty MaterialIconCodeProperty = DependencyProperty.Register("MaterialIconCode", typeof(string), typeof(MaterialIconSelection), new UIPropertyMetadata(""));
        public string MaterialIconCode
        {
            set => SetValue(MaterialIconCodeProperty, value);
            get => (string)GetValue(MaterialIconCodeProperty);
        }
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(MaterialIconSelection), new UIPropertyMetadata(""));
        public string Content
        {
            set => SetValue(ContentProperty, value);
            get => (string)GetValue(ContentProperty);
        }

    }
}
