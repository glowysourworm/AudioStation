using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderImportAdvancedViewModel : ViewModelBase
    {
        private readonly IConfigurationManager _configurationManager;

        string _fileStagingSearch;
        string _importFolder;

        LibraryEntryType _selectedImportType;
        ObservableCollection<string> _importFilesStaged;
        string _selectedImportStagedFile;
        bool _deleteOnImport;

        SimpleCommand _runImportCommand;

        public string FileStagingSearch
        {
            get { return _fileStagingSearch; }
            set { this.RaiseAndSetIfChanged(ref _fileStagingSearch, value); RefreshImportFiles(); }
        }

        public ObservableCollection<string> ImportFilesStaged
        {
            get { return _importFilesStaged; }
            set { this.RaiseAndSetIfChanged(ref _importFilesStaged, value); }
        }
        public string SelectedImportStagedFile
        {
            get { return _selectedImportStagedFile; }
            set { this.RaiseAndSetIfChanged(ref _selectedImportStagedFile, value); }
        }
        public string ImportFolder
        {
            get { return _importFolder; }
            set { this.RaiseAndSetIfChanged(ref _importFolder, value); }
        }
        public bool DeleteOnImport
        {
            get { return _deleteOnImport; }
            set { this.RaiseAndSetIfChanged(ref _deleteOnImport, value); }
        }
        public LibraryEntryType SelectedImportType
        {
            get { return _selectedImportType; }
            set { this.RaiseAndSetIfChanged(ref _selectedImportType, value); RefreshImportFiles(); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }


        [IocImportingConstructor]
        public LibraryLoaderImportAdvancedViewModel(IConfigurationManager configurationManager,
                                                    ILibraryLoader libraryLoader)
        {
            _configurationManager = configurationManager;

            this.ImportFilesStaged = new ObservableCollection<string>();
            this.FileStagingSearch = string.Empty;

            this.RunImportCommand = new SimpleCommand(() =>
            {
                var directoryBase = configurationManager.GetConfiguration().DirectoryBase;
                var subDirectory = this.SelectedImportType == LibraryEntryType.Music ? configurationManager.GetConfiguration().MusicSubDirectory :
                                   this.SelectedImportType == LibraryEntryType.AudioBook ? configurationManager.GetConfiguration().AudioBooksSubDirectory :
                                   string.Empty;

                // Calculate base directory
                var directory = Path.Combine(directoryBase, subDirectory);

                // Setup file load for the library loader
                var inputLoad = new LibraryLoaderImportLoad(this.SelectedImportStagedFile, directory, this.DeleteOnImport);
                libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderImportLoad>(LibraryLoadType.ImportAdvanced, inputLoad));
            });

            RefreshImportFiles();
        }

        private void RefreshImportFiles()
        {
            var configuration = _configurationManager.GetConfiguration();

            if (!string.IsNullOrEmpty(configuration.DirectoryBase))
            {
                var directoryBase = configuration.DirectoryBase;
                var subDirectory = this.SelectedImportType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                                   this.SelectedImportType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                                   string.Empty;

                // Calculate base directory
                var directory = Path.Combine(directoryBase, subDirectory);

                var files = ApplicationHelpers.FastGetFiles(directory, "*.mp3", SearchOption.AllDirectories);

                this.ImportFolder = directory;
                this.ImportFilesStaged.Clear();

                if (!string.IsNullOrWhiteSpace(this.FileStagingSearch))
                    this.ImportFilesStaged.AddRange(files.Where(x => StringHelpers.RegexMatchIC(this.FileStagingSearch, x)));

                else
                    this.ImportFilesStaged.AddRange(files);
            }
            else
            {
                this.ImportFilesStaged.Clear();
            }
        }

        public void SetImportComplete(LibraryLoaderImportLoadOutput output)
        {

        }
    }
}
