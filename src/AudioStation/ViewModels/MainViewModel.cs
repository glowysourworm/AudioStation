using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Component.AudioProcessing;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.Controls;
using AudioStation.ViewModels.MainViewModels;
using AudioStation.ViewModels.OtherViewModels;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels;

[IocExportDefault]
public class MainViewModel : ViewModelBase
{
    private readonly IIocEventAggregator _eventAggregator;

    bool _disposed = false;

    #region Backing Fields
    AudioStationConfiguration _configuration;
    bool _loadedFromConfiguration;
    float _volume;
    bool _loading;
    bool _configurationLocked;

    LibraryManagerViewModel _libraryManager;
    StatusViewModel _statusViewModel;
    RadioViewModel _radio;
    LogViewModel _log;
    NowPlayingViewModel _nowPlaying;
    BandcampViewModel _bandcamp;
    LibraryImporterViewModel _libraryImportViewModel;
    LibraryLoaderAcoustIDViewModel _libraryLoaderAcoustID;
    LibraryLoaderCDImportViewModel _libraryLoaderCDImport;
    LibraryLoaderMusicBrainzBasicViewModel _libraryLoaderMusicBrainzBasic;
    LibraryLoaderMusicBrainzAlbumArtViewModel _libraryLoaderMusicBrainzAlbumArt;

    ObservableCollection<float> _equalizerValues;
    ObservableCollection<EqualizerBandViewModel> _equalizerViewModel;
    PlayStopPause _playState;

    SimpleCommand _openLibraryFolderCommand;
    SimpleCommand _openMusicSubFolderCommand;
    SimpleCommand _openAudioBooksSubFolderCommand;
    SimpleCommand _openImportFolderCommand;
    SimpleCommand _openCacheFolderCommand;
    SimpleCommand _openStorageFolderCommand;
    SimpleCommand _saveConfigurationCommand;
    SimpleCommand _loadLibraryCommand;
    SimpleCommand _unlockConfigurationCommand;
    #endregion

