using System.Windows.Controls;

using AudioStation.Event.DialogEvents;

namespace AudioStation.Views.DialogViews
{
    public partial class SelectionView : UserControl
    {
        public SelectionView()
        {
            InitializeComponent();
        }

        private void TheLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as DialogSelectionListViewModel;

            if (viewModel != null)
            {
                foreach (var item in viewModel.SelectionList)
                {
                    item.Selected = this.TheLB.SelectedItems.Contains(item);
                }
            }
        }
    }
}
