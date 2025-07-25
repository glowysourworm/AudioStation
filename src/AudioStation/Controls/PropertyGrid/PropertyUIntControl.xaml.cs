using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyUIntControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(uint), typeof(PropertyUIntControl));

        public static readonly DependencyProperty ValueMinProperty =
            DependencyProperty.Register("ValueMin", typeof(uint), typeof(PropertyUIntControl));

        public static readonly DependencyProperty ValueMaxProperty =
            DependencyProperty.Register("ValueMax", typeof(uint), typeof(PropertyUIntControl));

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
        public uint Value
        {
            get { return (uint)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyUIntControl()
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
