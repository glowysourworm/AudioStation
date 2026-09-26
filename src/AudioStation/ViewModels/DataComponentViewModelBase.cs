using System.Windows.Threading;

using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;
using SimpleWpf.Utilities;

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

        protected abstract void InitializeWork(IAudioStationConfiguration configuration);

        public void Initialize(IAudioStationConfiguration configuration)
        {
            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Initialize, DispatcherPriority.Background, configuration);

            else
            {
                InitializeWork(configuration);

                // To be used by subclasses
                this.Initialized = true;
            }
        }

        public abstract void Dispose();
    }
}
