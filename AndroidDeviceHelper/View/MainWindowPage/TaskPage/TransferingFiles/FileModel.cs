using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace AndroidDeviceHelper.View.MainWindowPage.TaskPage.TransferingFiles
{
    public class FileModel
    {
        public DateTime Time { get; set; }
        public string FileExtension { get; set; } = "";
        public string FileSize { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileType { get; set; } = "";

    }
}
