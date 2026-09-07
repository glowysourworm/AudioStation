using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyEnumControl : PropertyGridControl
    {
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(PropertyEnumControl));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyEnumControl));

        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyEnumControl()
        {
            InitializeComponent();
        }

        public override bool Validate()
        {
            return this.Value != null;
        }
        public override void CommitChanges()
        {

        }
    }
}
