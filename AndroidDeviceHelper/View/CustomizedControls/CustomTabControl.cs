using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace AndroidDeviceHelper.View.CustomizedControls
{
    public enum HeaderPosition
    {
        Start,
        End,
        Center,
        Stretch
    }
    [TemplatePart(Name = "PART_MainLine", Type = typeof(Line))]
    [TemplatePart(Name = "PART_SelectionLine", Type = typeof(Line))]
    public class CustomizedTabControl : TabControl
    {
        protected Line MainLine = null;
        protected Line SelectionLine = null;
        protected UniformGrid HeaderPanel = null;
        public static readonly DependencyProperty HeaderPositionProperty = DependencyProperty.Register("HeaderPosition", typeof(HeaderPosition), typeof(CustomizedTabControl), new UIPropertyMetadata(HeaderPosition.Start, HeaderPositionPropertyChanged));
        private static void HeaderPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomizedTabControl ctrl = d as CustomizedTabControl;
            if (ctrl.HeaderPanel == null) return;
            HeaderPosition position = (HeaderPosition)e.NewValue;
            switch (ctrl.TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    switch (position)
                    {
                        case HeaderPosition.Start:
                            ctrl.HeaderPanel.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case HeaderPosition.End:
                            ctrl.HeaderPanel.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        case HeaderPosition.Center:
                            ctrl.HeaderPanel.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case HeaderPosition.Stretch:
                            ctrl.HeaderPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                            break;
                    }
                    break;
                case Dock.Left:
                case Dock.Right:
                    switch (position)
                    {
                        case HeaderPosition.Start:
                            ctrl.HeaderPanel.VerticalAlignment = VerticalAlignment.Top;
                            break;
                        case HeaderPosition.End:
                            ctrl.HeaderPanel.VerticalAlignment = VerticalAlignment.Bottom;
                            break;
                        case HeaderPosition.Center:
                            ctrl.HeaderPanel.VerticalAlignment = VerticalAlignment.Center;
                            break;
                        case HeaderPosition.Stretch:
                            ctrl.HeaderPanel.VerticalAlignment = VerticalAlignment.Stretch;
                            break;
                    }
                    break;
            }
            if (ctrl.MainLine != null)
            {
                ctrl.ReloadMainLine();
            }
            if (ctrl.SelectionLine != null) ctrl.ReloadSelectionLine();

        }
        public HeaderPosition HeaderPosition
        {
            set => SetValue(HeaderPositionProperty, value);
            get => (HeaderPosition)GetValue(HeaderPositionProperty);
        }
        public static readonly DependencyProperty HeaderMarginProperty = DependencyProperty.Register("HeaderMargin", typeof(double), typeof(CustomizedTabControl), new UIPropertyMetadata(0.0, HeaderMarginPropertyChanged));
        private static void HeaderMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomizedTabControl ctrl = d as CustomizedTabControl;
            if (ctrl.HeaderPanel == null) return;
            var margin = ctrl.HeaderPadding + (double)e.NewValue;
            switch (ctrl.TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    ctrl.HeaderPanel.Margin = new Thickness(margin, 0, margin, 0);
                    break;
                case Dock.Left:
                case Dock.Right:
                    ctrl.HeaderPanel.Margin = new Thickness(0, margin, 0, margin);
                    break;
            }
            if (ctrl.MainLine != null)
            {
                ctrl.ReloadMainLine();
            }
            if (ctrl.SelectionLine != null) ctrl.ReloadSelectionLine();
        }
        public double HeaderMargin
        {
            set => SetValue(HeaderMarginProperty, value);
            get => (double)GetValue(HeaderMarginProperty);
        }
        public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.Register("HeaderPadding", typeof(double), typeof(CustomizedTabControl), new UIPropertyMetadata(0.0, HeaderPaddingPropertyChanged));
        private static void HeaderPaddingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomizedTabControl ctrl = d as CustomizedTabControl;
            if (ctrl.HeaderPanel == null) return;
            var margin = ctrl.HeaderMargin + (double)e.NewValue;
            switch (ctrl.TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    ctrl.HeaderPanel.Margin = new Thickness(margin, 0, margin, 0);
                    break;
                case Dock.Left:
                case Dock.Right:
                    ctrl.HeaderPanel.Margin = new Thickness(0, margin, 0, margin);
                    break;
            }
            if (ctrl.MainLine != null)
            {
                ctrl.ReloadMainLine();
            }
            if (ctrl.SelectionLine != null) ctrl.ReloadSelectionLine();
        }
        public double HeaderPadding
        {
            set => SetValue(HeaderPaddingProperty, value);
            get => (double)GetValue(HeaderPaddingProperty);
        }
        public static readonly DependencyProperty ThemeColorProperty = DependencyProperty.Register("ThemeColor", typeof(System.Windows.Media.Brush), typeof(CustomizedTabControl), new UIPropertyMetadata(new BrushConverter().ConvertFromString("#3f7dd9")));

        public Brush ThemeColor
        {
            set => SetValue(ThemeColorProperty, value);
            get => (Brush)GetValue(ThemeColorProperty);
        }
        //public static readonly DependencyProperty CanAnimateProperty = DependencyProperty.Register("CanAnimate", typeof(bool), typeof(CustomizedTabControl), new UIPropertyMetadata(false));

        //public bool CanAnimate
        //{
        //    set => SetValue(CanAnimateProperty, value);
        //    get => (bool)GetValue(CanAnimateProperty);
        //}
        public CustomizedTabControl() : base()
        {
            SelectionChanged += CustomizedTabControl_SelectionChanged;
            Loaded += CustomizedTabControl_Loaded;
            SizeChanged += CustomizedTabControl_SizeChanged;
            
        }

        private void CustomizedTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainLine != null)
            {
                ReloadMainLine();
            }
            if (SelectionLine != null) ReloadSelectionLine();
        }

        private void CustomizedTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainLine = GetTemplateChild("PART_MainLine") as Line;
            SelectionLine = GetTemplateChild("PART_SelectionLine") as Line;
            HeaderPanel = GetTemplateChild("HeaderPanel") as UniformGrid;
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(TabStripPlacementProperty, typeof(TabControl));
            descriptor.AddValueChanged(this, CustomizedTabControl_TabStripPlacementChanged);
            if (HeaderPanel == null) return;
            HeaderPosition position = HeaderPosition;
            switch (TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    switch (position)
                    {
                        case HeaderPosition.Start:
                            HeaderPanel.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case HeaderPosition.End:
                            HeaderPanel.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                        case HeaderPosition.Center:
                            HeaderPanel.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case HeaderPosition.Stretch:
                            HeaderPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                            break;
                    }
                    break;
                case Dock.Left:
                case Dock.Right:
                    switch (position)
                    {
                        case HeaderPosition.Start:
                            HeaderPanel.VerticalAlignment = VerticalAlignment.Top;
                            break;
                        case HeaderPosition.End:
                            HeaderPanel.VerticalAlignment = VerticalAlignment.Bottom;
                            break;
                        case HeaderPosition.Center:
                            HeaderPanel.VerticalAlignment = VerticalAlignment.Center;
                            break;
                        case HeaderPosition.Stretch:
                            HeaderPanel.VerticalAlignment = VerticalAlignment.Stretch;
                            break;
                    }
                    break;
            }
            var margin = HeaderMargin + HeaderPadding;
            switch (TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    HeaderPanel.Margin = new Thickness(margin, 0, margin, 0);
                    break;
                case Dock.Left:
                case Dock.Right:
                    HeaderPanel.Margin = new Thickness(0, margin, 0, margin);
                    break;
            }
            if (MainLine != null)
            {
                ReloadMainLine();
            }
            if (SelectionLine != null) ReloadSelectionLine();
            HeaderPanel.SizeChanged += CustomizedTabControl_SizeChanged;
            HeaderPanel.PreviewMouseLeftButtonDown += (sender1, e1) =>
            {
                DidUILoad = true;
                HeaderPanel_PreviewMouseLeftButtonDown?.Invoke();
            };
        }
        public event Action HeaderPanel_PreviewMouseLeftButtonDown;
        private bool DidUILoad = false;
        private void CustomizedTabControl_TabStripPlacementChanged(object sender, EventArgs e)
        {
            if (HeaderPanel == null) return;
            var margin = HeaderMargin + HeaderPadding;
            switch (TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    HeaderPanel.Margin = new Thickness(margin, 0, margin, 0);
                    break;
                case Dock.Left:
                case Dock.Right:
                    HeaderPanel.Margin = new Thickness(0, margin, 0, margin);
                    break;
            }
            if (MainLine != null)
            {
                ReloadMainLine();
            }
            if (SelectionLine != null) ReloadSelectionLine();
        }
        private static readonly TimeSpan duration = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan halfOfDuration = TimeSpan.FromMilliseconds(200);
        private void CustomizedTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionLine != null)
            {
                if (SelectedIndex < 0)
                {
                    MainLine.Y2 = MainLine.Y1;
                    MainLine.X2 = MainLine.X1;
                    return;
                }
                var selectedItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;
                var positionOfSelectedItem = selectedItem.TranslatePoint(new Point(0, 0), this);
                var positionOfSelectionLine = SelectionLine.TranslatePoint(new Point(0, 0), this);
                var vector = positionOfSelectedItem - positionOfSelectionLine;
                if (!DidUILoad)
                {
                    ReloadSelectionLine();
                    return;
                }
                Storyboard storyboard = new Storyboard();
                //switch (TabStripPlacement)
                //{
                //    case Dock.Top:
                //    case Dock.Bottom:
                //        {
                //            DoubleAnimation doubleAnimationX1 = new DoubleAnimation(vector.X, duration);
                //            Storyboard.SetTarget(doubleAnimationX1, SelectionLine);
                //            Storyboard.SetTargetProperty(doubleAnimationX1, new PropertyPath(Line.X1Property));
                //            DoubleAnimation doubleAnimationX2 = new DoubleAnimation(vector.X + selectedItem.ActualWidth, duration);
                //            Storyboard.SetTarget(doubleAnimationX2, SelectionLine);
                //            Storyboard.SetTargetProperty(doubleAnimationX2, new PropertyPath(Line.X2Property));
                //            storyboard.Children.Add(doubleAnimationX1);
                //            storyboard.Children.Add(doubleAnimationX2);
                //        }
                //        break;
                //    case Dock.Left:
                //    case Dock.Right:
                //        {
                //            DoubleAnimation doubleAnimationY1 = new DoubleAnimation(vector.Y, duration);
                //            Storyboard.SetTarget(doubleAnimationY1, SelectionLine);
                //            Storyboard.SetTargetProperty(doubleAnimationY1, new PropertyPath(Line.Y1Property));
                //            DoubleAnimation doubleAnimationY2 = new DoubleAnimation(vector.Y + selectedItem.ActualHeight, duration);
                //            Storyboard.SetTarget(doubleAnimationY2, SelectionLine);
                //            Storyboard.SetTargetProperty(doubleAnimationY2, new PropertyPath(Line.Y2Property));
                //            storyboard.Children.Add(doubleAnimationY1);
                //            storyboard.Children.Add(doubleAnimationY2);
                //        }
                //        break;
                //}
                switch (TabStripPlacement)
                {
                    case Dock.Top:
                    case Dock.Bottom:
                        {
                            if (vector.X > SelectionLine.X1)
                            {
                                DoubleAnimationUsingKeyFrames framesX1 = new DoubleAnimationUsingKeyFrames();
                                framesX1.Duration = duration;
                                framesX1.KeyFrames.Add(new LinearDoubleKeyFrame(SelectionLine.X1, halfOfDuration));
                                framesX1.KeyFrames.Add(new LinearDoubleKeyFrame(vector.X, duration));
                                Storyboard.SetTarget(framesX1, SelectionLine);
                                Storyboard.SetTargetProperty(framesX1, new PropertyPath(Line.X1Property));
                                DoubleAnimation doubleAnimationX2 = new DoubleAnimation(vector.X + selectedItem.ActualWidth, halfOfDuration);
                                Storyboard.SetTarget(doubleAnimationX2, SelectionLine);
                                Storyboard.SetTargetProperty(doubleAnimationX2, new PropertyPath(Line.X2Property));
                                storyboard.Children.Add(framesX1);
                                storyboard.Children.Add(doubleAnimationX2);
                            }
                            else
                            {
                                DoubleAnimation doubleAnimationX1 = new DoubleAnimation(vector.X, halfOfDuration);
                                Storyboard.SetTarget(doubleAnimationX1, SelectionLine);
                                Storyboard.SetTargetProperty(doubleAnimationX1, new PropertyPath(Line.X1Property));
                                DoubleAnimationUsingKeyFrames framesX2 = new DoubleAnimationUsingKeyFrames();
                                framesX2.Duration = duration;
                                framesX2.KeyFrames.Add(new LinearDoubleKeyFrame(SelectionLine.X2, halfOfDuration));
                                framesX2.KeyFrames.Add(new LinearDoubleKeyFrame(vector.X + selectedItem.ActualWidth, duration));
                                Storyboard.SetTarget(framesX2, SelectionLine);
                                Storyboard.SetTargetProperty(framesX2, new PropertyPath(Line.X2Property));
                                storyboard.Children.Add(doubleAnimationX1);
                                storyboard.Children.Add(framesX2);
                            }
                        }
                        break;
                    case Dock.Left:
                    case Dock.Right:
                        {
                            if (vector.Y > SelectionLine.Y1)
                            {
                                DoubleAnimationUsingKeyFrames framesY1 = new DoubleAnimationUsingKeyFrames();
                                framesY1.Duration = duration;
                                framesY1.KeyFrames.Add(new LinearDoubleKeyFrame(SelectionLine.Y1, halfOfDuration));
                                framesY1.KeyFrames.Add(new LinearDoubleKeyFrame(vector.Y, duration));
                                Storyboard.SetTarget(framesY1, SelectionLine);
                                Storyboard.SetTargetProperty(framesY1, new PropertyPath(Line.Y1Property));
                                DoubleAnimation doubleAnimationY2 = new DoubleAnimation(vector.Y + selectedItem.ActualHeight, halfOfDuration);
                                Storyboard.SetTarget(doubleAnimationY2, SelectionLine);
                                Storyboard.SetTargetProperty(doubleAnimationY2, new PropertyPath(Line.Y2Property));
                                storyboard.Children.Add(framesY1);
                                storyboard.Children.Add(doubleAnimationY2);
                            }
                            else
                            {
                                DoubleAnimation doubleAnimationY1 = new DoubleAnimation(vector.Y, halfOfDuration);
                                Storyboard.SetTarget(doubleAnimationY1, SelectionLine);
                                Storyboard.SetTargetProperty(doubleAnimationY1, new PropertyPath(Line.Y1Property));
                                DoubleAnimationUsingKeyFrames framesY2 = new DoubleAnimationUsingKeyFrames();
                                framesY2.Duration = duration;
                                framesY2.KeyFrames.Add(new LinearDoubleKeyFrame(SelectionLine.Y2, halfOfDuration));
                                framesY2.KeyFrames.Add(new LinearDoubleKeyFrame(vector.Y + selectedItem.ActualHeight, duration));
                                Storyboard.SetTarget(framesY2, SelectionLine);
                                Storyboard.SetTargetProperty(framesY2, new PropertyPath(Line.Y2Property));
                                storyboard.Children.Add(doubleAnimationY1);
                                storyboard.Children.Add(framesY2);
                            }
                        }
                        break;
                }
                storyboard.Begin(this);
            }
        }
        public Point GetPoint()
        {
            return HeaderPanel.TranslatePoint(new Point(0, 0), this);
        }
        private void ReloadMainLine()
        {
            Dispatcher.InvokeAsync(() =>
            {
                var positionOfHeaderPanel = HeaderPanel.TranslatePoint(new Point(0, 0), this);
                var positionOfMainLine = MainLine.TranslatePoint(new Point(0, 0), this);
                var vector = positionOfHeaderPanel - positionOfMainLine;
                switch (TabStripPlacement)
                {
                    case Dock.Top:
                        MainLine.Y1 = MainLine.Y2 = vector.Y + HeaderPanel.ActualHeight;
                        MainLine.X1 = vector.X - HeaderPadding;
                        MainLine.X2 = MainLine.X1 + HeaderPanel.ActualWidth + 2 * HeaderPadding;

                        break;
                    case Dock.Bottom:
                        MainLine.Y1 = MainLine.Y2 = vector.Y;
                        MainLine.X1 = vector.X - HeaderPadding;
                        MainLine.X2 = MainLine.X1 + HeaderPanel.ActualWidth + 2 * HeaderPadding;
                        break;
                    case Dock.Left:
                        MainLine.X1 = MainLine.X2 = vector.X + HeaderPanel.ActualWidth;
                        MainLine.Y1 = vector.Y - HeaderPadding;
                        MainLine.Y2 = MainLine.Y1 + HeaderPanel.ActualHeight + 2 * HeaderPadding;
                        break;
                    case Dock.Right:
                        MainLine.X1 = MainLine.X2 = vector.X;
                        MainLine.Y1 = vector.Y - HeaderPadding;
                        MainLine.Y2 = MainLine.Y1 + HeaderPanel.ActualHeight + 2 * HeaderPadding;
                        break;
                }
            });
        }
        private void ReloadSelectionLine()
        {
            if (SelectedIndex < 0)
            {
                MainLine.Y2 = MainLine.Y1;
                MainLine.X2 = MainLine.X1;
                return;
            }
            Dispatcher.InvokeAsync(() =>
            {
                var selectedItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;
                var positionOfSelectedItem = selectedItem.TranslatePoint(new Point(0, 0), this);
                var positionOfSelectionLine = SelectionLine.TranslatePoint(new Point(0, 0), this);
                var vector = positionOfSelectedItem - positionOfSelectionLine;
                switch (TabStripPlacement)
                {
                    case Dock.Top:
                        SelectionLine.Y1 = SelectionLine.Y2 = vector.Y + selectedItem.ActualHeight;
                        SelectionLine.X1 = vector.X;
                        SelectionLine.X2 = SelectionLine.X1 + selectedItem.ActualWidth;

                        break;
                    case Dock.Bottom:
                        SelectionLine.Y1 = SelectionLine.Y2 = vector.Y;
                        SelectionLine.X1 = vector.X;
                        SelectionLine.X2 = SelectionLine.X1 + selectedItem.ActualWidth;
                        break;
                    case Dock.Left:
                        SelectionLine.X1 = SelectionLine.X2 = vector.X + selectedItem.ActualWidth;
                        SelectionLine.Y1 = vector.Y;
                        SelectionLine.Y2 = SelectionLine.Y1 + selectedItem.ActualHeight;
                        break;
                    case Dock.Right:
                        SelectionLine.X1 = SelectionLine.X2 = vector.X;
                        SelectionLine.Y1 = vector.Y;
                        SelectionLine.Y2 = SelectionLine.Y1 + selectedItem.ActualHeight;
                        break;
                }
            });
        }
    }
}
