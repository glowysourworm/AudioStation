using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Component.AudioProcessing;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.ViewModels.Controls;
using AudioStation.ViewModels.LibraryViewModels.Comparer;
using AudioStation.ViewModels.LogViewModels;
using AudioStation.ViewModels.PlaylistViewModels.Interface;
using AudioStation.ViewModels.RadioViewModels;
using AudioStation.ViewModels.Vendor;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels;

[IocExportDefault]
public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IDialogController _dialogController;
    private readonly IAudioController _audioController;
    private readonly IModelController _modelController;
    private readonly ILibraryLoader _libraryLoader;
    private readonly IOutputController _outputController;
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

    LibraryViewModel _library;
    RadioViewModel _radio;
    LogViewModel _log;
    NowPlayingViewModel _nowPlaying;
    BandcampViewModel _bandcamp;
    LibraryLoaderViewModel _libraryLoaderViewModel;

    ObservableCollection<float> _equalizerValues;
    ObservableCollection<EqualizerBandViewModel> _equalizerViewModel;
    PlayStopPause _playState;

    SimpleCommand _openLibraryFolderCommand;
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
    public LibraryViewModel Library
    {
        get { return _library; }
        set { this.RaiseAndSetIfChanged(ref _library, value); }
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
    public SimpleCommand LoadLibraryCommand
    {
        get { return _loadLibraryCommand; }
        set { this.RaiseAndSetIfChanged(ref _loadLibraryCommand, value); }
    }
    public SimpleCommand UnlockConfigurationCommand
    {
        get { return _unlockConfigurationCommand; }
        set { this.RaiseAndSetIfChanged(ref _unlockConfigurationCommand, value); }
    }
    #endregion

    [IocImportingConstructor]
    public MainViewModel(IConfigurationManager configurationManager,
                         IDialogController dialogController,
                         IAudioController audioController,
                         IModelController modelController,
                         ILibraryLoader libraryLoader,
                         IIocEventAggregator eventAggregator,

                         // View Models
                         LibraryViewModel libraryViewModel,
                         RadioViewModel radioViewModel,
                         LogViewModel logViewModel,
                         LibraryLoaderViewModel libraryLoaderViewModel,
                         NowPlayingViewModel nowPlayingViewModel,
                         BandcampViewModel bandcampViewModel)
    {
        _dialogController = dialogController;
        _audioController = audioController;
        _modelController = modelController;
        _libraryLoader = libraryLoader;
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
        this.Library = libraryViewModel;
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
        eventAggregator.GetEvent<MainLoadingChangedEvent>().Subscribe(OnMainLoadingChanged);

        this.SaveConfigurationCommand = new SimpleCommand(() =>
        {
            configurationManager.SaveConfiguration();
            this.ConfigurationLocked = true;
        });
        this.OpenLibraryFolderCommand = new SimpleCommand(() =>
        {
            this.Configuration.DirectoryBase = dialogController.ShowSelectFolder();
        });
        this.LoadLibraryCommand = new SimpleCommand(() =>
        {
            if (dialogController.ShowConfirmation("Library Database Initialization", 
                                                  "This will delete your existing library data and reload it from:",
                                                  "", this.Configuration.DirectoryBase, "", 
                                                  "Your audio file(s) will not be otherwise disturbed.",
                                                  "Are you sure you want to do this?"))
            {
                libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
                libraryLoader.Start();
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

    private void OnLog(LogMessage message)
    {
        if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
            Application.Current.Dispatcher.BeginInvoke(OnLog, DispatcherPriority.Background, message);

        else
        {
            this.StatusMessage = message.Message;
        }
    }

    private void OnMainLoadingChanged(bool loading)
    {
        this.Loading = loading;
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
        if (this.NowPlaying.Playlist.CurrentTrack != null)
            this.NowPlaying.Playlist.CurrentTrack.UpdateCurrentTime(currentTime);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            // Dispose of any threads / unmanaged resources / and managed hooks (if they hold memory!) (but, application is now likely finished)
            _libraryLoader.Dispose();
            _dialogController.Dispose();
            _modelController.Dispose();
            _audioController.Dispose();

            this.Configuration = null;
        }
    }
}
