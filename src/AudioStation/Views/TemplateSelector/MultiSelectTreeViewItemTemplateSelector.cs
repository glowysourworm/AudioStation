using System.Windows;
using System.Windows.Controls;

using SimpleWpf.ViewModel;

using Xceed.Wpf.Toolkit.Core.Utilities;

namespace AudioStation.Views.TemplateSelector
{
    public class MultiSelectTreeViewItemTemplateSelector : DataTemplateSelector
    {
        public MultiSelectTreeViewItemTemplateSelector()
        {
        }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var listBoxItem = VisualTreeHelperEx.FindAncestorByType<ListBoxItem>(container);

            if (listBoxItem == null)
                throw new NullReferenceException("Improper handling of MultiSelectTreeViewItemTemplateSelector");

            var viewModel = listBoxItem.DataContext as RecursiveDispatcherViewModel<PathViewModel>;

            if (viewModel == null)
                throw new NullReferenceException("Improper handling of MultiSelectTreeViewItemTemplateSelector");

            //if (!viewModel.Children.None())
            //    return listBoxItem.FindResource("MultiSelectTreeViewCollectionItemTemplate") as DataTemplate;

            //else
            //{
            if (!viewModel.NodeValue.IsDirectory)
                return listBoxItem.FindResource("MultiSelectTreeViewLeafNoIndentItemTemplate") as DataTemplate;
            else //if (!viewModel.Children.None())
                return listBoxItem.FindResource("MultiSelectTreeViewCollectionItemTemplate") as DataTemplate;
            //else
            //    return listBoxItem.FindResource("MultiSelectTreeViewLeafItemTemplate") as DataTemplate;
            //}

        }
    }
}
