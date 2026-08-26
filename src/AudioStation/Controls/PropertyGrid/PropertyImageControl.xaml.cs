using System.Windows;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.ImageCacheModel;
using AudioStation.ViewModels.OtherViewModels;

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

        public override bool Validate()
        {
            return this.Value != null;
        }
        public override void CommitChanges()
        {

        }
    }
}
