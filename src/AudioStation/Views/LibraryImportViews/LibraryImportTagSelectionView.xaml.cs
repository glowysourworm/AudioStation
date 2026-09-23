using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.TagViewModels;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.LibraryImportViews
{
    public partial class LibraryImportTagSelectionView : UserControl
    {
        private readonly IAudioStationMapper _audioStationMapper;

        public static readonly DependencyProperty TagSourceProperty =
            DependencyProperty.Register("TagSource", typeof(LibraryImportSource), typeof(LibraryImportTagSelectionView));

        public LibraryImportSource TagSource
        {
            get { return (LibraryImportSource)GetValue(TagSourceProperty); }
            set { SetValue(TagSourceProperty, value); }
        }

        public LibraryImportTagSelectionView()
        {
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();

            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterFileViewModel;

            if (viewModel != null)
            {
                switch (this.TagSource)
                {
                    case LibraryImportSource.File:
                        _audioStationMapper.MapOnto(viewModel, viewModel.TagRecordDirty);
                        break;
                    case LibraryImportSource.DataService:
                    {
                        var selectedItem = this.MusicBrainzCB.SelectedItem as ComboBoxItem;

                        if (selectedItem != null)
                        {
                            var dataContext = selectedItem.DataContext as TagSmallViewModel;

                            if (dataContext != null)
                                _audioStationMapper.MapOnto(dataContext, viewModel.TagRecordDirty);
                        }
                    }
                    break;
                    default:
                        throw new Exception("Unhandled Library Import Source");
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
    }
}
