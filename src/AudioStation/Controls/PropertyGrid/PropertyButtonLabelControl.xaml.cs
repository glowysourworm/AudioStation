using System.Windows;
using System.Windows.Input;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyButtonLabelControl : PropertyGridControl
    {
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonCommandParameterProperty =
            DependencyProperty.Register("ButtonCommandParameter", typeof(object), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register("ButtonWidth", typeof(double), typeof(PropertyButtonLabelControl));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }
        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        public object ButtonCommandParameter
        {
            get { return (object)GetValue(ButtonCommandParameterProperty); }
            set { SetValue(ButtonCommandParameterProperty, value); }
        }
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
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
