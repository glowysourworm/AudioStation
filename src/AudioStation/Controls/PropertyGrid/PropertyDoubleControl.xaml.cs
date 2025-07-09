using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyDoubleControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyDoubleControl));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyDoubleControl), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(PropertyDoubleControl));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyDoubleControl));

        public static readonly DependencyProperty ValueMinProperty =
            DependencyProperty.Register("ValueMin", typeof(double), typeof(PropertyDoubleControl));

        public static readonly DependencyProperty ValueMaxProperty =
            DependencyProperty.Register("ValueMax", typeof(double), typeof(PropertyDoubleControl));

        public static readonly DependencyProperty ValueIncrementProperty =
            DependencyProperty.Register("ValueIncrement", typeof(double), typeof(PropertyDoubleControl));

        public double ValueMin
        {
            get { return (double)GetValue(ValueMinProperty); }
            set { SetValue(ValueMinProperty, value); }
        }
        public double ValueMax
        {
            get { return (double)GetValue(ValueMaxProperty); }
            set { SetValue(ValueMaxProperty, value); }
        }
        public double ValueIncrement
        {
            get { return (double)GetValue(ValueIncrementProperty); }
            set { SetValue(ValueIncrementProperty, value); }
        }

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
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyDoubleControl()
        {
            InitializeComponent();
        }
    }
}
