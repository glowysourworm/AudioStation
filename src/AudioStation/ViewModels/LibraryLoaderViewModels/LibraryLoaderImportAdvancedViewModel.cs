using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderImportAdvancedViewModel : ViewModelBase
    {
        LibraryEntryType _selectedImportType;
        ObservableCollection<string> _importFilesStaged;
        string _selectedImportStagedFile;
        bool _deleteOnImport;

        SimpleCommand _runImportCommand;

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
        public bool DeleteOnImport
        {
            get { return _deleteOnImport; }
            set { this.RaiseAndSetIfChanged(ref _deleteOnImport, value); }
        }
        public LibraryEntryType SelectedImportType
        {
            get { return _selectedImportType; }
            set { this.RaiseAndSetIfChanged(ref _selectedImportType, value); }
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
            this.ImportFilesStaged = new ObservableCollection<string>();

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
        }

        public void SetImportComplete(LibraryLoaderImportLoadOutput output)
        {

        }
    }
}
