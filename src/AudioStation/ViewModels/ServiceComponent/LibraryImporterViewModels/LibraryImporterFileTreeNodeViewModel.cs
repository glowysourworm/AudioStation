using SimpleWpf.UI.ViewModel.FileTreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    /// <summary>
    /// PathViewModel provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryImporterFileTreeNodeViewModel : FileTreeNodeViewModel
    {
        LibraryImporterFileViewModel _importFile;
        LibraryImporterDirectoryViewModel _importDirectory;

        public LibraryImporterFileViewModel ImportFile
        {
            get { return _importFile; }
            set { this.RaiseAndSetIfChanged(ref _importFile, value); }
        }
        public LibraryImporterDirectoryViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }

        /// <summary>
        /// Constructor for an import file view model. This may represent either a file or a directory.
        /// </summary>
        public LibraryImporterFileTreeNodeViewModel(string fullPath,
                                                    string baseDirectory,
                                                    bool isDirectory,
                                                    LibraryImporterFileTreeNodeViewModel? parent,
                                                    LibraryImporterConfigurationViewModel importerConfiguration)
            : base(baseDirectory, fullPath, 0, parent)
        {
            // Directories are just placeholders (at the moment)
            if (this.IsDirectory)
                this.ImportDirectory = new LibraryImporterDirectoryViewModel();

            else
                this.ImportFile = new LibraryImporterFileViewModel(fullPath, baseDirectory, parent, importerConfiguration);
        }

        public override string ToString()
        {
            return this.FullPath;
        }
    }
}
