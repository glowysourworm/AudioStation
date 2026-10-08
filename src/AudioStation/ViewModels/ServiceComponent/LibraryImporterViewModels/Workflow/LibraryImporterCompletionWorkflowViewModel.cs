using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Event.DialogEvents.DialogEditorEvents;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ServiceComponentPartViewModelBase
    {
        private readonly IDialogController _dialogController;

        // Staged Files (carries the import load / output)
        private readonly LibraryImporterFileTreeViewModel _stagedFiles;
        LibraryImporterStagedFileFilterType _stagedFileFilterType;

        // These are forwarded directly from the UI
        IEnumerable<LibraryImporterFileTreeNodeViewModel> _selectedNodes;

        SimpleCommand _editTagCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand<string> _editTagGroupCommand;

        public LibraryImporterFileTreeViewModel StagedFiles
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
        public SimpleCommand<string> EditTagGroupCommand
        {
            get { return _editTagGroupCommand; }
            set { this.RaiseAndSetIfChanged(ref _editTagGroupCommand, value); }
        }

        public LibraryImporterCompletionWorkflowViewModel(
                LibraryImporterFileTreeViewModel stagedFiles,
                LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Importer", "This workflow component will complete the import process")
        {
            _dialogController = IocContainer.Get<IDialogController>();

            _stagedFiles = stagedFiles;

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditSelectedTagsField, CanEditSelectedTagsField);
            this.PlayAudioCommand = new SimpleCommand(PlayAudio, CanPlayAudio);

            _stagedFiles.TreeSelectionChangedEvent += OnStagedFileTreeSelectionChanged;
        }

        public override bool CanExecute()
        {
            return true;
        }
        public override bool CanLoad()
        {
            return !_dialogController.IsShowing() && !this.Loaded;
        }
        public override bool CanReset()
        {
            return !_dialogController.IsShowing() && this.Loaded;
        }
        private bool CanEditTag()
        {
            return !_dialogController.IsShowing() && _selectedNodes.Count(x => !x.IsDirectory) == 1;
        }
        private bool CanEditSelectedTagsField(string fieldName)
        {
            return !_dialogController.IsShowing() && _selectedNodes.Any(x => !x.IsDirectory);
        }
        private bool CanPlayAudio()
        {
            return !_dialogController.IsShowing() && _selectedNodes.Count(x => !x.IsDirectory) == 1;
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
                        stagedFile.ImportFile.TagRecordDirty.SetField(fieldName, viewModel.Value);
                    }
                }
            }
        }
        private void EditTag()
        {
            var stagedFile = _stagedFiles.Where(x => x.IsSelected).First();

            _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Source(s)", DialogEditorView.TagSourceView, stagedFile));
        }
        private void PlayAudio()
        {
            var stagedFile = _stagedFiles.Where(x => x.IsSelected).First();

            _dialogController.ShowDialogWindowSync(new DialogEventData(stagedFile.ShortPath, new DialogSmallAudioPlayerViewModel()
            {
                FileName = stagedFile.FullPath,
                SourceType = StreamSourceType.File,
                Album = stagedFile.ImportFile.Tag.Album ?? "Unknown",
                Artist = stagedFile.ImportFile.Tag.AlbumArtist ?? "Unknown",
                CurrentTime = TimeSpan.Zero,
                CurrentTimeRatio = 0,
                Duration = TimeSpan.FromMilliseconds(stagedFile.ImportFile.Tag.DurationMilliseconds ?? 0),
                PlayState = Core.Component.PlayStopPause.Play,
                Track = (stagedFile.ImportFile.Tag.TrackNumber ?? 0).ToString()
            }));
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.Loaded = true;
        }

        public override void Execute(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
        }

        public override void Reset(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.Loaded = false;
        }
        private void OnStagedFileTreeSelectionChanged(IEnumerable<TreeViewNodeModelBase> selectedNodes)
        {
            _selectedNodes = selectedNodes.Cast<LibraryImporterFileTreeNodeViewModel>();

            UpdateCommands();
        }
        public void UpdateCommands()
        {
            // These are needed for the base class
            this.ExecuteCommand.RaiseCanExecuteChanged();
            this.LoadCommand.RaiseCanExecuteChanged();
            this.ResetCommand.RaiseCanExecuteChanged();

            this.EditTagCommand.RaiseCanExecuteChanged();
            this.PlayAudioCommand.RaiseCanExecuteChanged();
            this.EditTagGroupCommand.RaiseCanExecuteChanged("");
        }
        public override void Dispose()
        {
            //TODO
        }
    }
}
