using System.Windows.Controls;

using AudioStation.Views.TemplateSelector;

namespace AudioStation.Views.Behavior
{
    public class ListBoxResponsiveBehavior : ResponsiveBehaviorBase<ListBox>
    {
        ResponsiveSize? _responsiveSize;

        protected override void OnResponsiveSizeChanged(ResponsiveSize responsiveSize)
        {
            this.AssociatedObject.ItemTemplateSelector = null;
            this.AssociatedObject.ItemTemplateSelector = new AlbumViewTemplateSelector(responsiveSize);
        }
    }
}
