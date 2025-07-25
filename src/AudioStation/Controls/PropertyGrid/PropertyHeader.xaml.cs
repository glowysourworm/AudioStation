using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyHeader : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyHeader));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyHeader()
        {
            InitializeComponent();
        }

        public override bool Validate()
        {
            return true;
        }
        public override void CommitChanges()
        {

        }
    }
}
