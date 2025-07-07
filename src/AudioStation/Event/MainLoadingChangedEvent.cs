using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public enum NavigationView
    {
        None,
        Configuration,
        LibraryLoader,
        LibraryManager,
        ArtistSearch,
        NowPlaying,
        Radio,
        RadioBrowser,
        Bandcamp,
        Log
    }

    public class MainLoadingChangedEventData
    {
        public bool IsLoading { get; set; }
        public bool ShowProgressBar { get; set; }
        public double Progress { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// View for user navigation
        /// </summary>
        public NavigationView RequestedView { get; set; }

        public MainLoadingChangedEventData(bool isLoading = false)
            : this(isLoading, "") { }

        public MainLoadingChangedEventData(NavigationView requestedView, bool isLoading = false)
            : this(isLoading, "", requestedView) { }

        public MainLoadingChangedEventData(bool isLoading, string message)
            : this(isLoading, false, 0, message, NavigationView.None) { }

        public MainLoadingChangedEventData(bool isLoading, string message, NavigationView requestedView)
            : this(isLoading, false, 0, message, requestedView) { }

        public MainLoadingChangedEventData(bool isLoading, bool showProgressBar, double progress, string message, NavigationView requestedView)
        {
            this.IsLoading = isLoading;
            this.Progress = progress;
            this.Message = message;
            this.ShowProgressBar = showProgressBar;
            this.RequestedView = requestedView;
        }
    }

    /// <summary>
    /// Primary loading event corresponding to the MainViewModel.Loading indicator boolean. This will
    /// be utilized to hide / show loading UI and cursor.
    /// </summary>
    public class MainLoadingChangedEvent : IocEvent<MainLoadingChangedEventData>
    {
    }
}
