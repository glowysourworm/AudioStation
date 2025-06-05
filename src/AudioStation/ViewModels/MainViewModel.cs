using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Model;
using AudioStation.Model.Database;
using AudioStation.ViewModel;
using AudioStation.ViewModels.Interface;
using AudioStation.ViewModels.RadioViewModel;

using m3uParser;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.NativeIO.FastDirectory;

namespace AudioStation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Could move these to dependency injection; but too many other issues
    public static IDialogController DialogController { get; private set; }
    public static IAudioController AudioController { get; private set; }

    public const string CONFIGURATION_FILE = ".AudioStation";
    public const string RADIO_FILE = ".AudioStationRadio";

    // Some View Properties
    public static SolidColorBrush DefaultMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
    public static SolidColorBrush ValidMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 220, 255, 220));
    public static SolidColorBrush InvalidMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 255, 220, 220));

    const int MAX_LOG_COUNT = 1000;

    Library _library;
    Configuration _configuration;
    Radio _radio;
    string _statusMessage;
    bool _showOutputMessages;

    Playlist _playlist;
    float _volume;

    INowPlayingViewModel _nowPlayingViewModel;

    ObservableCollection<LogMessageViewModel> _outputMessages;

    SimpleCommand _saveCommand;
    SimpleCommand _openCommand;
    SimpleCommand _importM3UFileCommand;
    SimpleCommand _importM3UDirectoryCommand;
    SimpleCommand<string> _searchRadioBrowserCommand;


    public Library Library
    {
        get { return _library; }
        set { this.RaiseAndSetIfChanged(ref _library, value); }
    }
    public Configuration Configuration
    {
        get { return _configuration; }
        set { this.RaiseAndSetIfChanged(ref _configuration, value); }
    }
    public Radio Radio
    {
        get { return _radio; }
        set { this.RaiseAndSetIfChanged(ref _radio, value); }
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
    public Playlist Playlist
    {
        get { return _playlist; }
        set { this.RaiseAndSetIfChanged(ref _playlist, value); }
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
    public SimpleCommand SaveCommand
    {
        get { return _saveCommand; }
        set { this.RaiseAndSetIfChanged(ref _saveCommand, value); }
    }
    public SimpleCommand OpenCommand
    {
        get { return _openCommand; }
        set { this.RaiseAndSetIfChanged(ref _openCommand, value); }
    }
    public SimpleCommand ImportM3UFileCommand
    {
        get { return _importM3UFileCommand; }
        set { this.RaiseAndSetIfChanged(ref _importM3UFileCommand, value); }
    }
    public SimpleCommand ImportM3UDirectoryCommand
    {
        get { return _importM3UDirectoryCommand; }
        set { this.RaiseAndSetIfChanged(ref _importM3UDirectoryCommand, value); }
    }
    public SimpleCommand<string> SearchRadioBrowserCommand
    {
        get { return _searchRadioBrowserCommand; }
        set { this.RaiseAndSetIfChanged(ref _searchRadioBrowserCommand, value); }
    }

    public MainViewModel(IDialogController dialogController, IAudioController audioController)
    {
        MainViewModel.DialogController = dialogController;
        MainViewModel.AudioController = audioController;

        this.Library = new Library();
        this.Configuration = new Configuration();
        this.Radio = new Radio();
        this.ShowOutputMessages = false;
        this.OutputMessages = new ObservableCollection<LogMessageViewModel>();
        this.NowPlayingViewModel = null;

        OnLog("Welcome to Audio Player!");

        this.Library.LogEvent += (message, type, severity) =>
        {
            OnLog(message, type, severity);
        };
        this.Configuration.LibraryConfiguration.PropertyChanged += OnConfigurationChanged;

        this.SaveCommand = new SimpleCommand(() =>
        {
            Save();
        });
        this.OpenCommand = new SimpleCommand(() =>
        {
            Open();
        });
        this.ImportM3UFileCommand = new SimpleCommand(() =>
        {
            ImportM3UFile();
        });
        this.ImportM3UDirectoryCommand = new SimpleCommand(async () =>
        {
            var radioStreams = await ImportM3UDirectory();

            foreach (var stream in radioStreams)
                this.Radio.RadioStreams.Add(stream);
        });
        this.SearchRadioBrowserCommand = new SimpleCommand<string>((search) =>
        {
            SearchRadioBrowser(search);
        });

        // Dependency Injection Needed:  too many other issues right now
        MainViewModel.AudioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        MainViewModel.AudioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;
    }

    ~MainViewModel()
    {
        // Dependency Injection Needed:  too many other issues right now
        MainViewModel.AudioController.PlaybackStartedEvent -= OnAudioControllerPlaybackStarted;
        MainViewModel.AudioController.PlaybackStoppedEvent -= OnAudioControllerPlaybackStopped;
    }

    private void OnAudioControllerPlaybackStopped(INowPlayingViewModel nowPlaying)
    {
        this.NowPlayingViewModel = null;        // Remove View Model (affects view binding to the player controls)
    }

    private void OnAudioControllerPlaybackStarted(INowPlayingViewModel nowPlaying)
    {
        this.NowPlayingViewModel = nowPlaying;  // Primary settings for the view (binding)
    }

    public void Save()
    {
        try
        {
            // Current working directory + configuration file name
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_FILE);

            // Configuration
            Serializer.Serialize(this.Configuration, configPath);

            OnLog("Configuration saved successfully: {0}", LogMessageType.General, LogMessageSeverity.Info, configPath);

            // Database
            var database = new LibraryDatabase(this.Library);

            database.Save(this.Configuration.LibraryDatabaseFile);

            OnLog("Library database saved successfully: {0}", LogMessageType.General, LogMessageSeverity.Info, this.Configuration.LibraryDatabaseFile);

            // Radio
            var radioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RADIO_FILE);

            Serializer.Serialize(this.Radio, radioPath);

            OnLog("Radio database saved successfully: {0}", LogMessageType.General, LogMessageSeverity.Info, radioPath);
        }
        catch (Exception ex)
        {
            OnLog("Error saving configuration / data files:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }
    }
    public void Open()
    {
        try
        {
            // Current working directory + configuration file name
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_FILE);

            this.Configuration = Serializer.Deserialize<Configuration>(configPath);

            OnLog("Configuration read successfully!");

            // Library
            var database = LibraryDatabase.Open(this.Configuration.LibraryDatabaseFile);

            OnLog("Library database read successfully! Opening library...");

            this.Library = database.CreateLibrary();

            // Radio
            var radioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RADIO_FILE);

            this.Radio = Serializer.Deserialize<Radio>(radioPath);

            OnLog("Radio database read successfully!");
        }
        catch (Exception ex)
        {
            OnLog("Error reading configuration file. Please try saving the working configuration first and then restarting.");
        }
    }
    public void ImportM3UFile()
    {
        try
        {
            var fileName = MainViewModel.DialogController.ShowSelectFile();

            if (!string.IsNullOrEmpty(fileName))
            {
                var entryName = Path.GetFileNameWithoutExtension(fileName);
                var fileData = File.ReadAllText(fileName);
                var entry = ReadM3U(fileData, entryName);

                if (entry != null)
                    this.Radio.RadioStreams.Add(entry);
            }
        }
        catch (Exception ex)
        {
            OnLog("Error reading m3u file:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }
    }
    public Task<IEnumerable<RadioEntry>> ImportM3UDirectory()
    {
        try
        {
            var directory = MainViewModel.DialogController.ShowSelectFolder();

            return Task<IEnumerable<RadioEntry>>.Run(() =>
            {
                var result = new List<RadioEntry>();

                if (!string.IsNullOrEmpty(directory))
                {
                    foreach (var file in FastDirectoryEnumerator.GetFiles(directory, "*.m3u", SearchOption.AllDirectories))
                    {
                        var entryName = Path.GetFileNameWithoutExtension(file.Path);
                        var fileData = File.ReadAllText(file.Path);
                        var entry = ReadM3U(fileData, entryName);

                        if (entry != null)
                            result.Add(entry);
                    }
                }

                return result as IEnumerable<RadioEntry>;
            });
        }
        catch (Exception ex)
        {
            OnLog("Error reading m3u file:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }

        return (Task<IEnumerable<RadioEntry>>)Task<IEnumerable<RadioEntry>>.Delay(0);
    }
    public async void SearchRadioBrowser(string search)
    {
        try
        {
            var streams = await RadioBrowserSearchComponent.SearchStation(search);
            //var streams = await RadioBrowserSearchComponent.GetTopStations(10);

            if (streams.Count == 0)
                return;

            this.Radio.RadioStations.Clear();
            this.Radio.RadioStations.AddRange(streams.Where(stream => stream != null && stream.Bitrate > 0 && !string.IsNullOrEmpty(stream.Codec))
                                                     .Select(stream =>
            {
                return new RadioStationViewModel()
                {
                    Bitrate = stream.Bitrate,
                    Codec = stream.Codec,
                    Name = stream.Name,
                    Homepage = stream.Homepage.ToString(),
                    Endpoint = stream.Url.ToString(),
                    LogoEndpoint = stream.Favicon.ToString()
                };
            }));
        }
        catch (Exception ex)
        {
            OnLog("Error querying Radio Browser:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }
    }

    private RadioEntry ReadM3U(string fileContents, string fileNameOrStreamName)
    {
        // Adding a nested try / catch for these files
        try
        {
            var m3uFile = M3U.Parse(fileContents);

            if (m3uFile != null)
            {
                OnLog("M3U file read successfully! Adding m3u stream:  {0}", LogMessageType.General, LogMessageSeverity.Info, fileNameOrStreamName);

                return new RadioEntry()
                {
                    Name = fileNameOrStreamName,
                    RadioPlaylist = new ObservableCollection<RadioEntryStreamInfo>(m3uFile.Medias.Select(media => new RadioEntryStreamInfo()
                    {
                        Title = media.Title.RawTitle,
                        Endpoint = media.MediaFile,
                        LogoEndpoint = media.Attributes.TvgLogo
                    }))
                };
            }
        }
        catch (Exception ex)
        {
            OnLog("Error reading m3u file:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
        }

        return null;
    }

    private async void OnConfigurationChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // LibraryConfiguration -> DirectoryBase (rescan)
        //
        if (e.PropertyName == "DirectoryBase")
        {
            OnLog("Library Base Directory Changed. Scanning for music files...");

            var libraryEntries = await LibraryLoader.Load(this.Configuration.LibraryConfiguration, (message, severity) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnLog(message, LogMessageType.General, severity);
                });
            });

            foreach (var entry in libraryEntries)
            {
                this.Library.Add(entry);
            }
        }
    }

    private void OnLog(string message,
                        LogMessageType type = LogMessageType.General,
                        LogMessageSeverity severity = LogMessageSeverity.Info,
                        params object[] parameters)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnLogImpl(message, type, severity, parameters);

            }, DispatcherPriority.Background);
        }
        else
            OnLogImpl(message, type, severity, parameters);
    }

    private void OnLogImpl(string message,
                           LogMessageType type = LogMessageType.General,
                           LogMessageSeverity severity = LogMessageSeverity.Info,
                           params object[] parameters)
    {
        if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            throw new Exception("Trying to access MainViewModel on non-dispatcher thread");

        this.OutputMessages.Insert(0, new LogMessageViewModel()
        {
            Message = string.Format(message, parameters),
            Type = type,
            Severity = severity,
        });

        if (this.OutputMessages.Count > MAX_LOG_COUNT)
        {
            this.OutputMessages.RemoveAt(this.OutputMessages.Count - 1);
        }

        // Status Message (for now, last log message)
        this.StatusMessage = string.Format(message, parameters);
    }
}
