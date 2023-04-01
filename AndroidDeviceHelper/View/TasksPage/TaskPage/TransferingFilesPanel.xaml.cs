using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    /// <summary>
    /// Interaction logic for TransferingFilesPanel.xaml
    /// </summary>
    public partial class TransferingFilesPanel : UserControl
    {
        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        public TransferingFilesPanel()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty DeviceProfileProperty = DependencyProperty.Register("DeviceProfile", typeof(DeviceProfile), typeof(TransferingFilesPanel), new UIPropertyMetadata(null));
        public DeviceProfile DeviceProfile
        {
            set => SetValue(DeviceProfileProperty, value);
            get => (DeviceProfile)GetValue(DeviceProfileProperty);
        }
    }
}
