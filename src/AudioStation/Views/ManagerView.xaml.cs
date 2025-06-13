using System.Windows;
using System.Windows.Controls;

using AudioStation.Component.Interface;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        private readonly IViewModelLoader _viewModelLoader;

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
                // Invoke main pager request
                if (viewModel.LibraryEntries.Count == 0)
                {
                    viewModel.LibraryEntryRequestPage = 1;
                    viewModel.LibraryEntryPageRequestCommand.Execute(1);
                }
            }
        }
    }
}
