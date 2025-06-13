using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using AudioStation.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.ViewModels;

using EMA.ExtendedWPFVisualTreeHelper;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        private readonly IViewModelLoader _viewModelLoader;
        int _pageNumber = 0;

        // Designer only
        public ManagerView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public ManagerView(IViewModelLoader viewModelLoader)
        {
            _viewModelLoader = viewModelLoader;

            InitializeComponent();

            this.DataContextChanged += ManagerView_DataContextChanged;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void ManagerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (viewModel != null)
            {
                if (viewModel.LibraryEntries.Count == 0)
                    LoadPage(1, true);
            }
        }

        private void LoadPage(int pageNumber, bool reset)
        {
            _pageNumber = pageNumber;

            var viewModel = this.DataContext as LibraryViewModel;

            if (viewModel != null)
            {
                var result = _viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = pageNumber,
                    PageSize = 30,                              // TODO: Observable collections don't work w/ the view
                    OrderByCallback = (entity) => entity.Id,
                    //WhereCallback = !string.IsNullOrWhiteSpace(viewModel.ArtistSearch) ? this.ArtistContainsCallback : null
                });

                if (result.Results.Any())
                {
                    viewModel.LoadEntryPage(result, reset);
                }
            }
        }
        private void LibraryEntryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = WpfVisualFinders.FindChild<ScrollViewer>(sender as DependencyObject);
            var viewModel = this.DataContext as LibraryViewModel;

            if (scrollViewer != null && viewModel != null)
            {
                if (scrollViewer.VerticalOffset >= (0.8 * scrollViewer.ScrollableHeight))
                {
                    //LoadPage(_pageNumber + 1, false);
                }
            }
        }
    }
}
