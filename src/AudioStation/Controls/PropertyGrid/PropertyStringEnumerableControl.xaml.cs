using System.Windows;
using System.Windows.Controls;

using EMA.ExtendedWPFVisualTreeHelper;

using Xceed.Wpf.Toolkit;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyStringEnumerableControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(IList<string>), typeof(PropertyStringEnumerableControl), new PropertyMetadata(OnValueChanged));

        public IList<string> Value
        {
            get { return (IList<string>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyStringEnumerableControl()
        {
            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PropertyStringEnumerableControl)d;

            if (control != null &&
                e.NewValue != null &&
                e.NewValue is IList<string>)
            {

            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Value?.Add(string.Empty);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = WpfVisualFinders.FindParent<Grid>(sender as Button);
            var textBox = WpfVisualFinders.FindChild<WatermarkTextBox>(grid);

            if (this.Value != null &&
                textBox != null)
            {
                this.Value.Remove(textBox.Text);
            }
        }

        private void ValueTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Nothing to bind to. So, every time the text is changed the container
            // generator has to be called to update the binding source.
            //
            for (int index = 0; index < this.Value.Count; index++)
            {
                var itemContainer = this.ItemsLB.ItemContainerGenerator.ContainerFromIndex(index);
                var textBox = WpfVisualFinders.FindChild<WatermarkTextBox>(itemContainer);

                if (textBox != null)
                {
                    this.Value[index] = textBox.Text;
                }
            }
        }

        protected override bool Validate()
        {
            return this.Value != null &&
                   this.Value.Count > 0 &&
                   this.Value.All(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
