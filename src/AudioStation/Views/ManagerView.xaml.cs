using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using AudioStation.Component.Interface;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        [IocImportingConstructor]
        public ManagerView()
        {
            InitializeComponent();

            this.DataContextChanged += ManagerView_DataContextChanged;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void ManagerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Set header bindings
            foreach (var column in this.LibraryEntryGrid.Columns)
            {
                if (column.GetType() != typeof(DataGridTextColumn))
                    continue;

                // Bind each column to the LibraryEntrySearch of the data context
                BindingOperations.SetBinding(column, DataGridColumn.HeaderProperty, new Binding("LibraryEntrySearch")
                {
                    Source = this.DataContext
                });
            }

            ExecuteSearch();
        }

        private void OnLibraryManagerFilterChanged(object sender, RoutedEventArgs e)
        {
            ExecuteSearch();
        }

        private void ExecuteSearch()
        {
            var viewModel = this.DataContext as LibraryViewModel;

            if (viewModel != null)
            {
                // Invoke main pager request
                viewModel.LibraryEntryRequestPage = 1;
                viewModel.LibraryEntryPageRequestCommand.Execute(1);
            }
        }
    }
}
