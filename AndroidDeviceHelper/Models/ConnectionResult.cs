using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class ConnectionResult
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public ConnectionState ConnectionState { get; set; }
    }
}
