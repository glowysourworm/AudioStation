using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

            // There's a Run inside the list box item template. So, there's probably a better way to avoid these casts; but this works for now.
            var trackViewModel = e.OriginalSource is FrameworkElement ? (e.OriginalSource as FrameworkElement).DataContext as IPlaylistEntryViewModel :
                                                                        (e.OriginalSource as FrameworkContentElement).DataContext as IPlaylistEntryViewModel;

            if (viewModel != null && trackViewModel != null)
            {
                // Loading...
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.ShowLoading("Loading Playlist..."));

                // Set Now Playing
                viewModel.SetNowPlaying(trackViewModel, true);

                // Loading Finished
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss(NavigationView.NowPlaying));
            }
        }
    }
}
