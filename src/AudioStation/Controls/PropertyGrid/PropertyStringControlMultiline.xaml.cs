using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyStringControlMultiline : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyStringControlMultiline));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyStringControlMultiline), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register("MaxLinesForeground", typeof(int), typeof(PropertyStringControlMultiline), new PropertyMetadata(5));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLengthForeground", typeof(int), typeof(PropertyStringControlMultiline), new PropertyMetadata(300));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyStringControlMultiline), new PropertyMetadata(OnTextValueChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyStringControlMultiline));

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
        public int MaxLines
        {
            get { return (int)GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public PropertyStringControlMultiline()
        {
            InitializeComponent();
        }

        private static void OnTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyStringControlMultiline;

            if (control != null)
            {
                //control.GetBindingExpression(ValueProperty).UpdateTarget();
            }
        }
    }
}
