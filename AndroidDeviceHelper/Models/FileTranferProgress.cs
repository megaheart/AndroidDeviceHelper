using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.Models
{
    public class FileTranferProgress : NotifiableObject
    {
        public FileModel File { get; set; }
        public string SaveToFolder { get; set; }
        public bool IsPulling { get; set; }
        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private bool _isCompleted = false;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged("IsCompleted");
                }
            }
        }
        private bool _openAfterCompleted = false;
        public bool OpenAfterCompleted
        {
            get => _openAfterCompleted;
            set
            {
                if (_openAfterCompleted != value)
                {
                    _openAfterCompleted = value;
                    OnPropertyChanged("OpenAfterCompleted");
                }
            }
        }
        private bool _isError = false;
        public bool IsError
        {
            get => _isError;
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    OnPropertyChanged("IsError");
                }
            }
        }
        private string _error = "";
        public string Error
        {
            get => _error;
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged("Error");
                }
            }
        }
    }
}
