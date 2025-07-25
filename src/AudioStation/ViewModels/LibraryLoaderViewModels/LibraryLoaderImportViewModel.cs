using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.ATLViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

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
        private readonly IIocEventAggregator _eventAggregator;
        private readonly ILibraryImporter _libraryImporter;
        private readonly ITagCacheController _tagCacheController;

        LibraryLoaderImportOptionsViewModel _options;

        NotifyingObservableCollection<LibraryLoaderImportFileViewModel> _sourceFiles;

        SimpleCommand _editOptionsCommand;
        SimpleCommand _editTagCommand;
        SimpleCommand _runImportCommand;
        SimpleCommand _runAcoustIDCommand;
        SimpleCommand _runMusicBrainzCommand;

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
        public SimpleCommand EditTagCommand
        {
            get { return _editTagCommand; }
            set { this.RaiseAndSetIfChanged(ref _editTagCommand, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }
        public SimpleCommand RunAcoustIDCommand
        {
            get { return _runAcoustIDCommand; }
            set { this.RaiseAndSetIfChanged(ref _runAcoustIDCommand, value); }
        }
        public SimpleCommand RunMusicBrainzCommand
        {
            get { return _runMusicBrainzCommand; }
            set { this.RaiseAndSetIfChanged(ref _runMusicBrainzCommand, value); }
        }

        public LibraryLoaderImportViewModel(IConfigurationManager configurationManager,
                                            IDialogController dialogController,
                                            IIocEventAggregator eventAggregator,
                                            ILibraryImporter libraryImporter,
                                            ITagCacheController tagCacheController)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;
            _libraryImporter = libraryImporter;
            _eventAggregator = eventAggregator;
            _tagCacheController = tagCacheController;

            var configuration = configurationManager.GetConfiguration();

            this.SourceFiles = new NotifyingObservableCollection<LibraryLoaderImportFileViewModel>();
            this.Options = new LibraryLoaderImportOptionsViewModel(configurationManager, dialogController);

            // RunImport -> Complete
            //eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(payload =>
            //{
            //    RefreshImportFiles(true);
            //});

            this.EditTagCommand = new SimpleCommand(() =>
            {
                EditTag();

            }, CanEditTags);

            this.RunImportCommand = new SimpleCommand(async () =>
            {
                await RunImport();

            }, CanRunImport);

            this.RunAcoustIDCommand = new SimpleCommand(async () =>
            {
                await RunAcoustID();

            }, CanRunAcoustID);

            this.RunMusicBrainzCommand = new SimpleCommand(async () =>
            {
                await RunMusicBrainz();

            }, CanRunMusicBrainz);

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

        private bool CanEditTags()
        {
            return this.SourceFileSelectedCount == 1;
        }
        private bool CanRunImport()
        {
            return this.SourceFileSelectedCount > 0 &&
                   this.SourceFiles
                       .Where(x => x.IsSelected)
                       .All(x => x.MinimumImportValid);
        }
        private bool CanRunAcoustID()
        {
            return this.SourceFileSelectedCount > 0;
        }
        private bool CanRunMusicBrainz()
        {
            return this.SourceFileSelectedCount > 0 &&
                   this.SourceFiles
                       .Where(x => x.IsSelected)
                       .All(x => x.ImportOutput.AcoustIDSuccess && x.SelectedAcoustIDResult != null);
        }

        private void SourceFiles_ItemPropertyChanged(NotifyingObservableCollection<LibraryLoaderImportFileViewModel> item1,
                                                     LibraryLoaderImportFileViewModel item2, PropertyChangedEventArgs item3)
        {
            OnPropertyChanged("SourceFileSelectedCount");

            this.EditOptionsCommand.RaiseCanExecuteChanged();
            this.EditTagCommand.RaiseCanExecuteChanged();
            this.RunImportCommand.RaiseCanExecuteChanged();
            this.RunAcoustIDCommand.RaiseCanExecuteChanged();
            this.RunMusicBrainzCommand.RaiseCanExecuteChanged();
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

            if (!string.IsNullOrWhiteSpace(this.Options.SourceFolderSearch))
                return result.Where(x => StringHelpers.RegexMatchIC(this.Options.SourceFolderSearch, x))
                                                      .Select(x => CreateSourceFile(x, directory));
            else
                return result.Select(x => CreateSourceFile(x, directory));
        }

        private LibraryLoaderImportFileViewModel CreateSourceFile(string fileName, string destinationDirectoryBase)
        {
            var viewModel = new LibraryLoaderImportFileViewModel(fileName, destinationDirectoryBase, this.Options);

            viewModel.SelectAcoustIDEvent += ShowAcoustIDResults;
            viewModel.SelectMusicBrainzEvent += ShowMusicBrainzResults;
            viewModel.PlayAudioEvent += ShowSmallAudioPlayer;

            return viewModel;
        }

        private void ClearSourceFiles()
        {
            for (int index = this.SourceFiles.Count - 1; index >= 0; index--)
            {
                this.SourceFiles[index].SelectAcoustIDEvent -= ShowAcoustIDResults;
                this.SourceFiles[index].SelectMusicBrainzEvent -= ShowMusicBrainzResults;
                this.SourceFiles[index].PlayAudioEvent -= ShowSmallAudioPlayer;
                this.SourceFiles.RemoveAt(index);
            }
        }

        private void EditTag()
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
                // Get the current working tag
                var tag = firstFile.GetTagCopy();

                // Create view model for the data
                var viewModel = new TagViewModel(tag);

                // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
                var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (" + firstFile.FileName + ")", DialogEditorView.TagView, viewModel));

                // User wishes to save the data
                if (dialogResult)
                {
                    // Update Import File (view model)(still in new memory only)
                    firstFile.SaveTagEdit(viewModel);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Application error:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private async Task RunImport()
        {
            // Procedure
            //
            // 0) Show Dialog (progress handler)
            // 1) CanRunImport (already checked)
            // 2) Run source files that are selected using ILibraryImporter
            //

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Importing Audio Files",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = true
            };
            var progressCounter = 0;
            var progressTotal = this.SourceFiles.Where(x => x.IsSelected).Count();

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var file in this.SourceFiles.Where(x => x.IsSelected))
            {
                loadingViewModel.Message = "Importing " + file.FileName;

                // -> Import to Database
                var success = _libraryImporter.WorkImportEntity(file.ImportLoad, file.ImportOutput);

                if (!success)
                    file.ImportOutput.LogMessages.Add("Import Failed (progress halted)");
                else
                    file.ImportOutput.LogMessages.Add("Import Succeeded!");

                // -> Migrate File
                if (file.ImportLoad.ImportFileMigration && success)
                {
                    if (!_libraryImporter.CanImportMigrateFile(file.ImportLoad, file.ImportOutput))
                        file.ImportOutput.LogMessages.Add("File Migration Not Possible (progress halted)");

                    else
                    {
                        var migrationSuccess = _libraryImporter.WorkMigrateFile(file.ImportLoad, file.ImportOutput);

                        if (!migrationSuccess)
                            file.ImportOutput.LogMessages.Add("File Migration Failed (progress halted)");
                        else
                            file.ImportOutput.LogMessages.Add("File Migration Succeeded!");
                    }
                }

                loadingViewModel.Progress = (++progressCounter) / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        private async Task RunAcoustID()
        {
            // Procedure
            //
            // 0) Show Dialog (progress handler)
            // 1) CanRunAcoustID (already checked)
            // 2) Run source files that are selected using ILibraryImporter
            //

            var progressCounter = 0;
            var progressTotal = this.SourceFiles.Where(x => x.IsSelected).Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running AcoustID Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var selectedFile in this.SourceFiles.Where(x => x.IsSelected))
            {
                // Double Check (existing results)
                if (!selectedFile.ImportOutput.AcoustIDSuccess)
                {
                    loadingViewModel.Message = "AcoustID: " + selectedFile.FileName;

                    var success = await _libraryImporter.WorkAcoustID(selectedFile.ImportLoad, selectedFile.ImportOutput);

                    if (!success)
                        selectedFile.ImportOutput.LogMessages.Add("AcoustID Failed (progress halted)");
                    else
                    {
                        selectedFile.ImportOutput.LogMessages.Add("AcoustID Succeeded!");

                        // Set initial selection
                        selectedFile.SelectedAcoustIDResult = selectedFile.ImportOutput.AcoustIDResults.First();
                    }
                }

                loadingViewModel.Progress = (++progressCounter) / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        private async Task RunMusicBrainz()
        {
            // Procedure
            //
            // 0) Show Dialog (progress handler)
            // 1) CanRunMusicBrainz (already checked)
            // 2) Run source files that are selected using ILibraryImporter
            //
            var progressCounter = 0;
            var progressTotal = this.SourceFiles.Where(x => x.IsSelected).Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running Music Brainz Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var selectedFile in this.SourceFiles.Where(x => x.IsSelected))
            {
                // Double Check (existing records)
                //
                if (!selectedFile.ImportOutput.MusicBrainzRecordingMatchSuccess)
                {
                    loadingViewModel.Message = "Music Brainz Lookup:  " + selectedFile.FileName;

                    var success = await _libraryImporter.WorkMusicBrainzDetail(selectedFile.ImportLoad, selectedFile.ImportOutput);

                    if (!success)
                        selectedFile.ImportOutput.LogMessages.Add("Music Brainz Failed (progress halted)");
                    else
                    {
                        selectedFile.ImportOutput.LogMessages.Add("Music Brainz Succeeded!");

                        // Set initial selection
                        selectedFile.SelectedMusicBrainzRecordingMatch = selectedFile.ImportOutput.MusicBrainzRecordingMatches.First();
                    }
                }

                loadingViewModel.Progress = (++progressCounter) / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        private void ShowAcoustIDResults(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Format AcoustID Output
            string format = "Id={0}\nScore={1:P2}\nMusic Brainz Id={2}";

            var oldSelection = selectedFile.SelectedAcoustIDResult;
            var dialogViewModel = new DialogSelectionListViewModel()
            {
                SelectionMode = SelectionMode.Single,
                SelectionList = new NotifyingObservableCollection<SelectionViewModel>(
                                    selectedFile.ImportOutput
                                                .AcoustIDResults
                                                .Select(x => new SelectionViewModel(x, string.Format(format, x.Id, x.Score, x.MusicBrainzRecordingId),
                                                                                       x == selectedFile.SelectedAcoustIDResult)))
            };

            // Show Dialog (MODAL)
            _dialogController.ShowDialogWindowSync(new DialogEventData("Acoust ID Results (Min Score = 30%)", dialogViewModel));

            // Take Selection
            selectedFile.SelectedAcoustIDResult = (LookupResultViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;

            if (selectedFile.SelectedAcoustIDResult != oldSelection)
                selectedFile.SelectedMusicBrainzRecordingMatch = null;
        }

        private void ShowMusicBrainzResults(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Format Music Brainz Output
            string format = "Id={0}\nArtist={1}\nAlbum={2}\nTrack={3}";

            var zippedCollections = selectedFile.ImportOutput
                                                .AcoustIDResults
                                                .Zip(selectedFile.ImportOutput.MusicBrainzRecordingMatches);

            var dialogViewModel = new DialogSelectionListViewModel()
            {
                SelectionMode = SelectionMode.Single,
                SelectionList = new NotifyingObservableCollection<SelectionViewModel>(
                                    zippedCollections
                                        .Select(x => x.Second)
                                        .Select(x => new SelectionViewModel(x, string.Format(format, x.Id,
                                                                                                    x.ArtistCredit?.FirstOrDefault()?.Name ?? string.Empty,
                                                                                                    x.Releases?.FirstOrDefault()?.Title ?? string.Empty,
                                                                                                    x.Title ?? string.Empty),
                                                                               x == selectedFile.SelectedMusicBrainzRecordingMatch)))
            };

            // Show Dialog (MODAL)
            _dialogController.ShowDialogWindowSync(new DialogEventData("Music Brainz Results", dialogViewModel));

            // Take Selection
            var result = (MusicBrainzRecordingViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;
            var acoustIDResult = zippedCollections.Where(x => x.Second == result).Select(z => z.First).Single();

            // Select Both Records
            selectedFile.SelectedMusicBrainzRecordingMatch = result;
            selectedFile.SelectedAcoustIDResult = acoustIDResult;
        }

        private void ShowSmallAudioPlayer(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Small Audio Player:  This follows the dialog pattern; but is self-dismissing!
            //

            var tagFile = _tagCacheController.Get(selectedFile.FileFullPath);

            var dialogViewModel = new DialogSmallAudioPlayerViewModel()
            {
                Album = tagFile.Album,
                Artist = tagFile.AlbumArtist,
                CurrentTime = TimeSpan.Zero,
                CurrentTimeRatio = 0,
                Duration = tagFile.Duration,
                FileName = selectedFile.FileFullPath,
                PlayState = PlayStopPause.Stop,
                SourceType = StreamSourceType.File,
                Track = tagFile.Title
            };

            // Show Dialog (starts on load)
            _dialogController.ShowDialogWindowSync(new DialogEventData(selectedFile.FileName, dialogViewModel));
        }
    }
}
