using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels.Interface;
using AudioStation.ViewModels.LibraryViewModel;
using AudioStation.ViewModels.LibraryViewModel.Comparer;
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

    const int MAX_LOG_COUNT = 300;

    bool _disposed = false;

    #region Backing Fields
    Configuration _configuration;
    string _statusMessage;
    bool _showOutputMessages;
    bool _loadedFromConfiguration;
    float _volume;

    PlayStopPause _libraryLoaderState;

    LogMessageType _selectedLogType;
    LogLevel _databaseLogLevel;
    LogLevel _generalLogLevel;
    LibraryWorkItemState _selectedLibraryWorkItemState;

    INowPlayingViewModel _nowPlayingViewModel;

    ObservableCollection<LogMessageViewModel> _outputMessages;
    ObservableCollection<LibraryWorkItemViewModel> _libraryCoreWorkItems;
    List<LibraryWorkItemViewModel> _libraryCoreWorkItemsUnfiltered;

    // Our Primary Library Collections
    SortedObservableCollection<RadioEntryViewModel> _radioEntries;
    SortedObservableCollection<LibraryEntryViewModel> _libraryEntries;

    SortedObservableCollection<AlbumViewModel> _albums;
    SortedObservableCollection<ArtistViewModel> _artists;
    SortedObservableCollection<TitleViewModel> _titles;

    SimpleCommand<string> _searchRadioBrowserCommand;
    SimpleCommand _openLibraryFolderCommand;
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
    public ObservableCollection<LibraryWorkItemViewModel> LibraryCoreWorkItems
    {
        get { return _libraryCoreWorkItems; }
        set { this.RaiseAndSetIfChanged(ref _libraryCoreWorkItems, value); }
    }
    public SortedObservableCollection<RadioEntryViewModel> RadioStations
    {
        get { return _radioEntries; }
        set { this.RaiseAndSetIfChanged(ref _radioEntries, value); }
    }
    public SortedObservableCollection<LibraryEntryViewModel> LibraryEntries
    {
        get { return _libraryEntries; }
        set { this.RaiseAndSetIfChanged(ref _libraryEntries, value); }
    }
    public SortedObservableCollection<AlbumViewModel> Albums
    {
        get { return _albums; }
        set { this.RaiseAndSetIfChanged(ref _albums, value); }
    }
    public SortedObservableCollection<ArtistViewModel> Artists
    {
        get { return _artists; }
        set { this.RaiseAndSetIfChanged(ref _artists, value); }
    }
    public SortedObservableCollection<TitleViewModel> Titles
    {
        get { return _titles; }
        set { this.RaiseAndSetIfChanged(ref _titles, value); }
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
    public PlayStopPause LibraryLoaderState
    {
        get { return _libraryLoaderState; }
        set { this.RaiseAndSetIfChanged(ref _libraryLoaderState, value); OnLibraryLoaderStateRequest(); }
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
    public LibraryWorkItemState SelectedLibraryWorkItemState
    {
        get { return _selectedLibraryWorkItemState; }
        set { this.RaiseAndSetIfChanged(ref _selectedLibraryWorkItemState, value); SetLibraryCoreWorkItemsFilter(); }
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
                         IIocEventAggregator eventAggregator)
    {
        _dialogController = dialogController;
        _audioController = audioController;
        _modelController = modelController;
        _libraryLoader = libraryLoader;
        _outputController = outputController;

        this.Configuration = configurationManager.GetConfiguration();
        this.ShowOutputMessages = false;
        this.OutputMessages = new ObservableCollection<LogMessageViewModel>();
        this.LibraryCoreWorkItems = new NotifyingObservableCollection<LibraryWorkItemViewModel>();
        this.RadioStations = new SortedObservableCollection<RadioEntryViewModel>(new PropertyComparer<string, RadioEntryViewModel>(x => x.Name));
        this.LibraryEntries = new SortedObservableCollection<LibraryEntryViewModel>(new PropertyComparer<string, LibraryEntryViewModel>(x => x.FileName));
        this.Albums = new SortedObservableCollection<AlbumViewModel>(new PropertyComparer<string, AlbumViewModel>(x => x.Album));
        this.Artists = new SortedObservableCollection<ArtistViewModel>(new PropertyComparer<string, ArtistViewModel>(x => x.Artist));
        this.Titles = new SortedObservableCollection<TitleViewModel>(new PropertyComparer<string, TitleViewModel>(x => x.Title));
        this.NowPlayingViewModel = null;

        // Filtering of the library loader work items
        _selectedLibraryWorkItemState = LibraryWorkItemState.Pending;
        _libraryCoreWorkItemsUnfiltered = new List<LibraryWorkItemViewModel>();

        this.LibraryLoaderState = libraryLoader.GetState();
        this.DatabaseLogLevel = LogLevel.None;
        this.GeneralLogLevel = LogLevel.None;
        this.SelectedLogType = LogMessageType.General;

        // Log Message (model) -> OnLogImpl (view-model)
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);

        libraryLoader.LibraryEntryLoaded += OnLibraryEntryLoaded;
        libraryLoader.RadioEntryLoaded += OnRadioEntryLoaded;
        libraryLoader.WorkItemCompleted += OnWorkItemCompleted;
        libraryLoader.WorkItemsAdded += OnWorkItemsAdded;
        libraryLoader.WorkItemsRemoved += OnWorkItemsRemoved;
        libraryLoader.ProcessingComplete += OnLibraryProcessingComplete;
        libraryLoader.ProcessingChanged += OnLibraryProcessingChanged;

        audioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        audioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;

        this.SearchRadioBrowserCommand = new SimpleCommand<string>((search) =>
        {
            SearchRadioBrowser(search);
        });
        this.LoadLibraryCommand = new SimpleCommand(() =>
        {
            libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
            libraryLoader.Start();
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
    private void OnLibraryLoaderWorkItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // These items our our view model items
        var item = sender as LibraryWorkItemViewModel;

        if (item != null && e.PropertyName == "LoadState")
        {
            var contains = this.LibraryCoreWorkItems.Contains(item);

            if (item.LoadState != this.SelectedLibraryWorkItemState && contains)
                this.LibraryCoreWorkItems.Remove(item);

            else if (item.LoadState == this.SelectedLibraryWorkItemState && !contains)
                this.LibraryCoreWorkItems.Add(item);
        }
    }
    private void OnWorkItemsRemoved(LibraryLoaderWorkItem[] workItems)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemsRemoved, DispatcherPriority.ApplicationIdle, workItems);
        else
        {
            foreach (var workItem in workItems)
            {
                var removedItems = _libraryCoreWorkItemsUnfiltered.Remove(item => item.FileName == workItem.FileName);

                foreach (var item in removedItems)
                {
                    item.PropertyChanged -= OnLibraryLoaderWorkItemPropertyChanged;

                    if (this.LibraryCoreWorkItems.Contains(item))
                        this.LibraryCoreWorkItems.Remove(item);
                }
            }
        }
    }
    private void OnWorkItemsAdded(LibraryLoaderWorkItem[] workItems)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemsAdded, DispatcherPriority.ApplicationIdle, workItems);
        else
        {
            foreach (var workItem in workItems)
            {
                var item = new LibraryWorkItemViewModel()
                {
                    FileName = workItem.FileName,
                    LoadType = workItem.LoadType,
                    ErrorMessage = workItem.ErrorMessage,
                    LoadState = workItem.LoadState
                };

                item.PropertyChanged += OnLibraryLoaderWorkItemPropertyChanged;

                _libraryCoreWorkItemsUnfiltered.Add(item);

                if (item.LoadState == this.SelectedLibraryWorkItemState)
                    this.LibraryCoreWorkItems.Add(item);
            }
        }
    }

    private void OnWorkItemCompleted(LibraryLoaderWorkItem workItem)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemCompleted, DispatcherPriority.ApplicationIdle, workItem);
        else
        {
            var item = _libraryCoreWorkItemsUnfiltered.FirstOrDefault(x => x.FileName == workItem.FileName);

            // -> Property Changed (sets the filtering)
            if (item != null)
                item.LoadState = workItem.LoadState;

            else
                _outputController.AddLog("Cannot find work item in local cache:  {0}", LogMessageType.General, LogLevel.Error, workItem.FileName);
        }
    }
    private void OnLibraryProcessingChanged(PlayStopPause oldState, PlayStopPause newState)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
        {
            Application.Current.Dispatcher.BeginInvoke(OnLibraryProcessingChanged, DispatcherPriority.ApplicationIdle, oldState, newState);
            return;
        }

        // Just update our field. The setter will call the code to modify the loader state OnLibraryLoaderStateRequest
        _libraryLoaderState = newState;

        OnPropertyChanged("LibraryLoaderState");
    }
    private void OnLibraryProcessingComplete()
    {
        _outputController.AddLog("Library Loader processing complete", LogMessageType.General);
    }
    private void OnRadioEntryLoaded(RadioEntry radioEntry)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemCompleted, DispatcherPriority.ApplicationIdle, radioEntry);
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
    private void OnLibraryEntryLoaded(LibraryEntry entry)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnLibraryEntryLoaded, DispatcherPriority.ApplicationIdle, entry);
        else
        {
            if (!this.LibraryEntries.Any(item => item.FileName == entry.FileName))
            {
                var newEntry = new LibraryEntryViewModel()
                {
                    Album = entry.Album,
                    Disc = entry.Disc,
                    FileName = entry.FileName,
                    Id = entry.Id,
                    LoadError = entry.FileError,
                    LoadErrorMessage = entry.FileErrorMessage,
                    PrimaryArtist = entry.PrimaryArtist,
                    PrimaryGenre = entry.PrimaryGenre,
                    Title = entry.Title,
                    Track = entry.Track
                };

                this.LibraryEntries.Add(newEntry);
            }
            else
            {
                _outputController.AddLog("Library Entry already exists! {0}", LogMessageType.General, LogLevel.Error, entry.FileName);
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
    
    private void SetLibraryCoreWorkItemsFilter()
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(SetLibraryCoreWorkItemsFilter, DispatcherPriority.ApplicationIdle);
        else
        {
            this.LibraryCoreWorkItems.Clear();

            foreach (var item in _libraryCoreWorkItemsUnfiltered)
            {
                if (item.LoadState == this.SelectedLibraryWorkItemState)
                    this.LibraryCoreWorkItems.Add(item);
            }
        }
    }

    private void OnLibraryLoaderStateRequest()
    {
        switch (this.LibraryLoaderState)
        {
            case PlayStopPause.Play:
                _libraryLoader.Start();
                break;
            case PlayStopPause.Pause:
            case PlayStopPause.Stop:
                _libraryLoader.Stop();
                break;
            default:
                throw new Exception("Unhandled play / stop / pause state:  MainViewModel.cs");
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
            this.LibraryCoreWorkItems.Clear();
            this.RadioStations.Clear();
            this.LibraryEntries.Clear();
            this.Albums.Clear();
            this.Artists.Clear();
            this.Titles.Clear();
            this.NowPlayingViewModel = null;
        }
    }
}
