using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ServiceComponentPartViewModelBase
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

        SimpleCommand _editTagCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand _importCommand;
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
        public SimpleCommand ImportCommand
        {
            get { return _importCommand; }
            set { this.RaiseAndSetIfChanged(ref _importCommand, value); }
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
            _stagedFiles.SelectionChanged += OnStagedFilesSelectionChanged;

            this.ImportWorker = new LibraryLoaderImportViewModel(workflowConfiguration);

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditSelectedTagsField, CanEditSelectedTagsField);
            this.ImportCommand = new SimpleCommand(ImportValidFiles, CanImport);
            this.PlayAudioCommand = new SimpleCommand(PlayAudio, CanPlayAudio);
        }

        public override bool CanExecute()
        {
            return this.ImportWorker.CanExecute();       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }
        public override bool CanLoad()
        {
            return !this.ImportWorker.Loaded && !this.ImportWorker.Working;
        }
        public override bool CanReset()
        {
            return this.ImportWorker.CanReset();
        }

        private bool CanEditTag()
        {
            return !_dialogController.IsShowing() && _stagedFiles.SelectedFiles.Count == 1;
        }
        private bool CanEditSelectedTagsField(string fieldName)
        {
            return !_dialogController.IsShowing() && _stagedFiles.Any(x => x.IsSelected);
        }
        private bool CanImport()
        {
            return !_dialogController.IsShowing() && _stagedFiles.ValidFiles.Any() && !this.ImportWorker.Loaded;
        }
        private bool CanPlayAudio()
        {
            return !_dialogController.IsShowing() && _stagedFiles.SelectedFiles.Count == 1;
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
        private void ImportValidFiles()
        {
            if (this.ImportWorker.Loaded)
                throw new Exception("Must first unload and reload import worker before executing");



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
            // Initialize Component Parts
            this.ImportWorker.Load(this.StagedFiles.ValidFiles, configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Execute();
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Reset();
        }
        private void OnStagedFilesSelectionChanged()
        {
            this.EditTagCommand.RaiseCanExecuteChanged();
            this.PlayAudioCommand.RaiseCanExecuteChanged();
            this.EditTagGroupCommand.RaiseCanExecuteChanged("");
            this.ImportCommand.RaiseCanExecuteChanged();
        }
        public override void Dispose()
        {
            // TODO
        }
    }
}
