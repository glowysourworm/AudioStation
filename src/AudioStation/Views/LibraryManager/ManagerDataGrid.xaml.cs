using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels;

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

        // Row Double Click -> Add File Tab Item
        private void LibraryEntryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as LibraryViewModel;
            var selectedItem = this.LibraryEntryGrid.SelectedItem as LibraryEntryViewModel;

            if (selectedItem != null && viewModel != null)
                viewModel.AddLibraryEntryTabCommand.Execute(selectedItem);
        }
    }
}
