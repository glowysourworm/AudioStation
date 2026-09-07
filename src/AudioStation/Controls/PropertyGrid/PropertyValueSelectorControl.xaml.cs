using System.Collections;
using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyValueSelectorControl : PropertyGridControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PropertyValueSelectorControl));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(PropertyValueSelectorControl));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyValueSelectorControl));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyValueSelectorControl()
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
