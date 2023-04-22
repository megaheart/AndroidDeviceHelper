using AndroidDeviceHelper.Models;
using AndroidDeviceHelper.View.TasksPage.TaskPage.TransferingFiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    /// <summary>
    /// Interaction logic for TransferingFilesPanel.xaml
    /// </summary>
    public partial class TransferingFilesPanel : UserControl, IConnectionStateDependency
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        private ObservableCollection<FileModel> _files = new ObservableCollection<FileModel>();
        public TransferingFilesPanel()
        {
            InitializeComponent();
            ErrorMsgViewer.ItemsSource = _errors;
            _FileViewer.Files = _files;



        }
        private List<FileModel> DeserializeFileModel(string s, out string totalSize)
        {
            List<FileModel> result = new List<FileModel>();
            string[] lines = s.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            while (i < lines.Length && lines[i].IndexOf("total") == -1)
            {
                i++;
            }
            totalSize = lines[i].Substring(6);
            Console.WriteLine(totalSize);
            i++;
            while (i < lines.Length)
            {
                FileModel model = new FileModel();
                StringBuilder sb = new StringBuilder();
                int j = 0;
                var line = lines[i];
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ')
                {
                    sb.Append(line[j]);
                    j++;
                }
                model.FileSize = sb.ToString();
                sb.Clear();
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                while (j < line.Length && line[j] != ' ') j++;
                while (j < line.Length && line[j] == ' ') j++;
                DateTime time;
                if (line[j] == '?')
                {
                    time = DateTime.MinValue;
                    j++;
                }
                else
                {
                    time = DateTime.Parse(line.Substring(j, 35)).ToLocalTime();
                    j += 35;
                }

                model.Time = time;

                while (j < line.Length && line[j] == ' ') j++;
                model.FileType = line.Last().ToString();
                if (model.FileType != "@")
                {
                    while (j < line.Length - 1)
                    {
                        if (line[j] == '\\')
                        {
                            sb.Append(line[++j]);
                        }
                        else sb.Append(line[j]);
                        j++;
                    }
                }
                else
                {
                    while (j < line.Length - 1 && line[j] != ' ')
                    {
                        if (line[j] == '\\')
                        {
                            sb.Append(line[++j]);
                        }
                        else sb.Append(line[j]);
                        j++;
                    }
                }
                model.FileName = sb.ToString();
                sb.Clear();
                model.FileExtension = model.FileName.Substring(model.FileName.LastIndexOf(".") + 1);
                result.Add(model);
                i++;
            }
            return result;
        }
        //public static readonly DependencyProperty ConnectionStateProperty = DependencyProperty.Register("ConnectionState", typeof(ConnectionState), typeof(TransferingFilesPanel), new UIPropertyMetadata(null));
        //public ConnectionState ConnectionState
        //{
        //    set => SetValue(ConnectionStateProperty, value);
        //    get => (ConnectionState)GetValue(ConnectionStateProperty);
        //}
        public ConnectionState ConnectionState { get; set; }
        public void ResetPage()
        {

        }
        private void ReloadFileList(object sender, RoutedEventArgs e)
        {

        }
        private void FileViewer_FileOpened(object sender, TransferingFiles.FileOpenedEventArgs e)
        {
            MessageBox.Show(e.FileModel.FileName);
            //if (!PreviewWindow.Preview(e.FileModel, null))
            //{

            //}
        }

        private void OpenFilesTranferPanel(object sender, RoutedEventArgs e)
        {
            FilesTranferPanel.Visibility = Visibility.Visible;
        }
    }
}
