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
using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels.Interface;
using AudioStation.ViewModels.LibraryViewModel;
using AudioStation.ViewModels.LibraryViewModel.Comparer;
using AudioStation.ViewModels.RadioViewModel;

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

    const int MAX_LOG_COUNT = 1000;

    bool _disposed = false;

    #region Backing Fields
    Configuration _configuration;
    string _statusMessage;
    bool _showOutputMessages;
    bool _loadedFromConfiguration;
    float _volume;

    INowPlayingViewModel _nowPlayingViewModel;

    ObservableCollection<LogMessageViewModel> _outputMessages;
    ObservableCollection<LibraryWorkItemViewModel> _libraryCoreWorkItems;

    // Our Primary Library Collections
    SortedObservableCollection<RadioEntryViewModel> _radioEntries;
    SortedObservableCollection<LibraryEntryViewModel> _libraryEntries;

    SortedObservableCollection<AlbumViewModel> _albums;
    SortedObservableCollection<ArtistViewModel> _artists;
    SortedObservableCollection<TitleViewModel> _titles;

    SimpleCommand<string> _searchRadioBrowserCommand;
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
        this.LibraryCoreWorkItems = new ObservableCollection<LibraryWorkItemViewModel>();
        this.RadioStations = new SortedObservableCollection<RadioEntryViewModel>(new PropertyComparer<string, RadioEntryViewModel>(x => x.Name));
        this.LibraryEntries = new SortedObservableCollection<LibraryEntryViewModel>(new PropertyComparer<string, LibraryEntryViewModel>(x => x.FileName));
        this.Albums = new SortedObservableCollection<AlbumViewModel>(new PropertyComparer<string, AlbumViewModel>(x => x.Album));
        this.Artists = new SortedObservableCollection<ArtistViewModel>(new PropertyComparer<string, ArtistViewModel>(x => x.Artist));
        this.Titles = new SortedObservableCollection<TitleViewModel>(new PropertyComparer<string, TitleViewModel>(x => x.Title));
        this.NowPlayingViewModel = null;

        // Log Message (model) -> OnLogImpl (view-model)
        eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);

        libraryLoader.LibraryEntryLoaded += OnLibraryEntryLoaded;
        libraryLoader.RadioEntryLoaded += OnRadioEntryLoaded;
        libraryLoader.WorkItemCompleted += OnWorkItemCompleted;
        libraryLoader.WorkItemsAdded += OnWorkItemsAdded;
        libraryLoader.WorkItemsRemoved += OnWorkItemsRemoved;

        audioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        audioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;

        this.SearchRadioBrowserCommand = new SimpleCommand<string>((search) =>
        {
            SearchRadioBrowser(search);
        });

        outputController.AddLog("Welcome to Audio Station!");
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
    private void OnWorkItemsRemoved(LibraryLoaderWorkItem[] workItems)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemsRemoved);
        else
        {
            foreach (var workItem in workItems)
            {
                this.LibraryCoreWorkItems.Remove(item => item.FileName == workItem.FileName);
            }
        }
    }
    private void OnWorkItemsAdded(LibraryLoaderWorkItem[] workItems)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemsAdded);
        else
        {
            foreach (var workItem in workItems)
            {
                this.LibraryCoreWorkItems.Add(new LibraryWorkItemViewModel()
                {
                    FileName = workItem.FileName,
                    Completed = workItem.ProcessingComplete,
                    Error = !workItem.ProcessingSuccessful,
                    LoadType = workItem.LoadType
                });
            }
        }
    }
    private void OnWorkItemCompleted(LibraryLoaderWorkItem workItem)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemCompleted);
        else
        {
            var item = this.LibraryCoreWorkItems.FirstOrDefault(x => x.FileName == workItem.FileName);

            if (item != null)
                item.Completed = true;

            else
                _outputController.AddLog("Cannot find work item in local cache:  {0}", LogMessageType.General, LogMessageSeverity.Error, workItem.FileName);
        }
    }
    private void OnRadioEntryLoaded(RadioEntry radioEntry)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnWorkItemCompleted);
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
                _outputController.AddLog("Radio Station already exists! {0}", LogMessageType.General, LogMessageSeverity.Error, radioEntry.Name);
            }
        }
    }
    private void OnLibraryEntryLoaded(LibraryEntry entry)
    {
        if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            Application.Current.Dispatcher.Invoke(OnLibraryEntryLoaded);
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
                _outputController.AddLog("Library Entry already exists! {0}", LogMessageType.General, LogMessageSeverity.Error, entry.FileName);
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

    private void OnLog(LogMessage message)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnLogImpl(message);

            }, DispatcherPriority.Background);
        }
        else
            OnLogImpl(message);
    }

    private void OnLogImpl(LogMessage message)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            throw new Exception("Trying to access MainViewModel on non-dispatcher thread");

        this.OutputMessages.Insert(0, new LogMessageViewModel()
        {
            Message = message.Message,
            Type = message.Type,
            Severity = message.Severity,
        });

        if (this.OutputMessages.Count > MAX_LOG_COUNT)
        {
            this.OutputMessages.RemoveAt(this.OutputMessages.Count - 1);
        }

        // Status Message (for now, last log message)
        this.StatusMessage = message.Message;
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
