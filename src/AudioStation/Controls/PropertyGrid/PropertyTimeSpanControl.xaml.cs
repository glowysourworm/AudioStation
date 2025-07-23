using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyTimeSpanControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan), typeof(PropertyTimeSpanControl));

        public TimeSpan Value
        {
            get { return (TimeSpan)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyTimeSpanControl()
        {
            InitializeComponent();
        }

        protected override bool Validate()
        {
            return this.Value.TotalMilliseconds > 0;
        }
    }
}
