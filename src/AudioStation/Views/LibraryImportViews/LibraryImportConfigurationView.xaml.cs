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
        public LibraryImportConfigurationView(IIocEventAggregator eventAggregator, IAudioStationViewModelController audioStationViewModelController)
        {
            InitializeComponent();

            // Configuration
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                // TODO: Make this for a "application ready" broadcast, not just the configuration
                if (eventData.Type == ConfigurationEventType.Opened)
                {
                    // Initial Configuration
                    this.LibraryDirectoriesView.ItemsSource = audioStationViewModelController.GetComponent<AudioStationConfigurationViewModel>().LibraryDirectories;
                    this.LibraryDirectoriesView.Encoders = audioStationViewModelController.GetComponent<MainViewModel>().Encoders;

                    this.LibraryDirectoriesView.ItemsSource = eventData.ViewModel.LibraryDirectories;
                    this.LibraryDirectoriesCB.ItemsSource = eventData.ViewModel.LibraryDirectories;
                }
            });
        }
    }
}
