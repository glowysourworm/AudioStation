using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. 
    /// </summary>
    public abstract class DataComponentViewModelBase : ViewModelBase, IDisposable
    {
        Guid _id;
        bool _initialized;
        bool _loading;
        string _displayName;

        public Guid Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public bool Initialized
        {
            get { return _initialized; }
            private set { this.RaiseAndSetIfChanged(ref _initialized, value); }
        }
        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            private set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public DataComponentViewModelBase(string displayName)
        {
            this.Id = Guid.NewGuid();
            this.Initialized = false;
            this.DisplayName = displayName;
        }

        public abstract void Initialize(IAudioStationConfiguration configuration);
        public abstract void Dispose();
    }
}
