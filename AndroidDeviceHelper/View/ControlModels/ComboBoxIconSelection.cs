
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AndroidDeviceHelper
{
    public abstract class ComboBoxIconSelection : DependencyObject
    {
        public abstract FontFamily IconsFont { get; }
        public static readonly DependencyProperty IconCodeProperty = DependencyProperty.Register("IconCode", typeof(string), typeof(ComboBoxIconSelection), new UIPropertyMetadata(""));
        public string IconCode
        {
            set => SetValue(IconCodeProperty, value);
            get => (string)GetValue(IconCodeProperty);
        }
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(ComboBoxIconSelection), new UIPropertyMetadata(""));
        public string Content
        {
            set => SetValue(ContentProperty, value);
            get => (string)GetValue(ContentProperty);
        }

    }
    public class MaterialIconsComboBoxSelection : ComboBoxIconSelection
    {
        private static FontFamily _IconsFont = (FontFamily) App.Current.Resources["MaterialIcons"];
        public override FontFamily IconsFont
        {
            get
            {
                //if(_IconsFont == null)
                return _IconsFont;
            }
        }
    }
}
