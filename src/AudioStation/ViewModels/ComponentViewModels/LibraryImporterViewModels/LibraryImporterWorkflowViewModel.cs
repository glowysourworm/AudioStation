using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterWorkflowViewModel : ViewModelBase
    {
        string _name;
        DateTime _createdDate;
        DateTime _modifiedDate;
        LibraryImporterConfigurationViewModel _configuration;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { this.RaiseAndSetIfChanged(ref _createdDate, value); }
        }
        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
            set { this.RaiseAndSetIfChanged(ref _modifiedDate, value); }
        }
        public LibraryImporterConfigurationViewModel Configuration
        {
            get { return _configuration; }
            set { this.RaiseAndSetIfChanged(ref _configuration, value); }
        }


        public LibraryImporterWorkflowViewModel()
        {
            this.Name = string.Empty;
            this.Configuration = new LibraryImporterConfigurationViewModel();
        }
    }
}
