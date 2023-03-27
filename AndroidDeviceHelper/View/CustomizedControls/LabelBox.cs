using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace AndroidDeviceHelper.View.CustomizedControls
{
    public enum LabelPosition
    {
        //
        // Summary:
        //     The label is positioned on the left side of the LabelBox.
        Left = 0,
        //
        // Summary:
        //     The label is positioned at the top of the LabelBox.
        Top = 1,
        //
        // Summary:
        //     The label is positioned on the right side of the LabelBox.
        Right = 2
    }
    [TemplatePart(Name = "PART_label", Type = typeof(Line))]
    public class LabelBox : ContentControl
    {
        protected Button _labelBtn = null;
        public LabelBox():base()
        {
            this.Style = App.Current.Resources["LabelBox_DefaultStyle"] as Style;
        }
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(LabelBox), new PropertyMetadata(new CornerRadius(2)/*, CornerRadiusPropertyChanged*/));

        public CornerRadius CornerRadius
        {
            set => SetValue(CornerRadiusProperty, value);
            get => (CornerRadius)GetValue(CornerRadiusProperty);
        }
        public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register("LabelPosition", typeof(LabelPosition), typeof(LabelBox), new PropertyMetadata(LabelPosition.Left, LabelPositionPropertyChanged));
        public static void LabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabelBox l = d as LabelBox;
            DockPanel.SetDock(l._labelBtn, (Dock)e.NewValue);
        }
        public LabelPosition LabelPosition
        {
            set => SetValue(LabelPositionProperty, value);
            get => (LabelPosition)GetValue(LabelPositionProperty);
        }
        private object? _labelContent = null;
        public object LabelContent
        {
            set
            {
                if (_labelContent == value) return;
                _labelContent = value;
                ApplyLabelContent(value);


            }
            get => _labelContent;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _labelBtn = GetTemplateChild("PART_label") as Button;
            if (_labelBtn is null) throw new Exception("Template of LabelBox must have 'PART_label' Button");
            _labelBtn.Focusable = false;
            _labelBtn.Click += Label_Clicked;
            ApplyLabelContent(_labelContent);
        }
        private void ApplyLabelContent(object value)
        {
            if (_labelBtn == null) return;
            if (value is string)
            {
                TextBlock text;
                if (_labelBtn.Content is TextBlock)
                {
                    text = _labelBtn.Content as TextBlock;
                    text.Text = value as string;
                }
                else
                {
                    text = new TextBlock() { Text = value as string, Margin = new Thickness(20, 3, 15, 3), VerticalAlignment = VerticalAlignment.Center };
                    _labelBtn.Content = text;
                }
            }
            else
                _labelBtn.Content = value;
        }
        private void Label_Clicked(object sender, RoutedEventArgs e)
        {
            var c = Content as UIElement;
            if (c != null)
            {
                if (!c.IsKeyboardFocused) Keyboard.Focus(c);
            }
        }
    }
}
