using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Component;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.CoreViewModel
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        string _fileName;
        LibraryLoadType _loadType;
        bool _error;
        bool _completed;

        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { this.RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public bool Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }
        public bool Completed
        {
            get { return _completed; }
            set { this.RaiseAndSetIfChanged(ref _completed, value); }
        }

        public LibraryWorkItemViewModel()
        {
            this.FileName = string.Empty;
        }
    }
}
