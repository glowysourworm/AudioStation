using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.ViewModel;
using AudioStation.ViewModels.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public const string CONFIGURATION_FILE = ".AudioStation";
    public const string RADIO_FILE = ".AudioStationRadio";

    const int MAX_LOG_COUNT = 1000;

    #region Backing Fields
    Configuration _configuration;
    string _statusMessage;
    bool _showOutputMessages;
    float _volume;

    INowPlayingViewModel _nowPlayingViewModel;

    ObservableCollection<LogMessageViewModel> _outputMessages;

    SimpleCommand _saveCommand;
    SimpleCommand _openCommand;
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
    public SimpleCommand<string> SearchRadioBrowserCommand
    {
        get { return _searchRadioBrowserCommand; }
        set { this.RaiseAndSetIfChanged(ref _searchRadioBrowserCommand, value); }
    }
    #endregion

    [IocImportingConstructor]
    public MainViewModel(IDialogController dialogController,
                         IAudioController audioController,
                         IModelController modelController,
                         ILibraryLoader libraryLoader)
    {
        this.Configuration = new Configuration();
        this.ShowOutputMessages = false;
        this.OutputMessages = new ObservableCollection<LogMessageViewModel>();
        this.NowPlayingViewModel = null;

        OnLog("Welcome to Audio Player!");

        libraryLoader.LogEvent += (message, error) =>
        {
            OnLog(message, LogMessageType.General, error ? LogMessageSeverity.Error : LogMessageSeverity.Info);
        };

        this.SaveCommand = new SimpleCommand(() =>
        {
            Save();
        });
        this.OpenCommand = new SimpleCommand(() =>
        {
            Open();
        });
        this.SearchRadioBrowserCommand = new SimpleCommand<string>((search) =>
        {
            SearchRadioBrowser(search);
        });

        audioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        audioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;
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
        }
        catch (Exception ex)
        {
            OnLog("Error reading configuration file. Please try saving the working configuration first and then restarting.");
        }
    }

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
            OnLog("Error querying Radio Browser:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
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
