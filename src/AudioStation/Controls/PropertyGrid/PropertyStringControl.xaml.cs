using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyStringControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyStringControl));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyStringControl()
        {
            InitializeComponent();

            OnPropertyChanged(new DependencyPropertyChangedEventArgs(PropertyBooleanControl.ValueProperty, null, null));
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(this.Value);
        }
        public override void CommitChanges()
        {

        }
    }
}
