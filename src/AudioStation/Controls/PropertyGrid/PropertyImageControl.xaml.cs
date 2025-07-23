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
    public partial class PropertyImageControl : PropertyGridControl
    {
        private readonly IBitmapConverter _bitmapConverter;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(ImageViewModel), typeof(PropertyImageControl), new PropertyMetadata(OnValueChanged));

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

        private void SetImage()
        {
            var imageData = _bitmapConverter.BitmapDataToBitmapSource(this.Value.Buffer, new ImageSize(ImageCacheType.Medium), this.Value.MimeType);

            this.ImageControl.Source = imageData.Source;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyImageControl;

            if (control != null && control.Value != null)
            {
                control.SetImage();   
            }
        }

        protected override bool Validate()
        {
            return this.Value != null;
        }
    }
}
