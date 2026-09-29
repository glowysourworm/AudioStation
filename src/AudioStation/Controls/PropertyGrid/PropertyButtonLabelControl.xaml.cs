using System.Windows;

using SimpleWpf.UI.Command;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyButtonLabelControl : PropertyGridControl
    {
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(SimpleCommand), typeof(PropertyButtonLabelControl));

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register("ButtonWidth", typeof(double), typeof(PropertyButtonLabelControl));

        public SimpleCommand ButtonCommand
        {
            get { return (SimpleCommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }
        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
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
