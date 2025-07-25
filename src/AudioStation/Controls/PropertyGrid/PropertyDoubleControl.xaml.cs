using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyDoubleControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(PropertyDoubleControl));

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
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyDoubleControl()
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
    }
}
