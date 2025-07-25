using System.Windows;
using System.Windows.Controls;

using EMA.ExtendedWPFVisualTreeHelper;

namespace AudioStation.Controls.PropertyGrid
{
    public class PropertyGridTabControl : TabControl
    {
        public PropertyGridTabControl()
        {
        }
        
        public void CommitChanges()
        {
            var propertyGridControls = GetPropertyGridChildren();

            foreach (var control in propertyGridControls)
            {
                control.CommitChanges();
            }
        }

        public bool Validate()
        {
            var propertyGridControls = GetPropertyGridChildren();
            var result = true;

            foreach (var control in propertyGridControls)
            {
                result &= control.Validate();
            }

            return result;
        }

        private IEnumerable<PropertyGridControl> GetPropertyGridChildren()
        {
            var propertyGridControls = new List<PropertyGridControl>();

            
            foreach (TabItem tabItem in this.Items)
            {
                // Search tabs for PropertyGridControl instances
                var controls = WpfVisualFinders.FindAllChildren<PropertyGridControl>(this);

                propertyGridControls.AddRange(controls);
            }

            return propertyGridControls;
        }
    }
}
