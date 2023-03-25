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
    /// Interaction logic for NumbericBox.xaml
    /// </summary>
    public partial class NumbericTextBox : UserControl
    {
        public NumbericTextBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumbericTextBox), new UIPropertyMetadata(23, MaxValuePropertyChanged));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumbericTextBox), new UIPropertyMetadata(0, MinValuePropertyChanged));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumbericTextBox), new UIPropertyMetadata(0, ValuePropertyChanged));
        //public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(NumbericTextBox));
        
        public static bool CannotReload = false;//If this feature is not available in multithread,  
                                                // change to DependencyProperty
        private static void MaxValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            NumbericTextBox n = d as NumbericTextBox;
            CannotReload = true;
            n.MinValue = Math.Min(n.MinValue, (int)e.NewValue);
            if (n.Value > value) n.SetValue(ValueProperty, value);
            CannotReload = false;
            n.ReloadUI();
        }
        public int MaxValue
        {
            set => SetValue(MaxValueProperty, value);
            get => (int)GetValue(MaxValueProperty);
        }
        private static void MinValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = (int)e.NewValue;
            NumbericTextBox n = d as NumbericTextBox;
            CannotReload = true;
            n.MaxValue = Math.Max(n.MaxValue, (int)e.NewValue);
            if (n.Value < value) n.SetValue(ValueProperty, value);
            CannotReload = false;
            n.ReloadUI();
        }
        public int MinValue
        {
            set => SetValue(MinValueProperty, value);
            get => (int)GetValue(MinValueProperty);
        }
        private void ReloadUI()
        {
            if (CannotReload) return;
            int value = Value;
            PreviousBtn2.Content = SafeAdd(MinValue, MaxValue, value, -2).ToString();
            PreviousBtn1.Content = SafeAdd(MinValue, MaxValue, value, -1).ToString();
            MainTextBox.Text = value.ToString();
            NextBtn1.Content = SafeAdd(MinValue, MaxValue, value, 1).ToString();
            NextBtn2.Content = SafeAdd(MinValue, MaxValue, value, 2).ToString();
        }
        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumbericTextBox n = d as NumbericTextBox;
            n.ReloadUI();
            n.RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
        public int Value
        {
            set
            {
                if (value > MaxValue || value < MinValue) throw new Exception("Can't set value property out of min or max value");
                SetValue(ValueProperty, value);
            }
            get
            {
                return (int)GetValue(ValueProperty);
            }
        }
        private static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumbericTextBox));
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        public Brush ThemeColor
        {
            set => MainTextBox.Background = value;
        }
        public void IncreaseOne(object sender, RoutedEventArgs e)
        {
            Value = SafeAdd(MinValue, MaxValue, Value, 1);
        }
        public void IncreaseTwo(object sender, RoutedEventArgs e)
        {
            Value = SafeAdd(MinValue, MaxValue, Value, 2);
        }
        public void DecreaseOne(object sender, RoutedEventArgs e)
        {
            Value = SafeAdd(MinValue, MaxValue, Value, -1);
        }
        public void DecreaseTwo(object sender, RoutedEventArgs e)
        {
            Value = SafeAdd(MinValue, MaxValue, Value, -2);
        }
        public static int SafeAdd(int min, int max, int value, int i)
        {
            int length = max - min + 1;
            return ((value - min + i) % length + length) % length + min;
        }
        bool loop = false;
        private async void IncreaseContinuity_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loop = true;
            Value = SafeAdd(MinValue, MaxValue, Value, 1);
            await Task.Delay(140);
            while (loop)
            {
                await Task.Delay(20);
                Value = SafeAdd(MinValue, MaxValue, Value, 1);
            }
        }

        private void IncreaseContinuity_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            loop = false;
        }

        private async void DecreaseContinuity_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loop = true;
            Value = SafeAdd(MinValue, MaxValue, Value, -1);
            await Task.Delay(140);
            while (loop)
            {
                await Task.Delay(20);
                Value = SafeAdd(MinValue, MaxValue, Value, -1);
            }
        }

        private void DecreaseContinuity_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            loop = false;
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Value = SafeAdd(MinValue, MaxValue, Value, 1);
            else Value = SafeAdd(MinValue, MaxValue, Value, -1);
        }
    }
}
