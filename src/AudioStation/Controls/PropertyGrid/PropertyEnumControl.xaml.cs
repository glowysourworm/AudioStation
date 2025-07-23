using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyEnumControl : PropertyGridControl
    {
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(PropertyEnumControl));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Enum), typeof(PropertyEnumControl));

        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }
        public Enum Value
        {
            get { return (Enum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyEnumControl()
        {
            InitializeComponent();
        }

        protected override bool Validate()
        {
            return this.Value != null;
        }
    }
}
