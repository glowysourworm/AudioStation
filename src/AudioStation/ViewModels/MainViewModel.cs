using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Model;
using AudioStation.Model.Database;
using AudioStation.ViewModel;
using AudioStation.ViewModel.LibraryViewModel;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;

namespace AudioStation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Could move these to dependency injection; but too many other issues
    public static IDialogController DialogController { get; private set; }
    public static IAudioController AudioController { get; private set; }

    public const string CONFIGURATION_FILE = ".AudioStation";

    // Some View Properties
    public static SolidColorBrush DefaultMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
    public static SolidColorBrush ValidMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 220, 255, 220));
    public static SolidColorBrush InvalidMusicBrainzBackground = new SolidColorBrush(Color.FromArgb(255, 255, 220, 220));

    const int MAX_LOG_COUNT = 1000;

    Library _library;
    Configuration _configuration;
    string _statusMessage;
    bool _showOutputMessages;

    Playlist _playlist;
    float _volume;

    ObservableCollection<LogMessageViewModel> _outputMessages;

    SimpleCommand _saveCommand;
    SimpleCommand _openCommand;

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

    public MainViewModel(IDialogController dialogController, IAudioController audioController)
    {
        MainViewModel.DialogController = dialogController;
        MainViewModel.AudioController = audioController;

        this.Library = new Library();
        this.Configuration = new Configuration();
        this.ShowOutputMessages = false;
        this.OutputMessages = new ObservableCollection<LogMessageViewModel>();

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

    private void OnAudioControllerTrackChanged(Playlist payload, TitleViewModel nowPlaying)
    {

    }

    private void OnAudioControllerPlaybackStopped(Playlist payload)
    {

    }

    private void OnAudioControllerPlaybackStarted(Playlist payload, TitleViewModel nowPlaying)
    {

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

            var database = LibraryDatabase.Open(this.Configuration.LibraryDatabaseFile);

            OnLog("Library database read successfully! Opening library...");

            this.Library = database.CreateLibrary();
        }
        catch (Exception ex)
        {
            OnLog("Error reading configuration file. Please try saving the working configuration first and then restarting.");
        }
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
