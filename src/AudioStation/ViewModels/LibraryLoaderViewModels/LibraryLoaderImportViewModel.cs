using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.EventHandler;
using AudioStation.Model;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportViewModel : PrimaryViewModelBase
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IDialogController _dialogController;
        private readonly ILibraryLoaderService _libraryLoaderService;
        private readonly IModelValidationService _modelValidationService;
        private readonly ITagCacheController _tagCacheController;

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

        bool _includeMusicBrainzDetail;
        bool _identifyUsingAcoustID;

        bool _importFileMigration;
        bool _migrationDeleteSourceFiles;
        bool _migrationDeleteSourceFolders;
        bool _migrationOverwriteDestinationFiles;

        SimpleCommand _selectSourceFolderCommand;
        SimpleCommand _editTagsCommand;
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

        public LibraryLoaderImportViewModel(IConfigurationManager configurationManager,
                                            ILibraryLoaderService libraryLoaderService,
                                            IDialogController dialogController,
                                            ITagCacheController tagCacheController,
                                            IModelValidationService modelValidationService,
                                            IIocEventAggregator eventAggregator)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;
            _libraryLoaderService = libraryLoaderService;
            _modelValidationService = modelValidationService;
            _tagCacheController = tagCacheController;

            var configuration = configurationManager.GetConfiguration();

            this.SourceFiles = new NotifyingObservableCollection<LibraryLoaderImportFileViewModel>();
            this.SourceFolderSearch = string.Empty;
            this.DestinationFolderSearch = string.Empty;

            this.SourceFolder = configuration.DownloadFolder;
            this.DestinationFolder = configuration.DirectoryBase;
            this.DestinationMusicSubFolder = configuration.MusicSubDirectory;
            this.DestinationAudioBooksSubFolder = configuration.AudioBooksSubDirectory;

            this.ImportAsType = LibraryEntryType.Music;
            this.GroupingType = LibraryEntryGroupingType.ArtistAlbum;
            this.NamingType = LibraryEntryNamingType.Standard;

            // RunImport -> Complete
            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(payload =>
            {
                RefreshImportFiles();
            });

            this.EditTagsCommand = new SimpleCommand(() =>
            {
                EditTags(tagCacheController);
            });

            this.RunImportCommand = new SimpleCommand(() =>
            {
                RunImport(configurationManager.GetConfiguration(), false);
            });

            this.RunImportTestCommand = new SimpleCommand(() =>
            {
                RunImport(configurationManager.GetConfiguration(), true);
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
        }

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            // Let this run in the background
            ApplicationHelpers.InvokeDispatcher(() =>
            {
                RefreshImportFiles();

            }, DispatcherPriority.Background);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            ClearSourceFiles();
        }

        private void SourceFiles_ItemPropertyChanged(NotifyingObservableCollection<LibraryLoaderImportFileViewModel> item1, LibraryLoaderImportFileViewModel item2, PropertyChangedEventArgs item3)
        {
            OnPropertyChanged("SourceFileSelectedCount");
        }

        private void RunImport(Configuration configuration, bool testOnly)
        {
            // Queue work items
            foreach (var fileViewModel in this.SourceFiles)
            {
                RunImportImpl(configuration, fileViewModel.FileFullPath, testOnly);
            }
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
                ApplicationHelpers.Log("Error loading file tag:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
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

                ClearSourceFiles();

                if (!string.IsNullOrWhiteSpace(this.SourceFolderSearch))
                    this.SourceFiles.AddRange(files.Where(x => StringHelpers.RegexMatchIC(this.SourceFolderSearch, x))
                                                   .Select(x => CreateSourceFile(x, directory, this.ImportAsType)));
                else
                    this.SourceFiles.AddRange(files.Select(x => CreateSourceFile(x, directory, this.ImportAsType)));


            }
            else
            {
                ClearSourceFiles();
            }

            //RefreshDestinationFiles();
        }

        private LibraryLoaderImportFileViewModel CreateSourceFile(string fileName, string directory, LibraryEntryType importAsType)
        {
            var viewModel = new LibraryLoaderImportFileViewModel(fileName, directory, importAsType);

            // Hook event request
            viewModel.ImportBasicEvent += OnImportBasicRequest;
            viewModel.SaveTagEvent += OnSaveTag;

            return viewModel;
        }

        private void ClearSourceFiles()
        {
            // Unhook event request
            foreach (var sourceFile in this.SourceFiles)
            {
                sourceFile.ImportBasicEvent -= OnImportBasicRequest;
                sourceFile.SaveTagEvent -= OnSaveTag;
            }

            this.SourceFiles.Clear();
        }

        private void OnImportBasicRequest(LibraryLoaderImportFileViewModel sender)
        {
            RunImportImpl(_configurationManager.GetConfiguration(), sender.FileFullPath, false);
        }

        private void OnSaveTag(LibraryLoaderImportFileViewModel sender)
        {
            // Procedure
            //
            // 1) Verify AcoustID Success
            // 2) Verify Music Brainz Success
            // 3) Validate Music Brainz Best Match (Artist, Album, Track Title, Track Number)
            // 4) Save all data available
            //

            if (!sender.ImportOutput.AcoustIDSuccess)
                throw new ArgumentException("Cannot save import data without successful AcoustID result");

            if (!sender.ImportOutput.MusicBrainzRecordingMatchSuccess)
                throw new ArgumentException("Cannot save import data without successful Music Brainz result");

            var bestMatch = sender.ImportOutput.MusicBrainzRecordingMatches.FirstOrDefault(match =>
            {
                return _modelValidationService.ValidateMusicBrainzRecording_ImportBasic(match);
            });

            if (bestMatch == null)
                throw new ArgumentException("Music Brainz Results Invalid:  Cannot save import data without successful Music Brainz result.");

            // Validated Fields
            var release = bestMatch.Releases.First();
            sender.TagFile.Tag.Album = release.Title;
            sender.TagFile.Tag.AlbumSort = release.Title;
            sender.TagFile.Tag.Artists = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
            sender.TagFile.Tag.AlbumArtists = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
            sender.TagFile.Tag.AlbumArtistsSort = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
            sender.TagFile.Tag.Title = bestMatch.Title;
            sender.TagFile.Tag.TitleSort = bestMatch.Title;
            
            // Also Validated: This may be the CD or "Media" that contains the track (only); or some duplicate record of
            //                 that media (if there are multiple releases); but there will be at least one with the track information.
            //
            var trackMedia = release.Media.First(x => x.Tracks.Any(z => z.Title == bestMatch.Title));

            sender.TagFile.Tag.Year = (uint)(release.Date.Year ?? 0);
            sender.TagFile.Tag.Track = (uint)(trackMedia.Tracks.First(x => x.Title == bestMatch.Title).Position);
            sender.TagFile.Tag.TrackCount = (uint)trackMedia.TrackCount;
            sender.TagFile.Tag.Disc = (uint)(release.Media.IndexOf(trackMedia) + 1);
            sender.TagFile.Tag.DiscCount = (uint)release.Media.Count;

            // TODO: TRACK !!! There's no track data by default! The IRecording has no track data from music brainz!

            _tagCacheController.SetData(sender.FileFullPath, sender.TagFile.Tag, true);
        }

        private void RunImportImpl(Configuration configuration, string fileFullPath, bool testOnly)
        {
            var directoryBase = configuration.DirectoryBase;
            var subDirectory = this.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                               this.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                               string.Empty;

            // Calculate base directory
            var directory = Path.Combine(directoryBase, subDirectory);

            // Setup file load for the library loader
            var inputLoad = new LibraryLoaderImportBasicLoad(this.SourceFolder,
                                                             directory,
                                                             fileFullPath,
                                                             LibraryEntryGroupingType.ArtistAlbum,
                                                             LibraryEntryNamingType.Standard,
                                                             this.IncludeMusicBrainzDetail,
                                                             this.IdentifyUsingAcoustID,
                                                             this.ImportFileMigration,
                                                             this.MigrationDeleteSourceFiles,
                                                             this.MigrationDeleteSourceFolders,
                                                             this.MigrationOverwriteDestinationFiles);

            // Queue worker task
            _libraryLoaderService.RunLoaderTaskAsync(inputLoad);
        }
    }
}
