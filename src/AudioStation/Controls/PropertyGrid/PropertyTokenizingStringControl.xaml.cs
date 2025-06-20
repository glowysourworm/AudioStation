using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyTokenizingStringControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyTokenizingStringControl));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyTokenizingStringControl), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(IEnumerable<string>), typeof(PropertyTokenizingStringControl));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        public double LabelColumnWidth
        {
            get { return (double)GetValue(LabelColumnWidthProperty); }
            set { SetValue(LabelColumnWidthProperty, value); }
        }
        public IEnumerable<string> Value
        {
            get { return (IEnumerable<string>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyTokenizingStringControl()
        {
            InitializeComponent();
        }
    }
}
