using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace AndroidDeviceHelper.View.PreviewWindowPage
{
    /// <summary>
    /// Interaction logic for ImagePreviewer.xaml
    /// </summary>
    public partial class ImagePreviewer : UserControl, IPreviewingControl
    {
        public ImagePreviewer()
        {
            InitializeComponent();
        }
        private Stream Stream;
        public void SetStream(Stream stream)
        {
            Stream = stream;
            var source = BitmapFrame.Create(stream,
                                      BitmapCreateOptions.None,
                                      BitmapCacheOption.OnLoad);
            source.Freeze();
            imagePreview.Source = source;
        }
    }
}
