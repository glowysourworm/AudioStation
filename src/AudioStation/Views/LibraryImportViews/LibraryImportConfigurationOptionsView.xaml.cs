using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportConfigurationOptionsView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportConfigurationOptionsView(IIocEventAggregator eventAggregator, IAudioStationController audioStationController)
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
                        this.AudioFormatCB.ItemsSource = audioStationController.ComponentController.GetComponent<MainViewModel>().Encoders;
                    }
                }
            });

            this.Loaded += (sender, e) => UpdateConfiguration(audioStationController);
        }

        private void UpdateConfiguration(IAudioStationController audioStationController)
        {
            if (!audioStationController.Initialized)
                return;

            this.AudioFormatCB.ItemsSource = audioStationController.ComponentController.GetComponent<MainViewModel>().Encoders;
        }
    }
}
