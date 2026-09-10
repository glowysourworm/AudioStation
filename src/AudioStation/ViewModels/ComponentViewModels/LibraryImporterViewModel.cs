using System.Collections.ObjectModel;
using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow;
using AudioStation.ViewModels.MainViewModels;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.FileTreeView;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public class LibraryImporterViewModel : ComponentViewModelBase
    {
        // TODO: Try (Initialize, Load, Execute, and Save) for the component view model base
        //       to clean up this view model issue. We need components injected for other
        //       functions.
        private ILibraryLoaderService _libraryLoaderService;

        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IDialogController _dialogController;
        private readonly ITagCacheController _tagCacheController;



        // Configuration:  This is for the partial configuration editing control area for library directories.
        //
        AudioStationConfigurationViewModel _configuration;
        ObservableCollection<AudioEncoderViewModel> _encoders;

        // Workflow View Models
        LibraryImporterWorkflowViewModel _workflow;
        LibraryImporterLoaderViewModel _loader;
        LibraryImporterStagingViewModel _staging;

        // Saved Workflow(s)
        ObservableCollection<LibraryImporterWorkflowViewModel> _savedWorkflows;

        ObservableCollection<LibraryImporterFileViewModel> _acoustIDCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _musicBrainzCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _filesReadyToImport;
        ObservableCollection<LibraryImporterFileViewModel> _filesCompletedSuccessfully;
        ObservableCollection<LibraryImporterFileViewModel> _filesCompletedWithError;

        SimpleCommand _editTagCommand;
        SimpleCommand<string> _editTagGroupCommand;
        SimpleCommand _runImportCommand;
        //SimpleCommand _runChromaprintLookupCommand;

        string _sourceFolderSearch;
        string _stagedSearch;

        public AudioStationConfigurationViewModel Configuration
        {
            get { return _configuration; }
            set { this.RaiseAndSetIfChanged(ref _configuration, value); }
        }
        public ObservableCollection<AudioEncoderViewModel> Encoders
        {
            get { return _encoders; }
            set { this.RaiseAndSetIfChanged(ref _encoders, value); }
        }
        public LibraryImporterWorkflowViewModel Workflow
        {
            get { return _workflow; }
            set
            {
                RaiseAndSetIfChanged(ref _workflow, value);

                // Also, have to set component parts
                if (this.Loader != null)
                    this.Loader.Workflow = value;

                if (this.Staging != null)
                    this.Staging.Workflow = value;
            }
        }
        public LibraryImporterLoaderViewModel Loader
        {
            get { return _loader; }
            set { this.RaiseAndSetIfChanged(ref _loader, value); }
        }
        public LibraryImporterStagingViewModel Staging
        {
            get { return _staging; }
            set { this.RaiseAndSetIfChanged(ref _staging, value); }
        }
        public ObservableCollection<LibraryImporterWorkflowViewModel> SavedWorkflows
        {
            get { return _savedWorkflows; }
            set { this.RaiseAndSetIfChanged(ref _savedWorkflows, value); }
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

        public LibraryImporterViewModel(IAudioStationMapper audioStationMapper,
                                        IAudioConverter audioConverter,
                                        IDialogController dialogController,
                                        IIocEventAggregator eventAggregator,
                                        ITagCacheController tagCacheController) : base("Library Importer")
        {
            _audioStationMapper = audioStationMapper;
            _dialogController = dialogController;
            _tagCacheController = tagCacheController;

            this.Workflow = new LibraryImporterWorkflowViewModel()
            {
                Name = "New Workflow"
            };
            this.Loader = new LibraryImporterLoaderViewModel(eventAggregator, audioConverter, this.Workflow);
            this.Staging = new LibraryImporterStagingViewModel(eventAggregator, this.Workflow);

            this.SavedWorkflows = new ObservableCollection<LibraryImporterWorkflowViewModel>();

            this.Loader.PropertyChanged += OnImportStepUpdate;
            this.Staging.PropertyChanged += OnImportStepUpdate;

            _acoustIDCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _musicBrainzCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesReadyToImport = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesCompletedSuccessfully = new ObservableCollection<LibraryImporterFileViewModel>();
            _filesCompletedWithError = new ObservableCollection<LibraryImporterFileViewModel>();

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditTagGroup, CanEditTagGroup);
        }

        /// <summary>
        /// Saves current workflow changes
        /// </summary>
        public void SaveCurrentWorkflow()
        {
            // EXCEPTION (TODO)
            _libraryLoaderService.AddOrUpdateImportWorkflow(this.Workflow);
        }

        private void OnImportStepUpdate(object? sender, PropertyChangedEventArgs e)
        {
            this.Loading = this.Loader.Loading || this.Staging.Loading;
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            // TODO: Try making a new couple of pattern methods for components (Save, and Execute)
            _libraryLoaderService = audioStationController.LibraryLoaderService;

            // Sub-component(s)
            this.Loader.Initialize(configuration, audioStationController, progressHandler);
            this.Staging.Initialize(configuration, audioStationController, progressHandler);
            this.Configuration = audioStationController.ComponentController.GetComponent<AudioStationConfigurationViewModel>();
            this.Encoders = audioStationController.ComponentController.GetComponent<MainViewModel>().Encoders;

            // EXCEPTION (TODO)
            this.SavedWorkflows.AddRange(audioStationController.LibraryLoaderService.GetWorkflows());

            if (this.SavedWorkflows.Any())
                this.Workflow = this.SavedWorkflows.First();

            // Set View Model (Load)
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
        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.Workflow.Configuration.ImportDirectory == null)
                return;

            // Sub-component(s)
            this.Loader.Load(configuration, audioStationController, progressHandler);
            this.Staging.Load(configuration, audioStationController, progressHandler);
        }
        private bool CanUnstageFiles()
        {
            // TODO: Performance
            //return this.StagedFiles.Any();
            return false;
        }
        private bool CanStageFiles()
        {
            return false;
            // TODO: Performance
            //return this.SourceDirectory
            //           .RecursiveWhere(x => x.IsSelected)
            //           .Any();
        }
        private bool CanEditTag()
        {
            //return this.SourceFileSelectedCount == 1;
            return false;
        }
        private bool CanEditTagGroup(string fieldName)
        {
            //return this.SourceFileSelectedCount > 1;
            return false;
        }

        private void SourceDirectory_ItemPropertyChanged(FileTreeNodeViewModel item, PropertyChangedEventArgs propertyArgs)
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

            this.EditTagCommand.RaiseCanExecuteChanged();
            this.EditTagGroupCommand.RaiseCanExecuteChanged(string.Empty);
        }

        private void ClearSourceFiles()
        {
            //// Un-Hook Events (Recursively)
            //foreach (var file in this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory).Cast<LibraryImporterFileViewModel>())
            //{
            //    file.PlayAudioEvent -= ShowSmallAudioPlayer;
            //    file.SelectAcoustIDEvent -= ShowAcoustIDResults;
            //    file.SelectMusicBrainzEvent -= ShowMusicBrainzResults;
            //    file.PropertyChanged -= SourceFile_PropertyChanged;
            //}
            //// Nodes have list properties
            //this.SourceDirectory.ItemPropertyChanged -= SourceDirectory_ItemPropertyChanged;
        }

        private void StageFiles()
        {
            //// Initialization (?)
            //if (this.SourceDirectory == null)
            //    return;

            //this.SourceDirectory.RecurseForEach(path =>
            //{
            //    var pathNode = path as LibraryImporterTreeViewModel;

            //    if (pathNode.HasSelectedParent() ||
            //        pathNode.NodeValue.IsSelected)
            //    {
            //        // File
            //        if (!pathNode.NodeValue.IsDirectory &&
            //            !this.StagedFiles.Any(x => x.FullPath == pathNode.NodeValue.FullPath))
            //        {
            //            this.StagedFiles.Add(path.NodeValue as LibraryImporterFileViewModel);
            //        }

            //        // Directory
            //        else
            //        {
            //            // Nothing to do
            //        }
            //    }
            //});
        }

        private void UnstageFiles()
        {
            //// Initialization (?)
            //if (this.SourceDirectory == null)
            //    return;

            //// Remove unstaged files
            //var removedFiles = this.StagedFiles.Remove(x => x.IsSelected);

            //// Unhook
            //foreach (var file in removedFiles)
            //{
            //    file.PropertyChanged -= SourceFile_PropertyChanged;
            //}
        }

        private void EditTag()
        {
            //var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
            //var firstFile = inputFiles.FirstOrDefault();

            //if (firstFile == null)
            //    return;

            //// Base the tag view model on the first input. Then, build a group tag
            //// from there.
            ////
            //try
            //{
            //    // Get the current working tag
            //    var tag = firstFile.GetTagCopy();

            //    // Map tag to view model
            //    var viewModel = _audioStationMapper.Map<IAudioStationTag, TagViewModel>(tag);

            //    // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
            //    var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (" + firstFile.ShortPath + ")", DialogEditorView.TagView, viewModel));

            //    // User wishes to save the data
            //    if (dialogResult)
            //    {
            //        // Update Import File (view model)(still in new memory only)
            //        firstFile.SaveTagEdit(viewModel);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
            //    throw ex;
            //}
        }

        private void EditTagGroup(string fieldName)
        {
            //var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
            //var firstFile = inputFiles.FirstOrDefault();

            //if (firstFile == null)
            //    return;

            //// Base the tag view model on the first input. Then, build a group tag
            //// from there.
            ////
            //try
            //{
            //    // Create view model for editing
            //    var viewModel = new DialogTagFieldEditorViewModel()
            //    {
            //        TagFieldName = fieldName
            //    };

            //    // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
            //    var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (Group)", DialogEditorView.TagFieldView, viewModel));

            //    // User wishes to save the data
            //    if (dialogResult)
            //    {
            //        foreach (var file in inputFiles)
            //        {
            //            // Update Import File:  Group Fields Only (view model) (still in new memory only)
            //            file.SaveTagFieldEdit(fieldName, viewModel.Tag);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
            //    throw ex;
            //}
        }

        private void ShowAcoustIDResults(LibraryImporterFileViewModel selectedFile)
        {
            //// Format AcoustID Output
            //var format = "Id={0}\nScore={1:P2}\nMusic Brainz Id={2}";

            //var oldSelection = selectedFile.SelectedAcoustIDResult;
            //var dialogViewModel = new DialogSelectionListViewModel()
            //{
            //    SelectionMode = SelectionMode.Single,
            //    SelectionList = new NotifyingObservableCollection<SelectionViewModel>(
            //                        selectedFile.ImportOutput
            //                                    .AcoustIDResults
            //                                    .Select(x => new SelectionViewModel(x, string.Format(format, x.Id, x.Score, x.MusicBrainzRecordingId),
            //                                                                           x == selectedFile.SelectedAcoustIDResult)))
            //};

            //// Show Dialog (MODAL)
            //_dialogController.ShowDialogWindowSync(new DialogEventData("Acoust ID Results (Min Score = 30%)", dialogViewModel));

            //// Take Selection
            //selectedFile.SelectedAcoustIDResult = (AcoustIDLookupResultViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;

            //if (selectedFile.SelectedAcoustIDResult != oldSelection)
            //    selectedFile.SelectedMusicBrainzRecordingMatch = null;
        }

        private void ShowMusicBrainzResults(LibraryImporterFileViewModel selectedFile)
        {
            //// Format Music Brainz Output
            //var format = "Id={0}\nArtist={1}\nAlbum={2}\nTrack={3}";

            //var zippedCollections = selectedFile.ImportOutput
            //                                    .AcoustIDResults
            //                                    .Zip(selectedFile.ImportOutput.MusicBrainzRecordingMatches);

            //var dialogViewModel = new DialogSelectionListViewModel()
            //{
            //    SelectionMode = SelectionMode.Single,
            //    SelectionList = new NotifyingObservableCollection<SelectionViewModel>(
            //                        zippedCollections
            //                            .Select(x => x.Second)
            //                            .Select(x => new SelectionViewModel(x, string.Format(format, x,
            //                                                                                        x.AlbumArtist,
            //                                                                                        x.Album,
            //                                                                                        x.Title ?? string.Empty),
            //                                                                   x == selectedFile.SelectedMusicBrainzRecordingMatch)))
            //};

            //// Show Dialog (MODAL)
            //_dialogController.ShowDialogWindowSync(new DialogEventData("Music Brainz Results", dialogViewModel));

            //// Take Selection
            //var result = (TagSmallViewModel)dialogViewModel.SelectionList.Single(x => x.Selected).Item;
            //var acoustIDResult = zippedCollections.Where(x => x.Second == result).Select(z => z.First).Single();

            //// Select Both Records
            //selectedFile.SelectedMusicBrainzRecordingMatch = result;
            //selectedFile.SelectedAcoustIDResult = acoustIDResult;
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
