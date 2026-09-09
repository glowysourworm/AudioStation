using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.Model.AudioProcessing;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.Controls;
using AudioStation.ViewModels.MainViewModels;
using AudioStation.ViewModels.OtherViewModels;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels;

public class MainViewModel : ComponentViewModelBase
{
    private readonly IIocEventAggregator _eventAggregator;


    #region Backing Fields
    AudioStationConfigurationViewModel _configuration;
    bool _loadedFromConfiguration;
    float _volume;
    bool _configurationLocked;

    ObservableCollection<AudioEncoderViewModel> _encoders;

    LibraryManagerViewModel _libraryManager;
    StatusViewModel _statusViewModel;
    RadioViewModel _radio;
    LogViewModel _log;
    NowPlayingViewModel _nowPlaying;
    BandcampViewModel _bandcamp;
    LibraryImporterViewModel _libraryImportViewModel;
    LibraryLoaderViewModel _libraryLoaderViewModel;
    CDImporterViewModel _libraryLoaderCDImport;

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
    public AudioStationConfigurationViewModel Configuration
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
    public bool ConfigurationLocked
    {
        get { return _configurationLocked; }
        set { this.RaiseAndSetIfChanged(ref _configurationLocked, value); }
    }
    public ObservableCollection<AudioEncoderViewModel> Encoders
    {
        get { return _encoders; }
        set { this.RaiseAndSetIfChanged(ref _encoders, value); }
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
    public LibraryLoaderViewModel LibraryLoader
    {
        get { return _libraryLoaderViewModel; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderViewModel, value); }
    }
    public CDImporterViewModel LibraryLoaderCDImport
    {
        get { return _libraryLoaderCDImport; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderCDImport, value); }
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

    public MainViewModel() :
        this(IocContainer.Get<IAudioController>(),
             IocContainer.Get<IAudioStationMapper>(),
             IocContainer.Get<IDialogController>(),
             IocContainer.Get<IIocEventAggregator>(),
             IocContainer.Get<ICDDrive>(),
             IocContainer.Get<IAudioConverter>())
    {
    }
    public MainViewModel(IAudioController audioController,
                         IAudioStationMapper audioStationMapper,
                         IDialogController dialogController,
                         IIocEventAggregator eventAggregator,
                         ICDDrive cdDrive,
                         IAudioConverter audioConverter) : base("Main")
    {
        audioController.CurrentTimeUpdated += OnCurrentTimeUpdated;
        audioController.CurrentBandLevelsUpdated += OnCurrentBandLevelsUpdated;

        // Event Aggregator
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);
        eventAggregator.GetEvent<PlaybackStateChangedEvent>().Subscribe(OnPlaybackStateChanged);
        eventAggregator.GetEvent<UpdateVolumeEvent>().Subscribe(OnUpdateVolume);
        eventAggregator.GetEvent<UpdateEqualizerGainEvent>().Subscribe(OnUpdateEqualizer);
        eventAggregator.GetEvent<PlaybackVolumeUpdatedEvent>().Subscribe(OnVolumeUpdated);
        eventAggregator.GetEvent<DialogEvent>().Subscribe(OnMainLoadingChanged, IocEventPriority.High);

        // -> Configuration
        eventAggregator.GetEvent<ConfigurationEvent>().Subscribe((eventData) =>
        {
            switch (eventData.Type)
            {
                case ConfigurationEventType.Opened:
                    this.Configuration = eventData.ViewModel;
                    break;
                case ConfigurationEventType.Modified:
                    this.Configuration = eventData.ViewModel;
                    break;
                case ConfigurationEventType.Saved:
                    this.Configuration = eventData.ViewModel;
                    this.ConfigurationLocked = true;
                    break;
                case ConfigurationEventType.SaveRequest:
                case ConfigurationEventType.ModifyRequest:
                    break;
                default:
                    throw new Exception("Unhandled configuration event type");
            }
        });

        var encoders = audioConverter.GetSupportedFormats();
        this.Encoders = new ObservableCollection<AudioEncoderViewModel>();

        foreach (var encoder in encoders)
        {
            this.Encoders.Add(audioStationMapper.Map<AudioEncoderInfo, AudioEncoderViewModel>(encoder));
        }

        // -> Configuration
        this.SaveConfigurationCommand = new SimpleCommand(() =>
        {
            // Save Request
            eventAggregator.GetEvent<ConfigurationEvent>().Publish(new ConfigurationEventData()
            {
                ViewModel = this.Configuration,
                Type = ConfigurationEventType.SaveRequest
            });
        });
        this.OpenLibraryFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.DirectoryBase = folder;
            }
        });
        this.OpenMusicSubFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.MusicSubDirectory = Path.GetFileName(folder) ?? string.Empty;
            }
        });
        this.OpenAudioBooksSubFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.AudioBooksSubDirectory = Path.GetFileName(folder) ?? string.Empty;
            }
        });
        this.OpenImportFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.ImportFolder = folder;
            }
        });
        this.OpenCacheFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.ApplicationCacheFolder = folder;
            }
        });
        this.OpenStorageFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                //this.Configuration.ApplicationStorageFolder = folder;
            }
        });
        this.UnlockConfigurationCommand = new SimpleCommand(() =>
        {
            this.ConfigurationLocked = false;
        });
    }

    protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
    {
        audioStationController.ServiceController.ComponentInitializedEvent += IAudioStationComponent_StatusChangeEvent;
        audioStationController.ServiceController.ComponentStatusChangedEvent += IAudioStationComponent_StatusChangeEvent;

        this.ConfigurationLocked = true;
        this.Configuration = audioStationController.ComponentController.GetComponent<AudioStationConfigurationViewModel>();
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
        this.Log = audioStationController.ComponentController.GetComponent<LogViewModel>();
        this.NowPlaying = audioStationController.ComponentController.GetComponent<NowPlayingViewModel>();
        this.PlayState = PlayStopPause.Stop;
        this.LibraryManager = audioStationController.ComponentController.GetComponent<LibraryManagerViewModel>();
        this.StatusViewModel = audioStationController.ComponentController.GetComponent<StatusViewModel>();
        this.Radio = audioStationController.ComponentController.GetComponent<RadioViewModel>();
        this.LibraryImporter = audioStationController.ComponentController.GetComponent<LibraryImporterViewModel>();
        this.LibraryLoader = audioStationController.ComponentController.GetComponent<LibraryLoaderViewModel>();
        this.LibraryLoaderCDImport = audioStationController.ComponentController.GetComponent<CDImporterViewModel>();
        this.Bandcamp = audioStationController.ComponentController.GetComponent<BandcampViewModel>();
        this.Volume = 1.0f;
        this.Loading = false;
    }
    protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
    {

    }
    private void OnLog(LogMessage message)
    {
        // --> IOuptutController (IAudioStationComponent)      
    }
    private void IAudioStationComponent_StatusChangeEvent(IAudioStationDataService sender, IAudioStationDataService.Status status)
    {
        // Still not initialized
        if (!this.Initialized)
            return;

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
