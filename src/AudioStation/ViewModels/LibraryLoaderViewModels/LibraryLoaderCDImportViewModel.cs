using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Core.Component.CDPlayer;
using AudioStation.Event;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderCDImportViewModel : ViewModelBase
    {
        private readonly ICDImportService _cdImportService;

        bool _cdPlayerLoaded;
        string _cdPlayerDrive;
        int _discNumber;
        int _discCount;
        string _artist;
        string _album;
        ObservableCollection<LibraryLoaderCDImportTrackViewModel> _tracks;

        SimpleCommand _importCommand;

        public bool CDPlayerLoaded
        {
            get { return _cdPlayerLoaded; }
            set { this.RaiseAndSetIfChanged(ref _cdPlayerLoaded, value); }
        }
        public string CDPlayerDrive
        {
            get { return _cdPlayerDrive; }
            set { this.RaiseAndSetIfChanged(ref _cdPlayerDrive, value); }
        }
        public int DiscNumber
        {
            get { return _discNumber; }
            set { this.RaiseAndSetIfChanged(ref _discNumber, value); }
        }
        public int DiscCount
        {
            get { return _discCount; }
            set { this.RaiseAndSetIfChanged(ref _discCount, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public ObservableCollection<LibraryLoaderCDImportTrackViewModel> Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }

        public SimpleCommand ImportCommand
        {
            get { return _importCommand; }
            set { this.RaiseAndSetIfChanged(ref _importCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderCDImportViewModel(IIocEventAggregator eventAggregator, ICDImportService cdImportService)
        {
            _cdImportService = cdImportService;

            this.CDPlayerLoaded = false;
            this.Artist = string.Empty;
            this.Album = string.Empty;
            this.Tracks = new ObservableCollection<LibraryLoaderCDImportTrackViewModel>();

            // CD-ROM
            eventAggregator.GetEvent<CDPlayerLoadEvent>().Subscribe(OnCDPlayerLoaded);
            eventAggregator.GetEvent<CDPlayerReadEvent>().Subscribe(OnCDPlayerRead);

            this.ImportCommand = new SimpleCommand(async () =>
            {
                foreach (var track in this.Tracks)
                {
                    await cdImportService.ImportTrack(track.Track, this.Artist, this.Album, progress =>
                    {
                        track.Progress = progress;
                    });
                }

            }, () => this.Tracks.Count > 0 && this.CDPlayerLoaded);
        }

        private void OnCDPlayerRead(CDDataReadEventArgs args)
        {

        }
        private void OnCDPlayerLoaded(CDDeviceTracksLoadedEventArgs args)
        {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(x => x.Name == args.Drive.ToString() + ":\\");

            if (driveInfo != null && driveInfo.IsReady)
            {
                this.CDPlayerDrive = driveInfo.VolumeLabel;
                this.CDPlayerLoaded = args.CDDeviceReady;
                this.Tracks.Clear();

                for (int index = 0; index < args.TrackCount; index++)
                {
                    this.Tracks.Add(new LibraryLoaderCDImportTrackViewModel()
                    {
                        Complete = false,
                        Progress = 0,
                        Track = index + 1
                    });
                }
            }
            else
            {
                this.CDPlayerDrive = string.Empty;
                this.CDPlayerLoaded = false;
                this.Tracks.Clear();
            }

            this.ImportCommand.RaiseCanExecuteChanged();
        }
    }
}
