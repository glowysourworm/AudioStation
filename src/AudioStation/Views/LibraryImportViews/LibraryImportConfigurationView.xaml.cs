using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.ViewModels;

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
                    UpdateConfiguration(audioStationController);
                }
            });

            this.Loaded += (sender, e) => UpdateConfiguration(audioStationController);
        }

        private void UpdateConfiguration(IAudioStationController audioStationController)
        {
            var configuration = audioStationController.ComponentController.GetComponent<AudioStationConfigurationViewModel>();

            // Initial Configuration
            this.LibraryDirectoriesView.ItemsSource = configuration.LibraryDirectories;
            this.LibraryDirectoriesView.Encoders = audioStationController.ComponentController.GetComponent<MainViewModel>().Encoders;

            this.LibraryDirectoriesView.ItemsSource = configuration.LibraryDirectories;
            this.LibraryDirectoriesCB.ItemsSource = configuration.LibraryDirectories;
        }
    }
}
