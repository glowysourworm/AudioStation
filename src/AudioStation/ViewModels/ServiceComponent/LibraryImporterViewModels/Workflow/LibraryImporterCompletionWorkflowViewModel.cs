using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ServiceComponentPartViewModelBase
    {
        private readonly IDialogController _dialogController;

        // Staged Files (carries the import load / output)
        private readonly LibraryImporterStagedFileCollection _stagedFiles;
        LibraryImporterStagedFileFilterType _stagedFileFilterType;

        SimpleCommand _editTagCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand<string> _editTagGroupCommand;

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

            _stagedFiles = stagedFiles;
            _stagedFiles.SelectionChanged += UpdateCommands;

            this.EditTagCommand = new SimpleCommand(EditTag, CanEditTag);
            this.EditTagGroupCommand = new SimpleCommand<string>(EditSelectedTagsField, CanEditSelectedTagsField);
            this.PlayAudioCommand = new SimpleCommand(PlayAudio, CanPlayAudio);
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
            return !_dialogController.IsShowing() && _stagedFiles.SelectedFiles.Count == 1;
        }
        private bool CanEditSelectedTagsField(string fieldName)
        {
            return !_dialogController.IsShowing() && _stagedFiles.Any(x => x.IsSelected);
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
        private void UpdateCommands()
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
