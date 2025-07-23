using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyLabelControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyLabelControl));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyLabelControl()
        {
            InitializeComponent();
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
