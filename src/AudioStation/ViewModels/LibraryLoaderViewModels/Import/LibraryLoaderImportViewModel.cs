using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Component.Interface;
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

using NAudio.Lame;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.ViewModel;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    public class LibraryLoaderImportViewModel : PrimaryViewModelBase
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IDialogController _dialogController;
        private readonly IIocEventAggregator _eventAggregator;
        private readonly ILibraryImporter _libraryImporter;
        private readonly ITagCacheController _tagCacheController;
        private readonly IViewModelLoader _viewModelLoader;

        LibraryLoaderImportOptionsViewModel _options;

        LibraryLoaderImportTreeViewModel _sourceDirectory;

        SimpleCommand _editOptionsCommand;
        SimpleCommand _editTagCommand;
        SimpleCommand<string> _editTagGroupCommand;
        SimpleCommand _runImportCommand;
        SimpleCommand _runChromaprintLookupCommand;

        public LibraryLoaderImportOptionsViewModel Options
        {
            get { return _options; }
            set { RaiseAndSetIfChanged(ref _options, value); }
        }
        public LibraryLoaderImportTreeViewModel SourceDirectory
        {
            get { return _sourceDirectory; }
            set { RaiseAndSetIfChanged(ref _sourceDirectory, value); }
        }
        public int SourceFileSelectedCount
        {
            get { return _sourceDirectory == null ? 0 : _sourceDirectory.RecursiveCount(x => !x.IsDirectory && x.IsSelected); }
            set { OnPropertyChanged("SourceFileSelectedCount"); }
        }
        public SimpleCommand EditOptionsCommand
        {
            get { return _editOptionsCommand; }
            set { RaiseAndSetIfChanged(ref _editOptionsCommand, value); }
        }
        public SimpleCommand EditTagCommand
        {
            get { return _editTagCommand; }
            set { RaiseAndSetIfChanged(ref _editTagCommand, value); }
        }
        public SimpleCommand<string> EditTagGroupCommand
        {
            get { return _editTagGroupCommand; }
            set { RaiseAndSetIfChanged(ref _editTagGroupCommand, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }
        public SimpleCommand RunAcousticFingerprintCommand
        {
            get { return _runChromaprintLookupCommand; }
            set { RaiseAndSetIfChanged(ref _runChromaprintLookupCommand, value); }
        }

        public LibraryLoaderImportViewModel(IConfigurationManager configurationManager,
                                            IDialogController dialogController,
                                            IIocEventAggregator eventAggregator,
                                            ILibraryImporter libraryImporter,
                                            ITagCacheController tagCacheController,
                                            IViewModelLoader viewModelLoader)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;
            _libraryImporter = libraryImporter;
            _eventAggregator = eventAggregator;
            _tagCacheController = tagCacheController;
            _viewModelLoader = viewModelLoader;

            var configuration = configurationManager.GetConfiguration();

            this.Options = new LibraryLoaderImportOptionsViewModel(configurationManager, dialogController);
            this.SourceDirectory = null;

            // RunImport -> Complete
            //eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(payload =>
            //{
            //    RefreshImportFiles(true);
            //});

            this.EditTagCommand = new SimpleCommand(() =>
            {
                EditTag();

            }, CanEditTag);

            this.EditTagGroupCommand = new SimpleCommand<string>((fieldName) =>
            {
                EditTagGroup(fieldName);

            }, CanEditTagGroup);

            this.RunImportCommand = new SimpleCommand(async () =>
            {
                await RunImport();

            }, CanRunImport);

            this.RunAcousticFingerprintCommand = new SimpleCommand(async () =>
            {
                await RunAcoustID();

                if (CanRunMusicBrainz())
                    await RunMusicBrainz();

            }, CanRunAcoustID);

            this.EditOptionsCommand = new SimpleCommand(async () =>
            {
                // Synchronous
                dialogController.ShowImportOptionsWindow(this.Options);

                // Show Loading
                var loadingViewModel = new DialogLoadingViewModel()
                {
                    Message = "Loading Import Files",
                    Progress = 0,
                    ShowProgressBar = true
                };

                eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

                await RefreshImportFiles((count,current, errorCount, message) => 
                {
                    loadingViewModel.Message = message;
                    loadingViewModel.Progress = current / (double)count;
                });

                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
            });
        }

        public override async Task Initialize(DialogProgressHandler progressHandler)
        {
            await RefreshImportFiles(progressHandler);
        }
        public override void Dispose()
        {
            ClearSourceFiles();
        }

        private bool CanEditTag()
        {
            return this.SourceFileSelectedCount == 1;
        }
        private bool CanEditTagGroup(string fieldName)
        {
            return this.SourceFileSelectedCount > 1;
        }
        private bool CanRunImport()
        {
            return this.SourceFileSelectedCount > 0 &&
                   this.SourceDirectory
                       .RecursiveWhere(x => x.IsSelected && !x.IsDirectory)
                       .Cast<LibraryLoaderImportFileViewModel>()
                       .All(x => x.MinimumImportValid);
        }
        private bool CanRunAcoustID()
        {
            return this.SourceFileSelectedCount > 0;
        }
        private bool CanRunMusicBrainz()
        {
            return this.SourceFileSelectedCount > 0 &&
                   this.SourceDirectory
                       .RecursiveWhere(x => x.IsSelected && !x.IsDirectory)
                       .Cast<LibraryLoaderImportFileViewModel>()
                       .All(x => x.ImportOutput.AcoustIDSuccess && x.SelectedAcoustIDResult != null);
        }

        private void SourceDirectory_ItemPropertyChanged(PathViewModel item, PropertyChangedEventArgs eventArgs)
        {
            OnPropertyChanged("SourceFileSelectedCount");

            this.EditOptionsCommand.RaiseCanExecuteChanged();
            this.EditTagCommand.RaiseCanExecuteChanged();
            this.EditTagGroupCommand.RaiseCanExecuteChanged(string.Empty);
            this.RunImportCommand.RaiseCanExecuteChanged();
            this.RunAcousticFingerprintCommand.RaiseCanExecuteChanged();
        }

        private async Task RefreshImportFiles(DialogProgressHandler progressHandler)
        {
            // Initialization:     This task is run during initialization.
            // 
            // Task / Dispatcher:  We have to invoke the dispatcher from here so that the view model
            //                     bindings to the UI don't throw exceptions.
            //
            await ApplicationHelpers.BeginInvokeDispatcherAsync(async () =>
            {
                if (!string.IsNullOrEmpty(this.Options.SourceFolder))
                {
                    // IViewModelLoader -> LoadImportFiles -> LibraryLoaderImportFileViewModel(...)
                    //
                    var importDirectory = _viewModelLoader.LoadImportFiles(this.Options, progressHandler);

                    if (importDirectory == null)
                        return;

                    // Hook Events (Recursively)
                    foreach (var sourceFile in importDirectory.RecursiveWhere(x => !x.IsDirectory).Cast<LibraryLoaderImportFileViewModel>())
                    {
                        sourceFile.SelectAcoustIDEvent += ShowAcoustIDResults;
                        sourceFile.SelectMusicBrainzEvent += ShowMusicBrainzResults;
                        sourceFile.PlayAudioEvent += ShowSmallAudioPlayer;
                    }

                    // Set View Model
                    this.SourceDirectory = importDirectory;
                    this.SourceDirectory.ItemPropertyChanged += SourceDirectory_ItemPropertyChanged;
                }

            }, DispatcherPriority.Background);
        }

        private void ClearSourceFiles()
        {
            // Un-Hook Events (Recursively)
            foreach (var file in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory).Cast<LibraryLoaderImportFileViewModel>())
            {
                file.PlayAudioEvent -= ShowSmallAudioPlayer;
                file.SelectAcoustIDEvent -= ShowAcoustIDResults;
                file.SelectMusicBrainzEvent -= ShowMusicBrainzResults;
            }
        }

        private void EditTag()
        {
            var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryLoaderImportFileViewModel>().ToList();
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
                var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (" + firstFile.ShortPath + ")", DialogEditorView.TagView, viewModel));

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

        private void EditTagGroup(string fieldName)
        {
            var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryLoaderImportFileViewModel>().ToList();
            var firstFile = inputFiles.FirstOrDefault();

            if (firstFile == null)
                return;

            // Base the tag view model on the first input. Then, build a group tag
            // from there.
            //
            try
            {
                // Create view model for editing
                var viewModel = new DialogTagFieldEditorViewModel()
                {
                    TagFieldName = fieldName
                };

                // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
                var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (Group)", DialogEditorView.TagFieldView, viewModel));

                // User wishes to save the data
                if (dialogResult)
                {
                    foreach (var file in inputFiles)
                    {
                        // Update Import File:  Group Fields Only (view model) (still in new memory only)
                        file.SaveTagFieldEdit(fieldName, viewModel.Tag);
                    }
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
            var progressTotal = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Count();

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var file in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryLoaderImportFileViewModel>())
            {
                loadingViewModel.Message = "Importing " + file.ShortPath;

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

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
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
            var progressTotal = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running AcoustID Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var selectedFile in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryLoaderImportFileViewModel>())
            {
                // Double Check (existing results)
                if (!selectedFile.ImportOutput.AcoustIDSuccess)
                {
                    loadingViewModel.Message = "AcoustID: " + selectedFile.ShortPath;

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

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
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
            var progressTotal = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Count();

            var loadingViewModel = new DialogLoadingViewModel()
            {
                Title = "Running Music Brainz Service",
                Message = string.Empty,
                Progress = 0,
                ShowProgressBar = progressTotal > 1
            };

            // Show Loading...
            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(loadingViewModel));

            foreach (var selectedFile in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryLoaderImportFileViewModel>())
            {
                // Double Check (existing records)
                //
                if (!selectedFile.ImportOutput.MusicBrainzRecordingMatchSuccess)
                {
                    loadingViewModel.Message = "Music Brainz Lookup:  " + selectedFile.ShortPath;

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

                loadingViewModel.Progress = ++progressCounter / (double)progressTotal;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        private void ShowAcoustIDResults(LibraryLoaderImportFileViewModel selectedFile)
        {
            // Format AcoustID Output
            var format = "Id={0}\nScore={1:P2}\nMusic Brainz Id={2}";

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
            var format = "Id={0}\nArtist={1}\nAlbum={2}\nTrack={3}";

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

            var tagFile = _tagCacheController.Get(selectedFile.FullPath);

            var dialogViewModel = new DialogSmallAudioPlayerViewModel()
            {
                Album = tagFile.Album,
                Artist = tagFile.AlbumArtist,
                CurrentTime = TimeSpan.Zero,
                CurrentTimeRatio = 0,
                Duration = tagFile.Duration,
                FileName = selectedFile.FullPath,
                PlayState = PlayStopPause.Stop,
                SourceType = StreamSourceType.File,
                Track = tagFile.Title
            };

            // Show Dialog (starts on load)
            _dialogController.ShowDialogWindowSync(new DialogEventData(selectedFile.ShortPath, dialogViewModel));
        }
    }
}
