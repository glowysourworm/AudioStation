using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyLabelControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyLabelControl));

        public static readonly DependencyProperty TextWrapProperty =
            DependencyProperty.Register("TextWrap", typeof(TextWrapping), typeof(PropertyLabelControl));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public TextWrapping TextWrap
        {
            get { return (TextWrapping)GetValue(TextWrapProperty); }
            set { SetValue(TextWrapProperty, value); }
        }

        public PropertyLabelControl()
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
