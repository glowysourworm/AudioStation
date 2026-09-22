using System.Windows;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyIntCopyControl : PropertyGridControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(PropertyIntCopyControl));

        public static readonly DependencyProperty ValueMinProperty =
            DependencyProperty.Register("ValueMin", typeof(uint), typeof(PropertyIntCopyControl));

        public static readonly DependencyProperty ValueMaxProperty =
            DependencyProperty.Register("ValueMax", typeof(uint), typeof(PropertyIntCopyControl));

        public static readonly DependencyProperty CanCopyProperty =
            DependencyProperty.Register("CanCopy", typeof(bool), typeof(PropertyIntCopyControl));

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
        public bool CanCopy
        {
            get { return (bool)GetValue(CanCopyProperty); }
            set { SetValue(CanCopyProperty, value); }
        }

        public event SimpleEventHandler<int> CopyEvent;

        public PropertyIntCopyControl()
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.CopyEvent != null)
                this.CopyEvent(this.Value);
        }
    }
}
