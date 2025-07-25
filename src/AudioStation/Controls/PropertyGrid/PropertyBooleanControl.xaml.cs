using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyBooleanControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(PropertyBooleanControl));

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyBooleanControl()
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
