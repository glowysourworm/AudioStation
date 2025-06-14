using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using AudioStation.ViewModels;

namespace AudioStation.Views.LibraryManager
{
    public partial class ManagerDataGrid : UserControl
    {
        public ManagerDataGrid()
        {
            InitializeComponent();

            this.DataContextChanged += ManagerDataGrid_DataContextChanged;
        }

        private void ManagerDataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
