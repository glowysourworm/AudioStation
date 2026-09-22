using System.Windows;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyPasswordControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyPasswordControl), new PropertyMetadata(OnChanged));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyPasswordControl()
        {
            InitializeComponent();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(this.Value);
        }
        public override void CommitChanges()
        {

        }

        private void PasswordTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.Value = this.PasswordTB.Password;
        }
        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyPasswordControl;

            if (control != null)
                control.PasswordTB.Password = control.Value;
        }
    }
}
