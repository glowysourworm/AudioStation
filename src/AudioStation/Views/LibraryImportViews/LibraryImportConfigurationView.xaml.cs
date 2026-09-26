using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.DataComponent;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportConfigurationView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportConfigurationView(IIocEventAggregator eventAggregator, IAudioStationController audioStationController)
        {
            InitializeComponent();

            // Configuration
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                // TODO: Make this for a "application ready" broadcast, not just the configuration
                if (eventData.Type == ConfigurationEventType.Modified ||
                    eventData.Type == ConfigurationEventType.Opened)
                {
                    // Initial Configuration
                    if (eventData.ViewModel != null)
                    {
                        this.LibraryDirectoriesView.ItemsSource = eventData.ViewModel.LibraryDirectories;
                        this.LibraryDirectoriesView.Encoders = audioStationController.ComponentController.GetDataComponent<MainViewModel>().Encoders;

                        this.LibraryDirectoriesView.ItemsSource = eventData.ViewModel.LibraryDirectories;
                        this.LibraryDirectoriesCB.ItemsSource = eventData.ViewModel.LibraryDirectories;
                    }
                }
            });

            this.Loaded += (sender, e) => UpdateConfiguration(audioStationController);
        }

        private void UpdateConfiguration(IAudioStationController audioStationController)
        {
            if (!audioStationController.Initialized)
                return;

            var configuration = audioStationController.ComponentController.GetDataComponent<AudioStationConfigurationViewModel>();

            // Initial Configuration
            this.LibraryDirectoriesView.ItemsSource = configuration.LibraryDirectories;
            this.LibraryDirectoriesView.Encoders = audioStationController.ComponentController.GetDataComponent<MainViewModel>().Encoders;

            this.LibraryDirectoriesView.ItemsSource = configuration.LibraryDirectories;
            this.LibraryDirectoriesCB.ItemsSource = configuration.LibraryDirectories;
        }
    }
}
