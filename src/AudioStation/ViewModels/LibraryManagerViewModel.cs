using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Component.Interface;
using AudioStation.ViewModels.LibraryManagerViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryManagerViewModel : ViewModelBase
    {
        LibraryViewModel _library;

        public LibraryViewModel Library
        {
            get { return _library; }
            set { this.RaiseAndSetIfChanged(ref _library, value); }
        }

        [IocImportingConstructor]
        public LibraryManagerViewModel(IViewModelLoader viewModelLoader)
        {
            this.Library = new LibraryViewModel(viewModelLoader);
        }
    }
}
