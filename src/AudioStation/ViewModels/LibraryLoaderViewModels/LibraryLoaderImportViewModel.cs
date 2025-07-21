using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Model;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.Vendor.TagLibViewModel;
using AudioStation.Windows;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
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

        LibraryLoaderImportOptionsViewModel _options;

        NotifyingObservableCollection<LibraryLoaderImportFileViewModel> _sourceFiles;

        SimpleCommand _editOptionsCommand;
        SimpleCommand _editTagsCommand;
        SimpleCommand _runImportCommand;
        SimpleCommand _runImportTestCommand;
        SimpleCommand _runMusicBrainzLookupCommand;

        public LibraryLoaderImportOptionsViewModel Options
        {
            get { return _options; }
            set { this.RaiseAndSetIfChanged(ref _options, value); }
        }
        public NotifyingObservableCollection<LibraryLoaderImportFileViewModel> SourceFiles
        {
            get { return _sourceFiles; }
            set { this.RaiseAndSetIfChanged(ref _sourceFiles, value); }
        }
        public int SourceFileSelectedCount
        {
            get { return _sourceFiles.Count(x => x.IsSelected); }
            set { OnPropertyChanged("SourceFileSelectedCount"); }
        }
        public SimpleCommand EditOptionsCommand
        {
            get { return _editOptionsCommand; }
            set { this.RaiseAndSetIfChanged(ref _editOptionsCommand, value); }
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
            this.Options = new LibraryLoaderImportOptionsViewModel(configurationManager, dialogController);

            // RunImport -> Complete
            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(payload =>
            {
                RefreshImportFiles(true);
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

            this.RunMusicBrainzLookupCommand = new SimpleCommand(() =>
            {

            });

            this.EditOptionsCommand = new SimpleCommand(() =>
            {
                dialogController.ShowImportOptionsWindow(this.Options);
                
                RefreshImportFiles(true);
            });

            this.SourceFiles.ItemPropertyChanged += SourceFiles_ItemPropertyChanged;
        }

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            if (string.IsNullOrEmpty(this.Options.SourceFolder))
                return Task.CompletedTask;

            // Task Thread Issue:  Calling back for the directory search - while inside the
            //                     Dispatcher invoke - was taking a lot of extra time (4-5 seconds!)
            //                  
            //                     We're going to call the file system before entering the 
            //                     invoke - and also use the native IO.
            //
            var sourceFiles = CalculateSourceFiles(true);

            // Load UI:  Dispatcher Only
            //
            if (sourceFiles.Any())
            {
                ApplicationHelpers.InvokeDispatcher(() =>
                {
                    this.SourceFiles.AddRange(sourceFiles);

                }, DispatcherPriority.Normal);
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            ClearSourceFiles();
        }

        private void SourceFiles_ItemPropertyChanged(NotifyingObservableCollection<LibraryLoaderImportFileViewModel> item1, LibraryLoaderImportFileViewModel item2, PropertyChangedEventArgs item3)
        {
            OnPropertyChanged("SourceFileSelectedCount");

            this.EditOptionsCommand.RaiseCanExecuteChanged();
            this.EditTagsCommand.RaiseCanExecuteChanged();
            this.RunImportCommand.RaiseCanExecuteChanged();
            this.RunImportTestCommand.RaiseCanExecuteChanged();
            this.RunMusicBrainzLookupCommand.RaiseCanExecuteChanged();
        }

        private void RunImport(Configuration configuration, bool testOnly)
        {
            // Queue work items
            foreach (var fileViewModel in this.SourceFiles)
            {
                RunImportImpl(configuration, fileViewModel.FileFullPath, testOnly);
            }
        }

        private void EditTags(ITagCacheController tagCacheController)
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

        private void RefreshImportFiles(bool useNativeFileIO)
        {
            if (!string.IsNullOrEmpty(this.Options.SourceFolder))
            {
                // Calculate Source Files
                //
                var sourceFiles = CalculateSourceFiles(useNativeFileIO);

                // Load UI:  Dispatcher Only
                //
                this.SourceFiles.AddRange(sourceFiles);
            }
            else
            {
                ClearSourceFiles();
            }

            //RefreshDestinationFiles();
        }

        private LibraryLoaderImportFileViewModel CreateSourceFile(string fileName, string directory)
        {
            var viewModel = new LibraryLoaderImportFileViewModel(fileName, directory, 
                                                                 this.Options.ImportAsType, 
                                                                 this.Options.NamingType, 
                                                                 this.Options.GroupingType);

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

        private IEnumerable<LibraryLoaderImportFileViewModel> CalculateSourceFiles(bool useNativeFileIO)
        {
            // Must have a valid configuration
            if (!_configurationManager.ValidateConfiguration())
                return Enumerable.Empty<LibraryLoaderImportFileViewModel>();

            // May be on task thread; but still ok.
            var configuration = _configurationManager.GetConfiguration();

            var result = new List<string>();

            // Task Thread / Dispatcher Thread:  During loading, there is a task thread, which must have access to create
            //                                   these files; but there may be a dispatcher invoke (to complicate matters).
            //                                   The NativeIO (FastGetFiles) will have problems with loading; and will take
            //                                   longer than the Dispatcher thread (managed version).
            //
            if (useNativeFileIO)
            {
                result.AddRange(ApplicationHelpers.FastGetFiles(this.Options.SourceFolder, "*.mp3", SearchOption.AllDirectories));
            }
            else
            {
                result.AddRange(Directory.GetFiles(this.Options.SourceFolder, "*.mp3", SearchOption.AllDirectories));
            }

            var directoryBase = configuration.DirectoryBase;
            var subDirectory = this.Options.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                               this.Options.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                               string.Empty;

            // Calculate base directory
            var directory = Path.Combine(directoryBase, subDirectory);

            ClearSourceFiles();

            if (!string.IsNullOrWhiteSpace(this.Options.SourceFolderSearch))
                return result.Where(x => StringHelpers.RegexMatchIC(this.Options.SourceFolderSearch, x))
                                                      .Select(x => CreateSourceFile(x, directory));
            else
                return result.Select(x => CreateSourceFile(x, directory));
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
                return _modelValidationService.ValidateMusicBrainzRecordingImport(match);
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
            sender.TagFile.Tag.Track = (uint)trackMedia.Tracks.First(x => x.Title == bestMatch.Title).Position;
            sender.TagFile.Tag.TrackCount = (uint)trackMedia.TrackCount;
            sender.TagFile.Tag.Disc = (uint)(release.Media.IndexOf(trackMedia) + 1);
            sender.TagFile.Tag.DiscCount = (uint)release.Media.Count;

            // TODO: TRACK !!! There's no track data by default! The IRecording has no track data from music brainz!

            _tagCacheController.SetData(sender.FileFullPath, sender.TagFile.Tag, true);
        }

        private void RunImportImpl(Configuration configuration, string fileFullPath, bool testOnly)
        {
            var directoryBase = configuration.DirectoryBase;
            var subDirectory = this.Options.ImportAsType == LibraryEntryType.Music ? configuration.MusicSubDirectory :
                               this.Options.ImportAsType == LibraryEntryType.AudioBook ? configuration.AudioBooksSubDirectory :
                               string.Empty;

            // Calculate base directory
            var directory = Path.Combine(directoryBase, subDirectory);

            // Setup file load for the library loader
            var inputLoad = new LibraryLoaderImportLoad(this.Options.SourceFolder,
                                                             directory,
                                                             fileFullPath,
                                                             LibraryEntryGroupingType.ArtistAlbum,
                                                             LibraryEntryNamingType.Standard,
                                                             this.Options.IncludeMusicBrainzDetail,
                                                             this.Options.IdentifyUsingAcoustID,
                                                             this.Options.ImportFileMigration,
                                                             this.Options.MigrationDeleteSourceFiles,
                                                             this.Options.MigrationDeleteSourceFolders,
                                                             this.Options.MigrationOverwriteDestinationFiles);

            // Queue worker task
            _libraryLoaderService.RunLoaderTaskAsync(inputLoad);
        }
    }
}
