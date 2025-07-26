using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AudioStation.Controls.PropertyGrid
{
    public abstract class PropertyGridControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyGridControl));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyGridControl), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register("LabelForeground", typeof(Brush), typeof(PropertyLabelControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyGridControl));

        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(PropertyGridControl));

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(PropertyGridControl));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        public double LabelColumnWidth
        {
            get { return (double)GetValue(LabelColumnWidthProperty); }
            set { SetValue(LabelColumnWidthProperty, value); }
        }
        public Brush LabelForeground
        {
            get { return (Brush)GetValue(LabelForegroundProperty); }
            set { SetValue(LabelForegroundProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }
        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        /// <summary>
        /// Causes the control to validate against the data source
        /// </summary>
        public abstract bool Validate();

        /// <summary>
        /// Causes the control to commit changes, if any are needed (against the binding), 
        /// to complete the data cycle.
        /// </summary>
        public abstract void CommitChanges();

        public PropertyGridControl()
        {
            this.Loaded += PropertyGridControl_Loaded;
            this.Unloaded += PropertyGridControl_Unloaded;
        }

        private void PropertyGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Default Style (have to set this here due to style override issues)
            //
            if (this.Style == null)
                this.Style = this.FindResource("PropertyGridUserControl") as Style;
        }

        private void PropertyGridControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Force Commit: The data cycle has to be completed. There are problems with lazy loading tabs
            //               and control bindings for collections.
            CommitChanges();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "Value")
            {
                this.IsValid = !this.IsRequired || Validate();
            }            
        }
    }
}
