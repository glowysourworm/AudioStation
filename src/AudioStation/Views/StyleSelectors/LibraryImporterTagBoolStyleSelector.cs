using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Views.StyleSelectors
{
    public class LibraryImporterTagBoolStyleSelector : StyleSelector
    {
        public string ValidStyleResourceName { get; set; }
        public string InvalidStyleResourceName { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (element == null)
                throw new NullReferenceException("Improper handling of LibraryImporterTagBoolStyleSelector");

            var tagBool = (bool)element.Tag;

            // Valid
            if (tagBool)
                return (Style)element.FindResource(this.ValidStyleResourceName);

            // Invalid
            else
                return (Style)element.FindResource(this.InvalidStyleResourceName);
        }
    }
}
