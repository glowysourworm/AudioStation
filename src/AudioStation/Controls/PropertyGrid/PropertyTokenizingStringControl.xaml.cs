using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyTokenizingStringControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(IEnumerable<string>), typeof(PropertyTokenizingStringControl));

        public IEnumerable<string> Value
        {
            get { return (IEnumerable<string>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyTokenizingStringControl()
        {
            InitializeComponent();
        }

        public override bool Validate()
        {
            return this.Value != null &&
                   this.Value.Any() &&
                   this.Value.All(x => !string.IsNullOrWhiteSpace(x));
        }
        public override void CommitChanges()
        {

        }
    }
}
