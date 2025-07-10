using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderImportViewModel : ViewModelBase
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IDialogController _dialogController;

        string _sourceFolderSearch;
        string _sourceFolder;
        string _destinationFolderSearch;
        string _destinationFolder;
        string _destinationMusicSubFolder;
        string _destinationAudioBooksSubFolder;

        LibraryEntryType _importAsType;
        LibraryEntryGroupingType _groupingType;
        LibraryEntryNamingType _namingType;

        NotifyingObservableCollection<LibraryLoaderImportFileViewModel> _sourceFiles;
        NotifyingObservableCollection<LibraryLoaderImportOutputViewModel> _destinationFiles;

        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;

        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;
        SimpleCommand _editTagsCommand;
        SimpleCommand _copyTagCommand;
        SimpleCommand _runImportCommand;
        SimpleCommand _runImportTestCommand;
        SimpleCommand _runMusicBrainzLookupCommand;

        public string SourceFolderSearch
        {
            get { return _sourceFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolderSearch, value); }
        }
        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolder, value); }
        }
        public string DestinationFolderSearch
        {
            get { return _destinationFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolderSearch, value); }
        }
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolder, value); }
        }
        public string DestinationMusicSubFolder
        {
            get { return _destinationMusicSubFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationMusicSubFolder, value); }
        }
        public string DestinationAudioBooksSubFolder
        {
            get { return _destinationAudioBooksSubFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationAudioBooksSubFolder, value); }
        }
        public LibraryEntryType ImportAsType
        {
            get { return _importAsType; }
            set { this.RaiseAndSetIfChanged(ref _importAsType, value); }
        }
        public LibraryEntryGroupingType GroupingType
        {
            get { return _groupingType; }
            set { this.RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public LibraryEntryNamingType NamingType
        {
            get { return _namingType; }
            set { this.RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public NotifyingObservableCollection<LibraryLoaderImportFileViewModel> SourceFiles
        {
            get { return _sourceFiles; }
            set { this.RaiseAndSetIfChanged(ref _sourceFiles, value); }
        }
        public NotifyingObservableCollection<LibraryLoaderImportOutputViewModel> DestinationFiles
        {
            get { return _destinationFiles; }
            set { this.RaiseAndSetIfChanged(ref _destinationFiles, value); }
        }
        public bool IncludeMusicBrainzDetail
        {
            get { return _includeMusicBrainzDetail; }
            set { this.RaiseAndSetIfChanged(ref _includeMusicBrainzDetail, value); }
        }
        public bool IdentifyUsingAcoustID
        {
            get { return _identifyUsingAcoustID; }
            set { this.RaiseAndSetIfChanged(ref _identifyUsingAcoustID, value); }
        }
        public bool ImportFileMigration
        {
            get { return _importFileMigration; }
            set { this.RaiseAndSetIfChanged(ref _importFileMigration, value); }
        }
        public bool MigrationDeleteSourceFiles
        {
            get { return _migrationDeleteSourceFiles; }
            set { this.RaiseAndSetIfChanged(ref _migrationDeleteSourceFiles, value); }
        }
        public bool MigrationDeleteSourceFolders
        {
            get { return _migrationDeleteSourceFolders; }
            set { this.RaiseAndSetIfChanged(ref _migrationDeleteSourceFolders, value); }
        }
        public bool MigrationOverwriteDestinationFiles
        {
            get { return _migrationOverwriteDestinationFiles; }
            set { this.RaiseAndSetIfChanged(ref _migrationOverwriteDestinationFiles, value); }
        }
        public bool SourceFolderSelectAll
        {
            get { return _sourceFiles.All(x => x.IsSelected); }
            set { ToggleSourceFolderSelectAll(); }
        }
        public int SourceFileSelectedCount
        {
            get { return _sourceFiles.Count(x => x.IsSelected); }
            set { OnPropertyChanged("SourceFileSelectedCount"); }
        }
        public SimpleCommand SelectSourceFolderCommand
        {
            get { return _selectSourceFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectSourceFolderCommand, value); }
        }
        public SimpleCommand EditTagsCommand
        {
            get { return _editTagsCommand; }
            set { this.RaiseAndSetIfChanged(ref _editTagsCommand, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }
        public SimpleCommand RunImportTestCommand
        {
            get { return _runImportTestCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportTestCommand, value); }
        }
        public SimpleCommand RunMusicBrainzLookupCommand
        {
            get { return _runMusicBrainzLookupCommand; }
            set { this.RaiseAndSetIfChanged(ref _runMusicBrainzLookupCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderImportViewModel(IConfigurationManager configurationManager,
                                            ILibraryLoader libraryLoader,
                                            IDialogController dialogController,
                                            ITagCacheController tagCacheController)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;

            var configuration = configurationManager.GetConfiguration();

            this.SourceFiles = new NotifyingObservableCollection<LibraryLoaderImportFileViewModel>();
            this.DestinationFiles = new NotifyingObservableCollection<LibraryLoaderImportOutputViewModel>();
            this.SourceFolderSearch = string.Empty;
            this.DestinationFolderSearch = string.Empty;

            this.SourceFolder = configuration.DownloadFolder;
            this.DestinationFolder = configuration.DirectoryBase;
            this.DestinationMusicSubFolder = configuration.MusicSubDirectory;
            this.DestinationAudioBooksSubFolder = configuration.AudioBooksSubDirectory;

            this.ImportAsType = LibraryEntryType.Music;
            this.GroupingType = LibraryEntryGroupingType.ArtistAlbum;
            this.NamingType = LibraryEntryNamingType.Standard;

            this.EditTagsCommand = new SimpleCommand(() =>
            {
                EditTags(tagCacheController);
            });

            this.RunImportCommand = new SimpleCommand(() =>
            {
                RunImport(libraryLoader, configurationManager.GetConfiguration(), false);
            });

            this.RunImportTestCommand = new SimpleCommand(() =>
            {
                RunImport(libraryLoader, configurationManager.GetConfiguration(), true);
            });

            this.SelectSourceFolderCommand = new SimpleCommand(() =>
            {
                var originalFolder = this.SourceFolder;
                var folder = dialogController.ShowSelectFolder();

                if (!string.IsNullOrEmpty(folder))
                {
                    this.SourceFolder = folder;

                    if (folder != originalFolder)
                        RefreshImportFiles();
                }
            });

            this.SourceFiles.ItemPropertyChanged += SourceFiles_ItemPropertyChanged;
            this.DestinationFiles.ItemPropertyChanged += DestinationFiles_ItemPropertyChanged;

            RefreshImportFiles();
        }

        private void DestinationFiles_ItemPropertyChanged(NotifyingObservableCollection<LibraryLoaderImportOutputViewModel> item1, LibraryLoaderImportOutputViewModel item2, PropertyChangedEventArgs item3)
        {
            
        }

        private void SourceFiles_ItemPropertyChanged(NotifyingObservableCollection<LibraryLoaderImportFileViewModel> item1, LibraryLoaderImportFileViewModel item2, PropertyChangedEventArgs item3)
        {
            OnPropertyChanged("SourceFileSelectedCount");

            RefreshDestinationFiles();
        }

        private void RunImport(ILibraryLoader libraryLoader, Configuration configuration, bool testOnly)
        {
            var directoryBase = configuration.DirectoryBase;
            var subDirectory = this.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                               this.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                               string.Empty;

            // Calculate base directory
            var directory = Path.Combine(directoryBase, subDirectory);

            // Setup file load for the library loader
            var inputLoad = new LibraryLoaderImportLoad(this.SourceFolder,
                                                        directory,
                                                        this.SourceFiles.Select(x => x.FileFullPath),
                                                        LibraryEntryGroupingType.ArtistAlbum,
                                                        LibraryEntryNamingType.Standard,
                                                        this.IncludeMusicBrainzDetail,
                                                        this.IdentifyUsingAcoustID,
                                                        this.ImportFileMigration,
                                                        this.MigrationDeleteSourceFiles,
                                                        this.MigrationDeleteSourceFolders,
                                                        this.MigrationOverwriteDestinationFiles,
                                                        testOnly);

            libraryLoader.RunLoaderTask(new LibraryLoaderParameters<LibraryLoaderImportLoad>(LibraryLoadType.Import, inputLoad));
            libraryLoader.Start();
        }

        private void  EditTags(ITagCacheController tagCacheController)
        {
            var inputFiles = _sourceFiles.Where(x => x.IsSelected).ToList();
            var firstFile = inputFiles.FirstOrDefault();

            if (firstFile == null)
                return;

            // Base the tag view model on the first input. Then, build a group tag
            // from there.
            //
            try
            {
                var fileViewModels = new List<TagFileViewModel>();

                foreach (var file in inputFiles)
                {
                    var tagFile = tagCacheController.Get(file.FileFullPath);
                    var tagFileViewModel = new TagFileViewModel(tagFile);

                    // Add tag to the group view model
                    fileViewModels.Add(tagFileViewModel);
                }

                _dialogController.ShowTagWindow(new TagFileGroupViewModel(fileViewModels));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading file tag:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }

        private void ToggleSourceFolderSelectAll()
        {
            var selectAll = _sourceFiles.All(x => x.IsSelected);

            foreach (var file in _sourceFiles)
            {
                file.IsSelected = !selectAll;
            }

            OnPropertyChanged("SourceFolderSelectAll");
        }

        private void RefreshImportFiles()
        {
            var configuration = _configurationManager.GetConfiguration();

            if (!string.IsNullOrEmpty(this.SourceFolder))
            {
                var files = ApplicationHelpers.FastGetFiles(this.SourceFolder, "*.mp3", SearchOption.AllDirectories);

                var directoryBase = configuration.DirectoryBase;
                var subDirectory = this.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                                   this.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                                   string.Empty;

                // Calculate base directory
                var directory = Path.Combine(directoryBase, subDirectory);

                this.SourceFiles.Clear();

                if (!string.IsNullOrWhiteSpace(this.SourceFolderSearch))
                    this.SourceFiles.AddRange(files.Where(x => StringHelpers.RegexMatchIC(this.SourceFolderSearch, x))
                                                   .Select(x => new LibraryLoaderImportFileViewModel(x, directory, this.ImportAsType)));
                else
                    this.SourceFiles.AddRange(files.Select(x => new LibraryLoaderImportFileViewModel(x, directory, this.ImportAsType)));
            }
            else
            {
                this.SourceFiles.Clear();
            }

            //RefreshDestinationFiles();
        }

        private void RefreshDestinationFiles()
        {
            this.DestinationFiles.Clear();
            //this.DestinationFiles.AddRange(this.SourceFiles
            //                                   .Where(x => x.IsSelected)
            //                                   .Select(x => new LibraryLoaderImportOutputViewModel()
            //{
            //    ImportFileName = x.FileName
            //}));
        }

        public void SetImportComplete(LibraryLoaderImportLoadOutput output)
        {

        }
    }
}
