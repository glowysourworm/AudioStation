using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    /// <summary>
    /// PathViewModel provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryImporterFileTreeNodeViewModel : FileTreeNodeViewModel
    {
        LibraryImporterFileViewModel _importFile;

        public LibraryImporterFileViewModel ImportFile
        {
            get { return _importFile; }
            set { this.RaiseAndSetIfChanged(ref _importFile, value); }
        }

        /// <summary>
        /// Constructor for an import file view model. This may represent either a file or a directory.
        /// </summary>
        public LibraryImporterFileTreeNodeViewModel(string fullPath,
                                                    string baseDirectory,
                                                    bool isDirectory,
                                                    TreeViewNodeModelBase? parent,
                                                    LibraryImporterConfigurationViewModel importerConfiguration)
            : base(baseDirectory, fullPath, 0, parent)
        {
            // Directories are just placeholders (at the moment)
            if (this.IsDirectory)
                throw new ArgumentException("Cannot set imporeter file tree node with directory file information");

            else
                this.ImportFile = new LibraryImporterFileViewModel(fullPath, baseDirectory, importerConfiguration);
        }

        /// <summary>
        /// Constructor for an import file view model with a custom tree depth. This may represent either a file or a directory.
        /// </summary>
        public LibraryImporterFileTreeNodeViewModel(int treeDepth,
                                                    string fullPath,
                                                    string baseDirectory,
                                                    bool isDirectory,
                                                    TreeViewNodeModelBase? parent,
                                                    LibraryImporterConfigurationViewModel importerConfiguration)
            : base(treeDepth, baseDirectory, fullPath, 0, parent)
        {
            // Directories are just placeholders (at the moment)
            if (this.IsDirectory)
                throw new ArgumentException("Cannot set imporeter file tree node with directory file information");

            else
                this.ImportFile = new LibraryImporterFileViewModel(fullPath, baseDirectory, importerConfiguration);
        }

        public override string ToString()
        {
            return this.FullPath;
        }
    }
}
