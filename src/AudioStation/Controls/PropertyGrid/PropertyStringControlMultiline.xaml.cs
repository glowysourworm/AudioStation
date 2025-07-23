using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyStringControlMultiline : PropertyGridControl
    {
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register("MaxLinesForeground", typeof(int), typeof(PropertyStringControlMultiline), new PropertyMetadata(5));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLengthForeground", typeof(int), typeof(PropertyStringControlMultiline), new PropertyMetadata(300));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyStringControlMultiline), new PropertyMetadata(OnTextValueChanged));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
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
        protected override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(this.Value) && this.Value.Length <= this.MaxLength;
        }
    }
}
