using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.View.PreviewWindowPage
{
    public interface IPreviewingControl
    {
        void SetStream(Stream stream);
    }
}
