using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ComponentPartViewModelBase
    {
        private readonly IDialogController _dialogController;

        // Workflow Configuration
        private readonly LibraryImporterConfigurationViewModel _workflowConfiguration;

        // Staged Files (carries the import load / output)
        private readonly LibraryImporterStagedFileCollection _stagedFiles;
        LibraryImporterStagedFileFilterType _stagedFileFilterType;

        // Import Worker:  This will require a load of type ILibraryLoaderImportLoad. It operates on
        //                 the current workflow entities; and performs the rest of the import and file
        //                 handling tasks that are needed to complete the import workflow.
        //
        LibraryLoaderImportViewModel _importWorker;


        int _selectedFileCount;

        SimpleCommand _editTagCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand<string> _editTagGroupCommand;

        public LibraryLoaderImportViewModel ImportWorker
        {
            get { return _importWorker; }
            set { this.RaiseAndSetIfChanged(ref _importWorker, value); }
        }
        public LibraryImporterStagedFileCollection StagedFiles
        {
            get { return _stagedFiles; }
        }
        public LibraryImporterStagedFileFilterType StagedFileFilterType
        {
            get { return _stagedFileFilterType; }
            set { this.RaiseAndSetIfChanged(ref _stagedFileFilterType, value); }
        }
        public int SelectedFileCount
        {
            get { return _selectedFileCount; }
            set { this.RaiseAndSetIfChanged(ref _selectedFileCount, value); }
        }

        public SimpleCommand EditTagCommand
        {
            get { return _editTagCommand; }
            set { this.RaiseAndSetIfChanged(ref _editTagCommand, value); }
        }
        public SimpleCommand PlayAudioCommand
        {
            get { return _playAudioCommand; }
            set { this.RaiseAndSetIfChanged(ref _playAudioCommand, value); }
        }
        public SimpleCommand<string> EditTagGroupCommand
        {
            get { return _editTagGroupCommand; }
            set { this.RaiseAndSetIfChanged(ref _editTagGroupCommand, value); }
        }

        public LibraryImporterCompletionWorkflowViewModel(
                LibraryImporterStagedFileCollection stagedFiles,
                LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Importer (completion)")
        {
            _dialogController = IocContainer.Get<IDialogController>();

            _workflowConfiguration = workflowConfiguration;
            _stagedFiles = stagedFiles;
            _stagedFiles.ItemPropertyChanged += OnStagedFilePropertyChanged;

            this.ImportWorker = new LibraryLoaderImportViewModel(workflowConfiguration);

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditSelectedTagsField, CanEditSelectedTagsField);
            this.PlayAudioCommand = new SimpleCommand(PlayAudio, CanPlayAudio);

            this.SelectedFileCount = 0;
        }

        public override bool CanExecute()
        {
            return this.ImportWorker.CanExecute();       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        private bool CanEditTag()
        {
            return !_dialogController.IsShowing() && this.SelectedFileCount == 1;
        }
        private bool CanEditSelectedTagsField(string fieldName)
        {
            return !_dialogController.IsShowing() && _stagedFiles.Any(x => x.IsSelected);
        }
        private bool CanPlayAudio()
        {
            return !_dialogController.IsShowing() && this.SelectedFileCount == 1;
        }

        private void EditSelectedTagsField(string fieldName)
        {
            var viewModel = new DialogSingleTextFieldViewModel();

            if (_dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Field:  " + fieldName, DialogEditorView.SingleTextField, viewModel)) == true)
            {
                if (!string.IsNullOrWhiteSpace(viewModel.Value))
                {
                    foreach (var stagedFile in _stagedFiles.Where(x => x.IsSelected))
                    {
                        stagedFile.TagRecordDirty.SetField(fieldName, viewModel.Value);
                    }
                }
            }
        }
        private void EditTag()
        {
            var stagedFile = _stagedFiles.First(x => x.IsSelected);

            _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Source(s)", DialogEditorView.TagSourceView, stagedFile));
        }
        private void PlayAudio()
        {
            var stagedFile = _stagedFiles.First(x => x.IsSelected);

            _dialogController.ShowDialogWindowSync(new DialogEventData(stagedFile.ShortPath, new DialogSmallAudioPlayerViewModel()
            {
                FileName = stagedFile.FullPath,
                SourceType = StreamSourceType.File,
                Album = stagedFile.Tag.Album ?? "Unknown",
                Artist = stagedFile.Tag.AlbumArtist ?? "Unknown",
                CurrentTime = TimeSpan.Zero,
                CurrentTimeRatio = 0,
                Duration = TimeSpan.FromMilliseconds(stagedFile.Tag.DurationMilliseconds ?? 0),
                PlayState = Core.Component.PlayStopPause.Play,
                Track = (stagedFile.Tag.TrackNumber ?? 0).ToString()
            }));
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker = new LibraryLoaderImportViewModel(_workflowConfiguration);

            this.ImportWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.ImportWorker.Load(this.StagedFiles, configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Execute();
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Reset();
        }

        private void OnWorkerStatusChangeEvent(ILibraryLoaderWorkerViewModel sender)
        {

        }
        private void OnStagedFilePropertyChanged(LibraryImporterFileViewModel item, PropertyChangedEventArgs propertyArgs)
        {
            if (propertyArgs.PropertyName == "IsSelected")
            {
                this.SelectedFileCount = _stagedFiles.Count(x => x.IsSelected);

                this.EditTagCommand.RaiseCanExecuteChanged();
                this.PlayAudioCommand.RaiseCanExecuteChanged();
                this.EditTagGroupCommand.RaiseCanExecuteChanged("");
            }
        }
    }
}
