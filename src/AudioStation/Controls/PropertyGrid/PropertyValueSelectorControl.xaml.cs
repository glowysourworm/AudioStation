using System.Collections;
using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyValueSelectorControl : PropertyGridControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PropertyValueSelectorControl), new PropertyMetadata(OnChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(PropertyValueSelectorControl));

        public static readonly DependencyProperty IsValueTypeProperty =
            DependencyProperty.Register("IsValueType", typeof(bool), typeof(PropertyValueSelectorControl));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyValueSelectorControl), new PropertyMetadata(OnChanged));

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
        public bool IsValueType
        {
            get { return (bool)GetValue(IsValueTypeProperty); }
            set { SetValue(IsValueTypeProperty, value); }
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
        private void Update()
        {
            // Not yet initialized
            if (this.ItemsSource == null)
                return;

            foreach (var item in this.ItemsSource)
            {
                // TODO: Convention on "bad data"
                if (item == null)
                    continue;

                // Value-Type Comparison
                //
                if (this.IsValueType && item.Equals(this.Value))
                    this.Value = item;

                // Reference Type (set "to be sure")
                //
                else if (!this.IsValueType && item == this.Value)
                    this.Value = item;
            }
        }
        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyValueSelectorControl;

            if (control != null)
            {
                control.Update();
            }
        }
    }
}
