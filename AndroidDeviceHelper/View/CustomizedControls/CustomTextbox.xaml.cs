using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AndroidDeviceHelper.View.CustomizedControls
{
    /// <summary>
    /// Interaction logic for CustomTextbox.xaml
    /// </summary>
    public partial class CustomTextbox : UserControl
    {
        public CustomTextbox()
        {
            InitializeComponent();
        }
        public string Text
        {
            get => MainText.Text;
            set
            {
                if (value != MainText.Text)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        if (!MainText.IsFocused)
                        {
                            PlaceHolderTxt.Visibility = Visibility.Visible;
                        }
                        MainText.Text = value;
                    }
                    else
                    {
                        PlaceHolderTxt.Visibility = Visibility.Collapsed;
                        MainText.Text = value;
                    }
                }
            }
        }
        //public bool ShortcutsEnabled
        //{
        //    set
        //    {
        //        if (!value)
        //        {
        //            CommandManager.AddPreviewExecutedHandler(MainText, new ExecutedRoutedEventHandler(TextBox_PreviewExecuted));
        //            MainText.ContextMenu = null;
        //        }
        //    }
        //}
        //private void TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste || e.Command == ApplicationCommands.SelectAll)
        //    {
        //        e.Handled = true;
        //    }
        //}
        public event TextChangedEventHandler TextChanged
        {
            add
            {
                MainText.TextChanged += value;
            }
            remove
            {
                MainText.TextChanged -= value;
            }
        }
        public string PlaceHolder
        {
            set
            {
                if (value != PlaceHolderTxt.Content.ToString())
                {
                    PlaceHolderTxt.Content = value;
                }
            }
            get => PlaceHolderTxt.Content.ToString();
        }
        public Thickness TextboxPadding
        {
            set
            {
                MainText.Padding = value;
            }
            get => MainText.Padding;
        }
        public TextAlignment TextAlignment
        {
            set
            {
                MainText.TextAlignment = value;
            }
        }
        //public Thickness borderThickness
        //{
        //    set
        //    {
        //        border.BorderThickness = value;
        //    }
        //    get => border.BorderThickness;
        //}
        //public Brush borderBrush
        //{
        //    set
        //    {
        //        border.BorderBrush = value;
        //    }
        //    get => border.BorderBrush;
        //}
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlaceHolderTxt.Visibility = Visibility.Collapsed;
            MainText.Focus();
        }

        private void MainText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainText.Text)) PlaceHolderTxt.Visibility = Visibility.Visible;
        }
    }
}
