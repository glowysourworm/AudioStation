using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.ViewModels.Interface;
using AudioStation.ViewModels.LibraryViewModels.Comparer;
using AudioStation.ViewModels.RadioViewModels;

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
    RadioViewModel _radio;
    BandcampViewModel _bandcamp;
    LibraryLoaderViewModel _libraryLoaderViewModel;
    INowPlayingViewModel _nowPlayingViewModel;

    PlayStopPause _playState;

    ObservableCollection<LogMessageViewModel> _outputMessages;

    SimpleCommand _openLibraryFolderCommand;
    SimpleCommand _openDownloadFolderCommand;
    SimpleCommand _saveConfigurationCommand;
    SimpleCommand _loadLibraryCommand;
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
    public RadioViewModel Radio
    {
        get { return _radio; }
        set { this.RaiseAndSetIfChanged(ref _radio, value); }
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
                         RadioViewModel radioViewModel,
                         LibraryLoaderViewModel libraryLoaderViewModel,
                         BandcampViewModel bandcampViewModel)
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

        // Child View Models
        this.NowPlayingViewModel = null;
        this.PlayState = PlayStopPause.Stop;
        this.Library = libraryViewModel;
        this.Radio = radioViewModel;
        this.LibraryLoader = libraryLoaderViewModel;
        this.Bandcamp = bandcampViewModel;

        this.DatabaseLogLevel = LogLevel.Trace;
        this.GeneralLogLevel = LogLevel.Trace;
        this.SelectedLogType = LogMessageType.General;

        // Event Aggregator
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);
        eventAggregator.GetEvent<LoadPlaybackEvent>().Subscribe(OnLoadPlayback);
        eventAggregator.GetEvent<PlaybackStateChangedEvent>().Subscribe(OnPlaybackStateChanged);      
        eventAggregator.GetEvent<UpdateVolumeEvent>().Subscribe(OnUpdateVolume);            
        eventAggregator.GetEvent<PlaybackVolumeUpdatedEvent>().Subscribe(OnVolumeUpdated);  

        this.SaveConfigurationCommand = new SimpleCommand(() =>
        {
            configurationManager.SaveConfiguration();
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

        outputController.AddLog("Welcome to Audio Station!", LogMessageType.General);
    }

    private void OnPlaybackStateChanged(PlayStopPause state)
    {
        this.PlayState = state;
    }

    private void OnLoadPlayback(INowPlayingViewModel model)
    {
        this.NowPlayingViewModel = model;       
    }
    private void OnUpdateVolume(double volume)
    {
        this.Volume = (float)volume;
    }
    private void OnVolumeUpdated(double volume)
    {
        this.Volume = (float)volume;
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
            this.NowPlayingViewModel = null;
        }
    }
}
