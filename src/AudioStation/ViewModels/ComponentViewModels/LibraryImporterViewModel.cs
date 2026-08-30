using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;
using AudioStation.ViewModels.TagViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.ATLViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;
using SimpleWpf.ViewModel;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryImporterViewModel : ComponentViewModelBase<LibraryImporterTreeViewModel>
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IDialogController _dialogController;
        private readonly IIocEventAggregator _eventAggregator;
        private readonly ILibraryImporter _libraryImporter;
        private readonly ITagCacheController _tagCacheController;

        LibraryImporterConfigurationViewModel _options;

        // Import Source Directory
        //
        LibraryImporterTreeViewModel _sourceDirectory;

        // Staged Files:  These will keep changes to the tag in memory until the tag data is saved (to
        //                the same file in the source direcotry. An "import" is complete when the file 
        //                finished - with the bare minimum tag data - and moved into the library's 
        //                directory structure.
        //
        ObservableCollection<LibraryImporterFileViewModel> _stagedFiles;
        ObservableCollection<LibraryImporterFileViewModel> _acoustIDCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _musicBrainzCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _filesReadyToImport;
        ObservableCollection<LibraryImporterFileViewModel> _filesCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _filesCompletedWithError;

        SimpleCommand _stageCommand;
        SimpleCommand _unstageCommand;
        SimpleCommand _editTagCommand;
        SimpleCommand<string> _editTagGroupCommand;
        SimpleCommand _runImportCommand;
        //SimpleCommand _runChromaprintLookupCommand;

        string _sourceFolderSearch;
        string _stagedSearch;

        public LibraryImporterConfigurationViewModel Options
        {
            get { return _options; }
            set { RaiseAndSetIfChanged(ref _options, value); }
        }
        public LibraryImporterTreeViewModel SourceDirectory
        {
            get { return _sourceDirectory; }
            set { RaiseAndSetIfChanged(ref _sourceDirectory, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> StagedFiles
        {
            get { return _stagedFiles; }
            set { this.RaiseAndSetIfChanged(ref _stagedFiles, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> AcoustIDCompletedSuccessfully
        {
            get { return _acoustIDCompletedSuccessfully; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDCompletedSuccessfully, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> MusicBrainzCompletedSuccessfully
        {
            get { return _musicBrainzCompletedSuccessfully; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzCompletedSuccessfully, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> FilesReadyToImport
        {
            get { return _filesReadyToImport; }
            set { this.RaiseAndSetIfChanged(ref _filesReadyToImport, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> FilesCompletedSuccessfully
        {
            get { return _filesCompletedSuccessfully; }
            set { this.RaiseAndSetIfChanged(ref _filesCompletedSuccessfully, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> FilesCompletedWithError
        {
            get { return _filesCompletedWithError; }
            set { this.RaiseAndSetIfChanged(ref _filesCompletedWithError, value); }
        }
        public int SourceFileSelectedCount
        {
            get { return _sourceDirectory == null ? 0 : _sourceDirectory.RecursiveCount(x => !x.IsDirectory && x.IsSelected); }
            set { OnPropertyChanged("SourceFileSelectedCount"); }
        }
        public int SourceFileCount
        {
            get { return _sourceDirectory == null ? 0 : _sourceDirectory.RecursiveCount(x => !x.IsDirectory); }
            set { OnPropertyChanged("SourceFileCount"); }
        }
        public SimpleCommand StageCommand
        {
            get { return _stageCommand; }
            set { this.RaiseAndSetIfChanged(ref _stageCommand, value); }
        }
        public SimpleCommand UnstageCommand
        {
            get { return _unstageCommand; }
            set { this.RaiseAndSetIfChanged(ref _unstageCommand, value); }
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

        public string SourceFolderSearch
        {
            get { return _sourceFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolderSearch, value); }
        }
        public string StagedSearch
        {
            get { return _stagedSearch; }
            set { this.RaiseAndSetIfChanged(ref _stagedSearch, value); }
        }

        public override LibraryImporterTreeViewModel Load
        {
            get { return this.SourceDirectory; }
        }

        [IocImportingConstructor]
        public LibraryImporterViewModel(IConfigurationManager configurationManager,
                                        IAudioStationMapper audioStationMapper,
                                        IDialogController dialogController,
                                        IIocEventAggregator eventAggregator,
                                        ILibraryImporter libraryImporter,
                                        ITagCacheController tagCacheController)
        {
            _configurationManager = configurationManager;
            _audioStationMapper = audioStationMapper;
            _dialogController = dialogController;
            _libraryImporter = libraryImporter;
            _eventAggregator = eventAggregator;
            _tagCacheController = tagCacheController;

            var configuration = configurationManager.GetConfiguration();

            this.Options = new LibraryImporterConfigurationViewModel(configurationManager, dialogController);
            this.SourceDirectory = null;

            _stagedFiles = new ObservableCollection<LibraryImporterFileViewModel>();
            _acoustIDCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _musicBrainzCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesReadyToImport = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesCompletedWithError = new ObservableCollection<LibraryImporterFileViewModel>();

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditTagGroup, CanEditTagGroup);
            this.StageCommand = new SimpleCommand(StageFiles, CanStageFiles);
            this.UnstageCommand = new SimpleCommand(UnstageFiles, CanUnstageFiles);
        }

        public override void Initialize(IAudioStationConfiguration configuration, LibraryImporterTreeViewModel load, DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(Initialize, System.Windows.Threading.DispatcherPriority.Background, configuration, load, progressHandler);

            else
            {
                //// Set View Model (Load)
                //this.SourceDirectory = load;

                //// Initialization:     This task is run during initialization.
                //// 
                //// Task / Dispatcher:  We have to invoke the dispatcher from here so that the view model
                ////                     bindings to the UI don't throw exceptions.
                ////
                //// Hook Events (Recursively)
                //foreach (var sourceFile in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory)
                //                                               .Cast<LibraryImporterFileViewModel>())
                //{
                //    sourceFile.SelectAcoustIDEvent += ShowAcoustIDResults;
                //    sourceFile.SelectMusicBrainzEvent += ShowMusicBrainzResults;
                //    sourceFile.PlayAudioEvent += ShowSmallAudioPlayer;
                //    //sourceFile.PropertyChanged += SourceFile_PropertyChanged;
                //}

                //// Set View Model
                //this.SourceDirectory.ItemPropertyChanged += SourceDirectory_ItemPropertyChanged;
            }
        }
        public override void Dispose()
        {
            ClearSourceFiles();
        }

        private bool CanUnstageFiles()
        {
            // TODO: Performance
            return this.StagedFiles.Any();
        }
        private bool CanStageFiles()
        {
            // TODO: Performance
            return this.SourceDirectory
                       .RecursiveWhere(x => x.IsSelected)
                       .Any();
        }
        private bool CanEditTag()
        {
            return this.SourceFileSelectedCount == 1;
        }
        private bool CanEditTagGroup(string fieldName)
        {
            return this.SourceFileSelectedCount > 1;
        }

        private void SourceDirectory_ItemPropertyChanged(PathViewModel item, PropertyChangedEventArgs propertyArgs)
        {
            SourceTreeNotify();
        }
        private void SourceFile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SourceTreeNotify();
        }

        private void SourceTreeNotify()
        {
            OnPropertyChanged("SourceFileSelectedCount");
            OnPropertyChanged("SourceFileCount");

            this.StageCommand.RaiseCanExecuteChanged();
            this.UnstageCommand.RaiseCanExecuteChanged();
            this.EditTagCommand.RaiseCanExecuteChanged();
            this.EditTagGroupCommand.RaiseCanExecuteChanged(string.Empty);
        }

        private void ClearSourceFiles()
        {
            // Un-Hook Events (Recursively)
            foreach (var file in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory).Cast<LibraryImporterFileViewModel>())
            {
                file.PlayAudioEvent -= ShowSmallAudioPlayer;
                file.SelectAcoustIDEvent -= ShowAcoustIDResults;
                file.SelectMusicBrainzEvent -= ShowMusicBrainzResults;
                file.PropertyChanged -= SourceFile_PropertyChanged;
            }
            // Nodes have list properties
            this.SourceDirectory.ItemPropertyChanged -= SourceDirectory_ItemPropertyChanged;
        }

        private void StageFiles()
        {
            // Initialization (?)
            if (this.SourceDirectory == null)
                return;

            this.SourceDirectory.RecurseForEach(path =>
            {
                var pathNode = path as LibraryImporterTreeViewModel;

                if (pathNode.HasSelectedParent() ||
                    pathNode.NodeValue.IsSelected)
                {
                    // File
                    if (!pathNode.NodeValue.IsDirectory &&
                        !this.StagedFiles.Any(x => x.FullPath == pathNode.NodeValue.FullPath))
                    {
                        this.StagedFiles.Add(path.NodeValue as LibraryImporterFileViewModel);
                    }

                    // Directory
                    else
                    {
                        // Nothing to do
                    }
                }
            });
        }

        private void UnstageFiles()
        {
            // Initialization (?)
            if (this.SourceDirectory == null)
                return;

            // Remove unstaged files
            var removedFiles = this.StagedFiles.Remove(x => x.IsSelected);

            // Unhook
            foreach (var file in removedFiles)
            {
                file.PropertyChanged -= SourceFile_PropertyChanged;
            }
        }

        private void EditTag()
        {
            var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
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

                // Map tag to view model
                var viewModel = _audioStationMapper.Map<IAudioStationTag, TagViewModel>(tag);

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
                ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private void EditTagGroup(string fieldName)
        {
            var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
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
                ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private void ShowAcoustIDResults(LibraryImporterFileViewModel selectedFile)
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
            selectedFile.SelectedAcoustIDResult = (AcoustIDLookupResultViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;

            if (selectedFile.SelectedAcoustIDResult != oldSelection)
                selectedFile.SelectedMusicBrainzRecordingMatch = null;
        }

        private void ShowMusicBrainzResults(LibraryImporterFileViewModel selectedFile)
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
                                        .Select(x => new SelectionViewModel(x, string.Format(format, x,
                                                                                                    x.AlbumArtist,
                                                                                                    x.Album,
                                                                                                    x.Title ?? string.Empty),
                                                                               x == selectedFile.SelectedMusicBrainzRecordingMatch)))
            };

            // Show Dialog (MODAL)
            _dialogController.ShowDialogWindowSync(new DialogEventData("Music Brainz Results", dialogViewModel));

            // Take Selection
            var result = (TagSmallViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;
            var acoustIDResult = zippedCollections.Where(x => x.Second == result).Select(z => z.First).Single();

            // Select Both Records
            selectedFile.SelectedMusicBrainzRecordingMatch = result;
            selectedFile.SelectedAcoustIDResult = acoustIDResult;
        }

        private void ShowSmallAudioPlayer(LibraryImporterFileViewModel selectedFile)
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
