using AndroidDeviceHelper.Models;
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

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage.TransferingFiles
{
    public class FileOpenedEventArgs:RoutedEventArgs
    {
        public FileOpenedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        //public string FileName { get; set; }
        public FileModel FileModel { get; set; }
    }
    public delegate void FileOpenedEventHandler(object sender, FileOpenedEventArgs e);
    /// <summary>
    /// Interaction logic for FileViewer.xaml
    /// </summary>
    public partial class FileViewer : UserControl
    {
        public FileViewer()
        {
            InitializeComponent();
            
        }
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof(IEnumerable<FileModel>), typeof(FileViewer), new UIPropertyMetadata(null, FilesPropertyChangedCallback));
        private static void FilesPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileViewer fileViewer = d as FileViewer;
            if (fileViewer != null)
            {
                //if (e.NewValue != null)
                //{
                //    fileViewer.FileListViewer.ItemsSource = e.NewValue as IEnumerable<FileModel>;
                //}
                fileViewer.FileListViewer.ItemsSource = e.NewValue as IEnumerable<FileModel>;
            }
        }
        public IEnumerable<FileModel> Files
        {
            set => SetValue(FilesProperty, value);
            get => (IEnumerable<FileModel>)GetValue(FilesProperty);
        }
        public IEnumerable<FileModel> SelectedFiles
        {
            get => FileListViewer.SelectedItems.Cast<FileModel>();
        }
        public static readonly RoutedEvent FileOpenedEvent
            = EventManager.RegisterRoutedEvent("FileOpened", RoutingStrategy.Bubble, typeof(FileOpenedEventHandler), typeof(FileViewer));
        public event FileOpenedEventHandler FileOpened
        {
            add => AddHandler(FileOpenedEvent, value);
            remove => RemoveHandler(FileOpenedEvent, value);
        }
        private void RaiseFileOpenedEvent(FileModel fileModel)
        {
            // Create a RoutedEventArgs instance.
            FileOpenedEventArgs routedEventArgs = new(routedEvent: FileOpenedEvent) { 
                FileModel = fileModel
            };

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }
        //private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if(e.ClickCount == 2)
        //    {
        //        var y = sender;

        //    }
        //}

        private void FileListViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileListViewer.SelectedItems.Count != 1) return;
            var fileModel = FileListViewer.SelectedValue as FileModel;
            //var fileName = fileModel.FileName;
            RaiseFileOpenedEvent(fileModel);
        }
    }
}
