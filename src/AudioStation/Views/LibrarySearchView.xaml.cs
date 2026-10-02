using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.ServiceComponent;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibrarySearchView : UserControl
    {
        private readonly ILibraryLoaderService _libraryLoaderService;
        private readonly IIocEventAggregator _eventAggregator;

        public LibrarySearchView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public LibrarySearchView(ILibraryLoaderService libraryLoaderService,
                                 IIocEventAggregator eventAggregator)
        {
            _libraryLoaderService = libraryLoaderService;
            _eventAggregator = eventAggregator;

            InitializeComponent();

            this.DataContextChanged += LibrarySearchView_DataContextChanged;
        }

        private void LibrarySearchView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibrarySearchViewModel;

            // Service Component 
            if (viewModel != null)
            {
                // -> Load
                if (viewModel.CanLoad())
                    viewModel.LoadCommand.Execute(null);

                // -> Execute
                if (viewModel.CanExecute())
                    viewModel.ExecuteCommand.Execute(null);
            }

        }

        private void ResultsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load Selected Album(s) into the AlbumDetailLB

            if (e.AddedItems != null &&
                e.AddedItems.Count > 0)
            {
                //this.AlbumDetailLB.ScrollIntoView(e.AddedItems[0]);
            }
        }

        private void ResultsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (e.OriginalSource as FrameworkElement).DataContext as ArtistViewModel;

            if (viewModel != null)
            {
                var nowPlaying = _libraryLoaderService.GetDefaultPlaylist(viewModel);

                // Load Playlist -> Start Playback
                if (nowPlaying.CurrentTrack != null)
                {
                    _eventAggregator.GetEvent<LoadPlaybackEvent>().Publish(new LoadPlaybackEventData()
                    {
                        Source = nowPlaying.CurrentTrack.Track.FileName,
                        SourceType = StreamSourceType.File
                    });
                    _eventAggregator.GetEvent<StartPlaybackEvent>().Publish();
                }
            }
        }
    }
}
