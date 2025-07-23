using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyDoubleSliderControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(PropertyDoubleSliderControl));
        
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyDoubleSliderControl()
        {
            InitializeComponent();
        }

        protected override bool Validate()
        {
            return this.Value >= 0 && this.Value <= 1;
        }
    }
}
