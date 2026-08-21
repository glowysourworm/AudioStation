using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    /// <summary>
    /// Class to decouple the IsSelected status between several list boxes
    /// </summary>
    public class LibraryImporterStagedFileViewModel : ViewModelBase
    {
        bool _isSelected;
        LibraryImporterFileViewModel _file;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public LibraryImporterFileViewModel File
        {
            get { return _file; }
            set { this.RaiseAndSetIfChanged(ref _file, value); }
        }

        public LibraryImporterStagedFileViewModel()
        {
            this.File = null;
            this.IsSelected = false;
        }
    }
}
