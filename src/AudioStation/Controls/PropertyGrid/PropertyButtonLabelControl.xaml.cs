using System.Windows;
using System.Windows.Input;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyButtonLabelControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(PropertyButtonLabelControl));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public PropertyButtonLabelControl()
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
