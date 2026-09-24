using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Component.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.TagViewModels;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.LibraryImportViews
{
    public partial class LibraryImportTagSelectionView : UserControl
    {
        private readonly IAudioStationMapper _audioStationMapper;

        private const string SOURCE_FILE_ITEM = "Source File";
        private const string SOURCE_MUSIC_BRAINZ = "Source Music Brainz (from tag)";
        private const string SOURCE_DATA_SERVICE = "Data Service";

        public LibraryImportTagSelectionView()
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            InitializeComponent();

            this.DataContextChanged += LibraryImportTagSelectionView_DataContextChanged;
        }

        private void LibraryImportTagSelectionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterFileViewModel;

            if (viewModel != null)
            {
                this.TagSourceCB.Items.Clear();
                this.TagSourceCB.Items.Add(SOURCE_FILE_ITEM);

                // Data Service (Music Brainz Tag)
                if (viewModel.MusicBrainzReleaseTrackQuerySuccess)
                    this.TagSourceCB.Items.Add(SOURCE_MUSIC_BRAINZ);

                this.TagSourceCB.Items.Add(SOURCE_DATA_SERVICE);

                // Initialize
                this.TagSourceCB.SelectedIndex = 0;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterFileViewModel;

            if (viewModel != null)
            {
                switch (this.TagSourceCB.SelectedItem)
                {
                    // File
                    case SOURCE_FILE_ITEM:
                        _audioStationMapper.MapOnto(viewModel.Tag, viewModel.TagRecordDirty);
                        break;

                    // Data Service (Music Brainz Tag)
                    case SOURCE_MUSIC_BRAINZ:
                        if (viewModel.MusicBrainzReleaseTrackQuerySuccess)
                            _audioStationMapper.MapOnto(viewModel.TagMusicBrainz, viewModel.TagRecordDirty);
                        break;

                    // Data Service (AcoustID -> Music Brainz)
                    case SOURCE_DATA_SERVICE:

                        var selectedItem = this.MusicBrainzCB.SelectedItem as TagSmallViewModel;

                        if (selectedItem != null)
                            _audioStationMapper.MapOnto(selectedItem, viewModel.TagRecordDirty);

                        break;
                }
            }
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterFileViewModel;

            if (viewModel != null)
            {
                _audioStationMapper.MapOnto(viewModel.TagRecordClean, viewModel.TagRecordDirty);
            }
        }

        private void TagSourceCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterFileViewModel;

            if (viewModel != null)
            {
                switch (this.TagSourceCB.SelectedItem)
                {
                    // File
                    case SOURCE_FILE_ITEM:
                        this.MusicBrainzCB.Visibility = Visibility.Collapsed;
                        this.MusicBrainzTB.Visibility = Visibility.Collapsed;
                        this.MusicBrainzView.Visibility = Visibility.Collapsed;
                        this.TagFileView.Visibility = Visibility.Visible;
                        break;

                    // Data Service (Music Brainz Tag)
                    case SOURCE_MUSIC_BRAINZ:
                        this.MusicBrainzCB.Visibility = Visibility.Visible;
                        this.MusicBrainzTB.Visibility = Visibility.Visible;
                        this.MusicBrainzView.Visibility = Visibility.Visible;
                        this.TagFileView.Visibility = Visibility.Collapsed;

                        if (viewModel.MusicBrainzReleaseTrackQuerySuccess)
                        {
                            this.MusicBrainzCB.ItemsSource = new TagSmallViewModel[] { viewModel.TagMusicBrainz };
                            this.MusicBrainzCB.SelectedIndex = 0;
                        }
                        break;

                    // Data Service (AcoustID -> Music Brainz)
                    case SOURCE_DATA_SERVICE:
                        this.MusicBrainzCB.Visibility = Visibility.Visible;
                        this.MusicBrainzTB.Visibility = Visibility.Visible;
                        this.MusicBrainzView.Visibility = Visibility.Visible;
                        this.TagFileView.Visibility = Visibility.Collapsed;

                        this.MusicBrainzCB.ItemsSource = viewModel.ImportOutput.MusicBrainzAcoustIDResults;
                        this.MusicBrainzCB.SelectedIndex = 0;
                        break;
                }
            }
        }

        private void MusicBrainzCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MusicBrainzView.DataContext = this.MusicBrainzCB.SelectedItem;
        }
    }
}
