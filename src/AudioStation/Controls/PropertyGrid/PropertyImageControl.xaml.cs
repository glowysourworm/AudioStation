using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AudioStation.Component;
using AudioStation.Component.Interface;
using AudioStation.Controller.Model;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyImageControl : UserControl
    {
        private readonly IBitmapConverter _bitmapConverter;

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(PropertyImageControl));

        public static readonly DependencyProperty LabelColumnWidthProperty =
            DependencyProperty.Register("LabelColumnWidth", typeof(double), typeof(PropertyImageControl), new PropertyMetadata(150.0D));

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register("LabelForeground", typeof(Brush), typeof(PropertyImageControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(ImageViewModel), typeof(PropertyImageControl), new PropertyMetadata(OnValueChanged));

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
        public ImageViewModel Value
        {
            get { return (ImageViewModel)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PropertyImageControl()
        {
            _bitmapConverter = IocContainer.Get<IBitmapConverter>();

            InitializeComponent();
        }
        ~PropertyImageControl()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ImageControl.Source = null;
            }));
        }

        private void SetImage()
        {
            this.ImageControl.Source = _bitmapConverter.BitmapDataToBitmapSource(this.Value.Buffer, new ImageSize(ImageCacheType.Medium), this.Value.MimeType);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyImageControl;

            if (control != null && control.Value != null)
            {
                control.SetImage();   
            }
        }
    }
}
