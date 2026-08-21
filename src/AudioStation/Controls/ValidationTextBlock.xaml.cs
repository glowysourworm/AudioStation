using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls
{
    public partial class ValidationTextBlock : UserControl
    {
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ValidationTextBlock), new PropertyMetadata(OnChanged));

        public static readonly DependencyProperty IsValidForegroundProperty =
            DependencyProperty.Register("IsValidForeground", typeof(Brush), typeof(ValidationTextBlock));

        public static readonly DependencyProperty IsInvalidForegroundProperty =
            DependencyProperty.Register("IsInvalidForeground", typeof(Brush), typeof(ValidationTextBlock));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ValidationTextBlock));

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }
        public Brush IsValidForeground
        {
            get { return (Brush)GetValue(IsValidForegroundProperty); }
            set { SetValue(IsValidForegroundProperty, value); }
        }
        public Brush IsInvalidForeground
        {
            get { return (Brush)GetValue(IsInvalidForegroundProperty); }
            set { SetValue(IsInvalidForegroundProperty, value); }
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public ValidationTextBlock()
        {
            this.Text = string.Empty;
            this.IsValidForeground = Brushes.Black;
            this.IsInvalidForeground = Brushes.Red;
            this.IsValid = true;

            InitializeComponent();
        }

        protected void Update()
        {
            this.Foreground = this.IsValid ? this.IsValidForeground : this.IsInvalidForeground;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            this.Update();
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ValidationTextBlock;

            if (control != null)
            {
                control.Update();
            }
        }
    }
}
