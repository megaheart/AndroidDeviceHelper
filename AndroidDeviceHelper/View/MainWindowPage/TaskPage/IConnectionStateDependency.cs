using AndroidDeviceHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AndroidDeviceHelper.View.TasksPage.TaskPage
{
    public interface IConnectionStateDependency
    {
        ConnectionState ConnectionState { set; }
    }
}
