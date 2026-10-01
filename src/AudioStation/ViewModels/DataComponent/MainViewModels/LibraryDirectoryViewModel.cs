using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents.DialogEditorEvents;
using AudioStation.ViewModels.LibraryViewModels;

using Newtonsoft.Json;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.DataComponent.MainViewModels
{
    public class LibraryDirectoryViewModel : ViewModelBase, ILibraryDirectory
    {
        string _directory;
        string _directoryLabel;
        string _customFolderFormat;
        string _customFileFormat;
        bool _isPrimary;
        bool _isReadOnly;
        bool _deleteUnusedFolders;
        TrackGroupingType _folderFormatType;
        TrackNamingType _fileFormatType;

        SimpleCommand _openFolderCommand;
        SimpleCommand _editCustomFileFormatCommand;
        SimpleCommand _editCustomFolderFormatCommand;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
        }
        public string DirectoryLabel
        {
            get { return _directoryLabel; }
            set { this.RaiseAndSetIfChanged(ref _directoryLabel, value); }
        }
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set { this.RaiseAndSetIfChanged(ref _isPrimary, value); }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { this.RaiseAndSetIfChanged(ref _isReadOnly, value); }
        }
        public bool DeleteUnusedFolders
        {
            get { return _deleteUnusedFolders; }
            set { this.RaiseAndSetIfChanged(ref _deleteUnusedFolders, value); }
        }
        public TrackGroupingType FolderFormatType
        {
            get { return _folderFormatType; }
            set { this.RaiseAndSetIfChanged(ref _folderFormatType, value); }
        }
        public TrackNamingType FileFormatType
        {
            get { return _fileFormatType; }
            set { this.RaiseAndSetIfChanged(ref _fileFormatType, value); }
        }
        public string CustomFolderFormat
        {
            get { return _customFolderFormat; }
            set { this.RaiseAndSetIfChanged(ref _customFolderFormat, value); }
        }
        public string CustomFileFormat
        {
            get { return _customFileFormat; }
            set { this.RaiseAndSetIfChanged(ref _customFileFormat, value); }
        }

        [JsonIgnore]
        public SimpleCommand OpenFolderCommand
        {
            get { return _openFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _openFolderCommand, value); }
        }

        [JsonIgnore]
        public SimpleCommand EditCustomFileFormatCommand
        {
            get { return _editCustomFileFormatCommand; }
            set { this.RaiseAndSetIfChanged(ref _editCustomFileFormatCommand, value); }
        }

        [JsonIgnore]
        public SimpleCommand EditCustomFolderFormatCommand
        {
            get { return _editCustomFolderFormatCommand; }
            set { this.RaiseAndSetIfChanged(ref _editCustomFolderFormatCommand, value); }
        }

        private TagSmallViewModel GetExample()
        {
            return new TagSmallViewModel()
            {
                Album = "Parachutes",
                AlbumArtist = "Cold Play",
                Genre = "Indie-Rock",
                MediaFormat = "CD",
                MediaNumber = 1,
                MediaTotal = 1,
                Title = "Yellow",
                TrackNumber = 5,
                TrackTotal = 10
            };
        }

        public LibraryDirectoryViewModel()
        {
            var dialogController = IocContainer.Get<IDialogController>();
            var audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            this.Directory = string.Empty;
            this.DirectoryLabel = string.Empty;
            this.FolderFormatType = TrackGroupingType.None;
            this.FileFormatType = TrackNamingType.None;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.CustomFolderFormat = string.Empty;
            this.CustomFileFormat = string.Empty;

            this.OpenFolderCommand = new SimpleCommand(() =>
            {
                this.Directory = dialogController.ShowSelectFolder();
            });

            this.EditCustomFolderFormatCommand = new SimpleCommand(() =>
            {
                var format = MigrationHelpers.GetFormat<ITagSmall>(LibraryFormatUse.Folder);
                var formatViewModel = audioStationMapper.Map<LibraryFormat, LibraryFormatViewModel>(format);
                var exampleTrack = GetExample();

                var viewModel = new DialogNamingGroupingFormatViewModel()
                {
                    Description = "The folder layout of your music library may be determined by a custom format. You can use the field templates below to determine how Audio Station lays out the directory structure of your library folders.",
                    Usage = "To use a particular field, just surround it with single curly brackets:  field -> {field}",
                    ExampleTrack = exampleTrack,
                    Format = formatViewModel,
                    LabelText = "Custom Folder Format",
                    UserOutputDescription = string.Format("Artist='{0}'\r\nAlbum='{1}'\r\nGenre='{2}'\r\nMedia Format='{3}'",
                                                           exampleTrack.AlbumArtist, exampleTrack.Album, exampleTrack.Genre, exampleTrack.MediaFormat)
                };

                if (dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Library Custom Folder Structure", DialogEditorView.NamingGroupingFormatView, viewModel)))
                {
                    this.CustomFolderFormat = viewModel.Result;
                }

            });

            this.EditCustomFileFormatCommand = new SimpleCommand(() =>
            {
                var format = MigrationHelpers.GetFormat<ITagSmall>(LibraryFormatUse.File, LibraryExtraneousFields.FileExtension);
                var formatViewModel = audioStationMapper.Map<LibraryFormat, LibraryFormatViewModel>(format);
                var exampleTrack = GetExample();

                var viewModel = new DialogNamingGroupingFormatViewModel()
                {
                    Description = "The file names of your music library may be determined by a custom format. You can use the field templates below to determine how Audio Station creates file names for your audio library.",
                    Usage = "To use a particular field, just surround it with single curly brackets:  field -> {field}",
                    ExampleTrack = exampleTrack,
                    Format = formatViewModel,
                    LabelText = "Custom File Format",
                    UserOutputDescription = string.Format(
                                            "Artist='{0}'\t\t\tMedia Number={1}\r\n" +
                                            "Album='{2}'\t\tMedia Total={3}\r\n" +
                                            "Title='{4}'\t\t\tMedia Format='{5}'\r\n" +
                                            "Genre='{6}'\t\tTrack Total={7}\r\n" +
                                            "\t\t\t\tTrack Number={8}",
                                            exampleTrack.AlbumArtist, exampleTrack.MediaNumber,
                                            exampleTrack.Album, exampleTrack.MediaTotal,
                                            exampleTrack.Title, exampleTrack.MediaFormat,
                                            exampleTrack.Genre, exampleTrack.TrackTotal,
                                            exampleTrack.TrackNumber)
                };

                if (dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Library Custom File Format", DialogEditorView.NamingGroupingFormatView, viewModel)))
                {
                    this.CustomFileFormat = viewModel.Result;
                }
            });
        }
    }
}
