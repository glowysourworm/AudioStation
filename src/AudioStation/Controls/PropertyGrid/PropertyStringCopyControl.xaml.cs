using System.Windows;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyStringCopyControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(PropertyStringCopyControl));

        public static readonly DependencyProperty CanCopyProperty =
            DependencyProperty.Register("CanCopy", typeof(bool), typeof(PropertyStringCopyControl));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public bool CanCopy
        {
            get { return (bool)GetValue(CanCopyProperty); }
            set { SetValue(CanCopyProperty, value); }
        }

        public event SimpleEventHandler<string> CopyEvent;

        public PropertyStringCopyControl()
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CopyEvent != null)
                this.CopyEvent(this.Value);
        }
    }
}
