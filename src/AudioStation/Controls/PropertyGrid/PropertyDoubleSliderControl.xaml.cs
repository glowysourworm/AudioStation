using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyDoubleSliderControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(PropertyDoubleSliderControl), new PropertyMetadata(OnChanged));

        public static readonly DependencyProperty ValueMinProperty =
            DependencyProperty.Register("ValueMin", typeof(double), typeof(PropertyDoubleSliderControl));

        public static readonly DependencyProperty ValueMaxProperty =
            DependencyProperty.Register("ValueMax", typeof(double), typeof(PropertyDoubleSliderControl));

        public static readonly DependencyProperty ValueStringFormatProperty =
            DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(PropertyDoubleSliderControl), new PropertyMetadata(OnChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
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
        public string ValueStringFormat
        {
            get { return (string)GetValue(ValueStringFormatProperty); }
            set { SetValue(ValueStringFormatProperty, value); }
        }
        public PropertyDoubleSliderControl()
        {
            InitializeComponent();
        }

        public override bool Validate()
        {
            return this.Value >= this.ValueMin && this.Value <= this.ValueMax;
        }
        public override void CommitChanges()
        {

        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PropertyDoubleSliderControl)d;

            if (control != null)
            {
                if (!string.IsNullOrWhiteSpace(control.ValueStringFormat))
                    control.ValueLabelTB.Text = control.Value.ToString(control.ValueStringFormat);

                else
                    control.ValueLabelTB.Text = control.Value.ToString();
            }

        }
    }
}
