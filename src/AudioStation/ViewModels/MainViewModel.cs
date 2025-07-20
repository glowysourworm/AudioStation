using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Component.AudioProcessing;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Event;
using AudioStation.EventHandler;
using AudioStation.Model;
using AudioStation.ViewModels.Controls;
using AudioStation.ViewModels.LibraryManagerViewModels;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels;

public class MainViewModel : PrimaryViewModelBase
{
    private readonly IIocEventAggregator _eventAggregator;

    const int MAX_LOG_COUNT = 300;

    bool _disposed = false;

    #region Backing Fields
    Configuration _configuration;
    string _statusMessage;
    bool _loadedFromConfiguration;
    float _volume;
    bool _loading;
    bool _configurationLocked;

    LibraryManagerViewModel _libraryManager;
    RadioViewModel _radio;
    LogViewModel _log;
    NowPlayingViewModel _nowPlaying;
    BandcampViewModel _bandcamp;
    LibraryLoaderViewModel _libraryLoaderViewModel;

    ObservableCollection<float> _equalizerValues;
    ObservableCollection<EqualizerBandViewModel> _equalizerViewModel;
    PlayStopPause _playState;

    SimpleCommand _openLibraryFolderCommand;
    SimpleCommand _openMusicSubFolderCommand;
    SimpleCommand _openAudioBooksSubFolderCommand;
    SimpleCommand _openDownloadFolderCommand;
    SimpleCommand _saveConfigurationCommand;
    SimpleCommand _loadLibraryCommand;
    SimpleCommand _unlockConfigurationCommand;
    #endregion

    #region Properties
    public Configuration Configuration
    {
        get { return _configuration; }
        set { this.RaiseAndSetIfChanged(ref _configuration, value); }
    }
    public string StatusMessage
    {
        get { return _statusMessage; }
        set { this.RaiseAndSetIfChanged(ref _statusMessage, value); }
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
    public LibraryLoaderViewModel LibraryLoader
    {
        get { return _libraryLoaderViewModel; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderViewModel, value); }
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
    public SimpleCommand OpenDownloadFolderCommand
    {
        get { return _openDownloadFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openDownloadFolderCommand, value); }
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

    public MainViewModel(IConfigurationManager configurationManager,
                         IDialogController dialogController,    
                         IAudioController audioController,
                         IIocEventAggregator eventAggregator,
                         ICDDrive cdDrive,

                         // View Models
                         Configuration configuration,
                         LibraryManagerViewModel libraryManagerViewModel,
                         RadioViewModel radioViewModel,
                         LogViewModel logViewModel,
                         LibraryLoaderViewModel libraryLoaderViewModel,
                         NowPlayingViewModel nowPlayingViewModel,
                         BandcampViewModel bandcampViewModel)
    {
        _eventAggregator = eventAggregator;

        this.ConfigurationLocked = true;
        this.Configuration = configuration;
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
        this.Radio = radioViewModel;
        this.LibraryLoader = libraryLoaderViewModel;
        this.Bandcamp = bandcampViewModel;
        this.Volume = 1.0f;
        this.Loading = false;

        // IAudioController playback tick event
        audioController.CurrentTimeUpdated += OnCurrentTimeUpdated;
        audioController.CurrentBandLevelsUpdated += OnCurrentBandLevelsUpdated;

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
        this.OpenDownloadFolderCommand = new SimpleCommand(() =>
        {
            var folder = dialogController.ShowSelectFolder();

            if (!string.IsNullOrEmpty(folder))
            {
                this.Configuration.DownloadFolder = folder;
            }
        });
        this.UnlockConfigurationCommand = new SimpleCommand(() =>
        {
            this.ConfigurationLocked = false;
        });
    }

    public override Task Initialize(DialogProgressHandler progressHandler)
    {
        return Task.CompletedTask;
    }

    private void OnLog(LogMessage message)
    {
        this.StatusMessage = message.Message;
    }

    private void OnMainLoadingChanged(DialogEventData eventData)
    {
        this.Loading = eventData.ShowDialog;
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

    public override void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
