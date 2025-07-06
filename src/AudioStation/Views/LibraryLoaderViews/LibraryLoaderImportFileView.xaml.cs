using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AudioStation.ViewModels.LibraryLoaderViewModels;

namespace AudioStation.Views.LibraryLoaderViews
{
    /// <summary>
    /// Interaction logic for LibraryLoaderImportFileView.xaml
    /// </summary>
    public partial class LibraryLoaderImportFileView : UserControl
    {
        public LibraryLoaderImportFileView()
        {
            InitializeComponent();
        }

        private void InputLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null)
            {
                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsSelected = this.InputLB.SelectedItems.Contains(item);
                }
            }
        }
    }
}
