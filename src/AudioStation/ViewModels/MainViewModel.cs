using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Model;
using AudioStation.ViewModels.Interface;
using AudioStation.ViewModels.LibraryViewModels.Comparer;
using AudioStation.ViewModels.RadioViewModel;

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
    bool _showOutputMessages;
    bool _loadedFromConfiguration;
    float _volume;

    LogMessageType _selectedLogType;
    LogLevel _databaseLogLevel;
    LogLevel _generalLogLevel;

    LibraryViewModel _library;
    LibraryLoaderViewModel _libraryLoaderViewModel;
    INowPlayingViewModel _nowPlayingViewModel;

    ObservableCollection<LogMessageViewModel> _outputMessages;

    // Our Primary Library Collections
    SortedObservableCollection<RadioEntryViewModel> _radioEntries;

    SimpleCommand<string> _searchRadioBrowserCommand;
    SimpleCommand _openLibraryFolderCommand;
    SimpleCommand _saveConfigurationCommand;
    #endregion

    #region Properties
    public Configuration Configuration
    {
        get { return _configuration; }
        set { this.RaiseAndSetIfChanged(ref _configuration, value); }
    }
    public ObservableCollection<LogMessageViewModel> OutputMessages
    {
        get { return _outputMessages; }
        set { this.RaiseAndSetIfChanged(ref _outputMessages, value); }
    }
    public SortedObservableCollection<RadioEntryViewModel> RadioStations
    {
        get { return _radioEntries; }
        set { this.RaiseAndSetIfChanged(ref _radioEntries, value); }
    }
    public string StatusMessage
    {
        get { return _statusMessage; }
        set { this.RaiseAndSetIfChanged(ref _statusMessage, value); }
    }
    public bool ShowOutputMessages
    {
        get { return _showOutputMessages; }
        set { this.RaiseAndSetIfChanged(ref _showOutputMessages, value); }
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
    public LibraryViewModel Library
    {
        get { return _library; }
        set { this.RaiseAndSetIfChanged(ref _library, value); }
    }
    public LibraryLoaderViewModel LibraryLoader
    {
        get { return _libraryLoaderViewModel; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderViewModel, value); }
    }
    public LogMessageType SelectedLogType
    {
        get { return _selectedLogType; }
        set { this.RaiseAndSetIfChanged(ref _selectedLogType, value); OnLogTypeChanged(); }
    }
    public LogLevel DatabaseLogLevel
    {
        get { return _databaseLogLevel; }
        set { this.RaiseAndSetIfChanged(ref _databaseLogLevel, value); OnLogLevelChanged(LogMessageType.Database); }
    }
    public LogLevel GeneralLogLevel
    {
        get { return _generalLogLevel; }
        set { this.RaiseAndSetIfChanged(ref _generalLogLevel, value); OnLogLevelChanged(LogMessageType.General); }
    }
    public INowPlayingViewModel NowPlayingViewModel
    {
        get { return _nowPlayingViewModel; }
        set { this.RaiseAndSetIfChanged(ref _nowPlayingViewModel, value); }
    }
    public SimpleCommand<string> SearchRadioBrowserCommand
    {
        get { return _searchRadioBrowserCommand; }
        set { this.RaiseAndSetIfChanged(ref _searchRadioBrowserCommand, value); }
    }
    public SimpleCommand OpenLibraryFolderCommand
    {
        get { return _openLibraryFolderCommand; }
        set { this.RaiseAndSetIfChanged(ref _openLibraryFolderCommand, value); }
    }
    public SimpleCommand SaveConfigurationCommand
    {
        get { return _saveConfigurationCommand; }
        set { this.RaiseAndSetIfChanged(ref _saveConfigurationCommand, value); }
    }
    #endregion

    [IocImportingConstructor]
    public MainViewModel(IConfigurationManager configurationManager,
                         IDialogController dialogController,
                         IAudioController audioController,
                         IModelController modelController,
                         ILibraryLoader libraryLoader,
                         IOutputController outputController,
                         IIocEventAggregator eventAggregator,

                         // View Models
                         LibraryViewModel libraryViewModel,
                         LibraryLoaderViewModel libraryLoaderViewModel)
    {
        _dialogController = dialogController;
        _audioController = audioController;
        _modelController = modelController;
        _libraryLoader = libraryLoader;
        _outputController = outputController;
        _eventAggregator = eventAggregator;

        this.Configuration = configurationManager.GetConfiguration();
        this.ShowOutputMessages = false;
        this.OutputMessages = new ObservableCollection<LogMessageViewModel>();
        this.RadioStations = new SortedObservableCollection<RadioEntryViewModel>(new PropertyComparer<string, RadioEntryViewModel>(x => x.Name));

        // Child View Models
        this.NowPlayingViewModel = null;
        this.Library = libraryViewModel;
        this.LibraryLoader = libraryLoaderViewModel;

        // Initialize Model
        //foreach (var libraryEntry in _modelController.Library)

        this.DatabaseLogLevel = LogLevel.Trace;
        this.GeneralLogLevel = LogLevel.Trace;
        this.SelectedLogType = LogMessageType.General;

        // Event Aggregator
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);

        libraryLoader.RadioEntryLoaded += OnRadioEntryLoaded;

        audioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        audioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;

        this.SearchRadioBrowserCommand = new SimpleCommand<string>((search) =>
        {
            SearchRadioBrowser(search);
        });
        this.SaveConfigurationCommand = new SimpleCommand(() =>
        {
            configurationManager.SaveConfiguration();
        });
        this.OpenLibraryFolderCommand = new SimpleCommand(() =>
        {
            this.Configuration.DirectoryBase = dialogController.ShowSelectFolder();
        });

        outputController.AddLog("Welcome to Audio Station!", LogMessageType.General);
    }

    private void OnAudioControllerPlaybackStopped(INowPlayingViewModel nowPlaying)
    {
        this.NowPlayingViewModel = null;        // Remove View Model (affects view binding to the player controls)
    }
    private void OnAudioControllerPlaybackStarted(INowPlayingViewModel nowPlaying)
    {
        this.NowPlayingViewModel = nowPlaying;  // Primary settings for the view (binding)
    }

    #region ILibraryLoader Events (some of these events are from the worker thread)
    private void OnRadioEntryLoaded(RadioEntry radioEntry)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.BeginInvoke(OnRadioEntryLoaded, DispatcherPriority.ApplicationIdle, radioEntry);
        else
        {
            if (!this.RadioStations.Any(item => item.Name == radioEntry.Name))
            {
                var entry = new RadioEntryViewModel()
                {
                    Name = radioEntry.Name,
                };

                entry.Stations.AddRange(radioEntry.Streams.Select(x => new RadioStationViewModel()
                {
                    Bitrate = x.Bitrate,
                    Codec = x.Codec,
                    Name = x.Name,
                    Endpoint = x.Endpoint,
                    Homepage = x.Homepage,
                    LogoEndpoint = x.LogoEndpoint

                }));

                this.RadioStations.Add(entry);
            }
            else
            {
                _outputController.AddLog("Radio Station already exists! {0}", LogMessageType.General, LogLevel.Error, radioEntry.Name);
            }
        }
    }
    #endregion

    public async void SearchRadioBrowser(string search)
    {
        try
        {
            var streams = await RadioBrowserSearchComponent.SearchStation(search);
            //var streams = await RadioBrowserSearchComponent.GetTopStations(10);

            if (streams.Count == 0)
                return;

            //this.Radio.RadioStations.Clear();
            //this.Radio.RadioStations.AddRange(streams.Where(stream => stream != null && stream.Bitrate > 0 && !string.IsNullOrEmpty(stream.Codec))
            //                                         .Select(stream =>
            //{
            //    return new RadioStationViewModel()
            //    {
            //        Bitrate = stream.Bitrate,
            //        Codec = stream.Codec,
            //        Name = stream.Name,
            //        Homepage = stream.Homepage.ToString(),
            //        Endpoint = stream.Url.ToString(),
            //        LogoEndpoint = stream.Favicon.ToString()
            //    };
            //}));
        }
        catch (Exception ex)
        {
            //OnLog("Error querying Radio Browser:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }
    }

    private void OnLogTypeChanged()
    {
        ResetOutputMessages();
    }
    private void OnLogLevelChanged(LogMessageType type)
    {
        ResetOutputMessages();
    }
    private void OnLog(LogMessage message)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnLogImpl(message);

            }, DispatcherPriority.ApplicationIdle);
        }
        else
            OnLogImpl(message);
    }
    private void OnLogImpl(LogMessage message)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            throw new Exception("Trying to access MainViewModel on non-dispatcher thread");

        var logLevel = this.SelectedLogType == LogMessageType.General ? this.GeneralLogLevel : this.DatabaseLogLevel;

        if (message.Type == this.SelectedLogType && message.Level >= logLevel)
        {
            this.OutputMessages.Insert(0, new LogMessageViewModel()
            {
                Message = message.Message,
                Type = message.Type,
                Level = message.Level
            });

            if (this.OutputMessages.Count > MAX_LOG_COUNT)
            {
                this.OutputMessages.RemoveAt(this.OutputMessages.Count - 1);
            }

            // Status Message (for now, last log message)
            this.StatusMessage = message.Message;
        }
    }
    private void ResetOutputMessages()
    {
        var logLevel = this.SelectedLogType == LogMessageType.General ? this.GeneralLogLevel : this.DatabaseLogLevel;

        this.OutputMessages.Clear();
        this.OutputMessages.AddRange(_outputController.GetLatestLogs(this.SelectedLogType, logLevel, MAX_LOG_COUNT)
                                                      .Select(log => new LogMessageViewModel()
                                                      {
                                                          Level = log.Level,
                                                          Message = log.Message,
                                                          Type = log.Type,
                                                      }));

        // Send event out for new log configuration
        // Update log output configuration
        _eventAggregator.GetEvent<LogConfigurationChangedEvent>().Publish(new LogConfigurationData()
        {
            Level = logLevel,
            Verbose = false,
            Type = this.SelectedLogType
        });
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
            this.ShowOutputMessages = false;
            this.OutputMessages.Clear();
            this.RadioStations.Clear();
            this.NowPlayingViewModel = null;
        }
    }
}
