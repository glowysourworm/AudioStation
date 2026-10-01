using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Views.TemplateSelector
{
    public class AlbumViewTemplateSelector : DataTemplateSelector
    {
        private readonly ResponsiveSize _currentSize;

        /// <summary>
        /// The only way to re-trigger this selector is essentially to re-create it.. until there
        /// is a way to re-create the list box items. (there may be a refresh way)
        /// </summary>
        public AlbumViewTemplateSelector(ResponsiveSize currentSize)
        {
            _currentSize = currentSize;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (_currentSize)
            {
                case ResponsiveSize.Small:
                    return Application.Current.FindResource("AlbumViewSmallTemplate") as DataTemplate;

                case ResponsiveSize.Medium:
                    return Application.Current.FindResource("AlbumViewMediumTemplate") as DataTemplate;

                case ResponsiveSize.Large:
                default:
                    return Application.Current.FindResource("AlbumViewLargeTemplate") as DataTemplate;
            }
        }
    }
}
