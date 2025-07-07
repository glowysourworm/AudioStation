using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Event;
using AudioStation.ViewModels;
using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class NowPlayingView : UserControl
    {
        IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public NowPlayingView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();
        }

        private void OnPlaylistDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as NowPlayingViewModel;
            var trackViewModel = (e.OriginalSource as FrameworkElement).DataContext as IPlaylistEntryViewModel;

            if (viewModel != null && trackViewModel != null)
            {
                // Loading...
                _eventAggregator.GetEvent<MainLoadingChangedEvent>().Publish(new MainLoadingChangedEventData(true, "Loading Playlist..."));

                // Set Now Playing
                viewModel.SetNowPlaying(trackViewModel, true);

                // Loading Finished
                _eventAggregator.GetEvent<MainLoadingChangedEvent>().Publish(new MainLoadingChangedEventData(NavigationView.NowPlaying));
            }
        }
    }
}
