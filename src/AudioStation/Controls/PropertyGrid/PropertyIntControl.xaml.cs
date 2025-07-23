using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyIntControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(PropertyIntControl));

        public static readonly DependencyProperty ValueMinProperty =
            DependencyProperty.Register("ValueMin", typeof(uint), typeof(PropertyIntControl));

        public static readonly DependencyProperty ValueMaxProperty =
            DependencyProperty.Register("ValueMax", typeof(uint), typeof(PropertyIntControl));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public uint ValueMin
        {
            get { return (uint)GetValue(ValueMinProperty); }
            set { SetValue(ValueMinProperty, value); }
        }
        public uint ValueMax
        {
            get { return (uint)GetValue(ValueMaxProperty); }
            set { SetValue(ValueMaxProperty, value); }
        }

        public PropertyIntControl()
        {
            InitializeComponent();
        }

        protected override bool Validate()
        {
            return this.Value >= this.ValueMin && this.Value <= this.ValueMax;
        }
    }
}
