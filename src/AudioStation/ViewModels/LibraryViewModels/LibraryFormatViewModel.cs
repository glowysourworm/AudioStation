using System.Collections.ObjectModel;

using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryFormatViewModel : ViewModelBase, ILibraryFormat
    {
        string _leftToken;
        string _rightToken;
        LibraryFormatUse _use;
        ObservableCollection<LibraryFormatFieldViewModel> _fields;

        public string LeftToken
        {
            get { return _leftToken; }
            set { this.RaiseAndSetIfChanged(ref _leftToken, value); }
        }
        public string RightToken
        {
            get { return _rightToken; }
            set { this.RaiseAndSetIfChanged(ref _rightToken, value); }
        }
        public LibraryFormatUse Use
        {
            get { return _use; }
            set { this.RaiseAndSetIfChanged(ref _use, value); }
        }
        public ObservableCollection<LibraryFormatFieldViewModel> Fields
        {
            get { return _fields; }
            set { this.RaiseAndSetIfChanged(ref _fields, value); }
        }

        IEnumerable<ILibraryFormatField> ILibraryFormat.Fields
        {
            get { return _fields; }
            set
            {
                throw new NotSupportedException("Cannot cast-set LibraryFormatViewModel.Fields property");
            }
        }

        public LibraryFormatViewModel()
        {
            this.LeftToken = MigrationHelpers.FormatLeftToken;
            this.RightToken = MigrationHelpers.FormatRightToken;
            this.Use = LibraryFormatUse.Folder;
            this.Fields = new ObservableCollection<LibraryFormatFieldViewModel>();
        }
    }
}
