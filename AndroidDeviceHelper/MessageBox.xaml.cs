using AndroidDeviceHelper.Native;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AndroidDeviceHelper
{
    public enum MessageBoxButton
    {
        Cancel,
        OkCancel,
        RetryCancel,
        Ok
    }
    public enum MessageBoxIcon
    {
        Message,
        Info,
        Warning,
        Error
    }
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            WindowResizer = new WindowResizer(this);
        }
        private bool accept = false;
        public static void Show(string message)
        {
            var box = new MessageBox();

            box.AcceptBtn.Visibility = Visibility.Collapsed;

            var bitmap = new BitmapImage(new Uri(@"pack://application:,,,/Resources/App Icons/MessageBox/speech-bubble.png"));
            bitmap.Freeze();
            box.MsgBoxIcon.Source = bitmap;

            box.MsgBoxTitle.Text = "Message";
            box.MsgBoxMessage.Text = message;
            box.ShowDialog();
        }

        public static bool Show(string title, string message, MessageBoxButton button = MessageBoxButton.Cancel, MessageBoxIcon icon = MessageBoxIcon.Message)
        {
            var box = new MessageBox();

            box.MsgBoxTitle.Text = title;
            box.MsgBoxMessage.Text = message;

            switch (button)
            {
                case MessageBoxButton.Cancel:
                    box.AcceptBtn.Visibility = Visibility.Collapsed;
                    break;

                case MessageBoxButton.RetryCancel:
                    box.AcceptBtn_Icon.Text = "replay";
                    box.AcceptBtn_Content.Text = "Retry";
                    box.AcceptBtn.Background = box.FindResource("Blue") as SolidColorBrush;
                    break;

                case MessageBoxButton.Ok:
                    box.DenyBtn.Visibility = Visibility.Collapsed;
                    box.AcceptBtn.Margin = new Thickness(0, 5, 0, 5);
                    break;
            }


            BitmapImage bitmap;
            switch (icon)
            {
                case MessageBoxIcon.Info:
                    bitmap = new BitmapImage(new Uri(@"pack://application:,,,/Resources/App Icons/MessageBox/box-important.png"));
                    break;
                case MessageBoxIcon.Warning:
                    bitmap = new BitmapImage(new Uri(@"pack://application:,,,/Resources/App Icons/MessageBox/medium-risk.png"));
                    break;
                case MessageBoxIcon.Error:
                    bitmap = new BitmapImage(new Uri(@"pack://application:,,,/Resources/App Icons/MessageBox/cancel.png"));
                    break;
                default:
                    bitmap = new BitmapImage(new Uri(@"pack://application:,,,/Resources/App Icons/MessageBox/speech-bubble.png"));
                    break;
            }
            bitmap.Freeze();
            box.MsgBoxIcon.Source = bitmap;

            box.ShowDialog();

            return box.accept;
        }

        #region Window resizer
        WindowResizer WindowResizer;
        // for each rectangle, assign the following method to its PreviewMouseDown event.
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            e.Handled = true;
        }
        private void Resize(object sender, MouseButtonEventArgs e)
        {
            WindowResizer.resizeWindow((System.Windows.Controls.Primitives.Thumb)sender);
            e.Handled = true;
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            accept = true;
            Close();
        }

        private void DenyBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