    #region Properties
    public AudioStationConfiguration Configuration
    {
        get { return _configuration; }
        set { this.RaiseAndSetIfChanged(ref _configuration, value); }
    }
    public StatusViewModel StatusViewModel
    {
        get { return _statusViewModel; }
        set { this.RaiseAndSetIfChanged(ref _statusViewModel, value); }
    }
    public bool LoadedFromConfiguration
    {
        get { return _loadedFromConfiguration; }
        set { this.RaiseAndSetIfChanged(ref _loadedFromConfiguration, value); }
    }
    public float Volume
    {
        get { return _volume; }
        set { this.RaiseAndSetIfChanged(ref _volume, value); }
    }
    public bool Loading
    {
        get { return _loading; }
        set { this.RaiseAndSetIfChanged(ref _loading, value); }
    }
    public bool ConfigurationLocked
    {
        get { return _configurationLocked; }
        set { this.RaiseAndSetIfChanged(ref _configurationLocked, value); }
    }
    public LibraryManagerViewModel LibraryManager
    {
        get { return _libraryManager; }
        set { this.RaiseAndSetIfChanged(ref _libraryManager, value); }
    }
    public LibraryImporterViewModel LibraryImporter
    {
        get { return _libraryImportViewModel; }
        set { this.RaiseAndSetIfChanged(ref _libraryImportViewModel, value); }
    }
    public LibraryLoaderAcoustIDViewModel LibraryLoaderAcoustID
    {
        get { return _libraryLoaderAcoustID; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderAcoustID, value); }
    }
    public LibraryLoaderCDImportViewModel LibraryLoaderCDImport
    {
        get { return _libraryLoaderCDImport; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderCDImport, value); }
    }
    public LibraryLoaderMusicBrainzBasicViewModel LibraryLoaderMusicBrainzBasic
    {
        get { return _libraryLoaderMusicBrainzBasic; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderMusicBrainzBasic, value); }
    }
    public LibraryLoaderMusicBrainzAlbumArtViewModel LibraryLoaderMusicBrainzAlbumArt
    {
        get { return _libraryLoaderMusicBrainzAlbumArt; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderMusicBrainzAlbumArt, value); }
    }
    public RadioViewModel Radio
    {
        get { return _radio; }
        set { this.RaiseAndSetIfChanged(ref _radio, value); }
    }
    public LogViewModel Log
    {
        get { return _log; }
        set { this.RaiseAndSetIfChanged(ref _log, value); }
    }
    public BandcampViewModel Bandcamp
    {
        get { return _bandcamp; }
        set { this.RaiseAndSetIfChanged(ref _bandcamp, value); }
    }
    public NowPlayingViewModel NowPlaying
    {
        get { return _nowPlaying; }
        set { this.RaiseAndSetIfChanged(ref _nowPlaying, value); }
    }
    public ObservableCollection<float> EqualizerValues
    {
        get { return _equalizerValues; }
        set { this.RaiseAndSetIfChanged(ref _equalizerValues, value); }
    }
    public ObservableCollection<EqualizerBandViewModel> EqualizerViewModel
    {
        get { return _equalizerViewModel; }
        set { this.RaiseAndSetIfChanged(ref _equalizerViewModel, value); }
    }
    public PlayStopPause PlayState
    {
        get { return _playState; }
        set { this.RaiseAndSetIfChanged(ref _playState, value); }
    }
    public SimpleCommand OpenLibraryFolderCommand
    {
        get { return _openLibraryFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openLibraryFolderCommand, value); }
    }
    public SimpleCommand OpenMusicSubFolderCommand
    {
        get { return _openMusicSubFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openMusicSubFolderCommand, value); }
    }
    public SimpleCommand OpenAudioBooksSubFolderCommand
    {
        get { return _openAudioBooksSubFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openAudioBooksSubFolderCommand, value); }
    }
    public SimpleCommand OpenImportFolderCommand
    {
        get { return _openImportFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openImportFolderCommand, value); }
    }
    public SimpleCommand OpenCacheFolderCommand
    {
        get { return _openCacheFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openCacheFolderCommand, value); }
    }
    public SimpleCommand OpenStorageFolderCommand
    {
        get { return _openStorageFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openStorageFolderCommand, value); }
    }
    public SimpleCommand SaveConfigurationCommand
    {
        get { return _saveConfigurationCommand; }
        set { this.RaiseAndSetIfChanged(ref _saveConfigurationCommand, value); }
    }
    public SimpleCommand UnlockConfigurationCommand
    {
        get { return _unlockConfigurationCommand; }
        set { this.RaiseAndSetIfChanged(ref _unlockConfigurationCommand, value); }
    }
    #endregion

    [IocImportingConstructor]
    public MainViewModel(IConfigurationManager configurationManager,
                         IAudioStationServiceController componentController,
                         IDialogController dialogController,
                         IIocEventAggregator eventAggregator,
                         ICDDrive cdDrive,

                         // View Models
                         LibraryManagerViewModel libraryManagerViewModel,
                         StatusViewModel statusViewModel,
                         RadioViewModel radioViewModel,
                         LogViewModel logViewModel,
                         LibraryImporterViewModel libraryImporterViewModel,
                         LibraryLoaderAcoustIDViewModel libraryLoaderAcoustIDViewModel,
                         LibraryLoaderCDImportViewModel libraryLoaderCDImportViewModel,
                         LibraryLoaderMusicBrainzBasicViewModel libraryLoaderMusicBrainzBasicViewModel,
                         LibraryLoaderMusicBrainzAlbumArtViewModel libraryLoaderMusicBrainzAlbumArtViewModel,
                         NowPlayingViewModel nowPlayingViewModel,
                         BandcampViewModel bandcampViewModel)
    {
        _eventAggregator = eventAggregator;

        this.ConfigurationLocked = true;
        this.Configuration = configurationManager.GetConfiguration();
        this.EqualizerValues = new ObservableCollection<float>();
        this.EqualizerViewModel = new ObservableCollection<EqualizerBandViewModel>()
        {
            // See SimpleMp3PlayerWithEqualizer (channel number won't be input.. just keeping things in sync w/ NAudio)
            new EqualizerBandViewModel(100, 0, 0.8f, 1),
            new EqualizerBandViewModel(200, 0, 0.8f, 1),
            new EqualizerBandViewModel(400, 0, 0.8f, 1),
            new EqualizerBandViewModel(800, 0, 0.8f, 1),
            new EqualizerBandViewModel(1200, 0, 0.8f, 1),
            new EqualizerBandViewModel(2400, 0, 0.8f, 1),
            new EqualizerBandViewModel(4800, 0, 0.8f, 1),
            new EqualizerBandViewModel(9600, 0, 0.8f, 1)
        };

        // Child View Models
        this.Log = logViewModel;
        this.NowPlaying = nowPlayingViewModel;
        this.PlayState = PlayStopPause.Stop;
        this.LibraryManager = libraryManagerViewModel;
        this.StatusViewModel = statusViewModel;
        this.Radio = radioViewModel;
        this.LibraryImporter = libraryImporterViewModel;
        this.LibraryLoaderAcoustID = libraryLoaderAcoustIDViewModel;
        this.LibraryLoaderCDImport = libraryLoaderCDImportViewModel;
        this.LibraryLoaderMusicBrainzBasic = libraryLoaderMusicBrainzBasicViewModel;
        this.LibraryLoaderMusicBrainzAlbumArt = libraryLoaderMusicBrainzAlbumArtViewModel;
        this.Bandcamp = bandcampViewModel;
        this.Volume = 1.0f;
        this.Loading = false;

        // IAudioStationComponent
        var audioController = componentController.GetComponent<IAudioController>();

        audioController.CurrentTimeUpdated += OnCurrentTimeUpdated;
        audioController.CurrentBandLevelsUpdated += OnCurrentBandLevelsUpdated;

        componentController.ComponentInitializedEvent += IAudioStationComponent_StatusChangeEvent;
        componentController.ComponentStatusChangedEvent += IAudioStationComponent_StatusChangeEvent;

        // Event Aggregator
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);
        eventAggregator.GetEvent<PlaybackStateChangedEvent>().Subscribe(OnPlaybackStateChanged);
        eventAggregator.GetEvent<UpdateVolumeEvent>().Subscribe(OnUpdateVolume);
        eventAggregator.GetEvent<UpdateEqualizerGainEvent>().Subscribe(OnUpdateEqualizer);
        eventAggregator.GetEvent<PlaybackVolumeUpdatedEvent>().Subscribe(OnVolumeUpdated);
        eventAggregator.GetEvent<DialogEvent>().Subscribe(OnMainLoadingChanged, IocEventPriority.High);

        this.SaveConfigurationCommand = new SimpleCommand(() =>
        {
            configurationManager.SaveConfiguration();
            this.ConfigurationLocked = true;
        });
        this.OpenLibraryFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.DirectoryBase = folder;
            }
        });
        this.OpenMusicSubFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.MusicSubDirectory = Path.GetFileName(folder) ?? string.Empty;
            }
        });
        this.OpenAudioBooksSubFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.AudioBooksSubDirectory = Path.GetFileName(folder) ?? string.Empty;
            }
        });
        this.OpenImportFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.ImportFolder = folder;
            }
        });
        this.OpenCacheFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.ApplicationCacheFolder = folder;
            }
        });
        this.OpenStorageFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.ApplicationStorageFolder = folder;
            }
        });
        this.UnlockConfigurationCommand = new SimpleCommand(() =>
        {
            this.ConfigurationLocked = false;
        });
    }

    private void OnLog(LogMessage message)
    {
        // --> IOuptutController (IAudioStationComponent)      
    }
    private void IAudioStationComponent_StatusChangeEvent(IAudioStationService sender, IAudioStationService.Status status)
    {
        StatusIconViewModel viewModel = null;

        if (sender is IOutputController)
            viewModel = this.StatusViewModel.OutputControllerStatus;

        else if (sender is IAudioStationDbClient)
            viewModel = this.StatusViewModel.AudioStationDbStatus;

        else if (sender is IAudioController)
            viewModel = this.StatusViewModel.AudioPlayerStatus;

        else if (sender is IAcoustIDClient)
            viewModel = this.StatusViewModel.AcoustIDClient;

        else if (sender is IBandcampClient)
            viewModel = this.StatusViewModel.BandcampClient;

        else if (sender is IDiscogsClient)
            viewModel = this.StatusViewModel.DiscogsClient;

        else if (sender is IFanartClient)
            viewModel = this.StatusViewModel.FanartClient;

        else if (sender is IITunesClient)
            viewModel = this.StatusViewModel.ITunesClient;

        else if (sender is ILastFmClient)
            viewModel = this.StatusViewModel.LastFmClient;

        else if (sender is IMusicBrainzClient)
            viewModel = this.StatusViewModel.MusicBrainzClient;

        else if (sender is ISpotifyClient)
            viewModel = this.StatusViewModel.SpotifyClient;

        else
            throw new Exception("Unhandled IAudioStationComponent type");

        viewModel.Status = status;
        viewModel.Message = sender.GetStatusMessage();

        // Primary status bar message
        this.StatusViewModel.PrimaryMessage = viewModel.Message;
    }

    private void OnMainLoadingChanged(DialogEventData eventData)
    {
        this.Loading = eventData.Show;
    }

    private void OnCurrentBandLevelsUpdated(EqualizerResultSet equalizerValues)
    {
        // There is a problem binding to this collection. So we may just publish things this way.
        _eventAggregator.GetEvent<PlaybackEqualizerUpdateEvent>().Publish(equalizerValues);
    }

    private void OnPlaybackStateChanged(PlaybackStateChangedEventData eventData)
    {
        this.PlayState = eventData.State;
    }
    private void OnUpdateVolume(double volume)
    {
        this.Volume = (float)volume;
    }
    private void OnVolumeUpdated(double volume)
    {
        this.Volume = (float)volume;
    }
    private void OnUpdateEqualizer(UpdateEqualizerGainEventData data)
    {
        this.EqualizerViewModel
            .First(x => x.Frequency == data.Frequency).Gain = data.Gain;
    }
    private void OnCurrentTimeUpdated(TimeSpan currentTime)
    {
        this.NowPlaying.Playlist.CurrentTrack?.UpdateCurrentTime(currentTime);
    }
}
