using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls
{
    public partial class PropertyStringControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyStringControl));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyStringControl), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register("LabelForeground", typeof(Brush), typeof(PropertyStringControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyStringControl));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyStringControl));

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
        public Brush LabelForeground
        {
            get { return (Brush)GetValue(LabelForegroundProperty); }
            set { SetValue(LabelForegroundProperty, value); }
        }
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public PropertyStringControl()
        {
            InitializeComponent();
        }
    }
}
