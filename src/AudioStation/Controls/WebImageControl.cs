using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using AudioStation.Component;

namespace AudioStation.Controls
{
    public class WebImageControl : Image
    {
        public static readonly DependencyProperty ImageEndpointProperty =
            DependencyProperty.Register("ImageEndpoint", typeof(string), typeof(WebImageControl), new PropertyMetadata(string.Empty, OnImageEndpointChanged));

        public string ImageEndpoint
        {
            get { return (string)GetValue(ImageEndpointProperty); }
            set { SetValue(ImageEndpointProperty, value); }
        }

        protected string LastImageEndpoint { get; private set; }

        public WebImageControl()
        {
            this.Unloaded += WebImageControl_Unloaded;
            this.IsVisibleChanged += WebImageControl_IsVisibleChanged;
        }

        ~WebImageControl()
        {
            // DESTRUCTOR CALED FROM NON-DISPATCHER ?!?
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Unloaded -= WebImageControl_Unloaded;
                this.IsVisibleChanged -= WebImageControl_IsVisibleChanged;

            }, DispatcherPriority.ApplicationIdle);
        }

        private void WebImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Source = null;
        }

        private void WebImageControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible &&
                !string.IsNullOrEmpty(this.ImageEndpoint) && 
                this.Source == null)
            {
                LoadImageAsync();
            }
        }

        private void LoadImageAsync()
        {
            if (this.ImageEndpoint == this.LastImageEndpoint)
                return;

            Application.Current.Dispatcher.BeginInvoke(async () =>
            {
                // This can happen during virtual scrolling
                if (this.ImageEndpoint == this.LastImageEndpoint)
                    return;

                var client = new HttpClient();
                var buffer = await client.GetByteArrayAsync(this.ImageEndpoint);

                this.Source = BitmapConverter.BitmapDataToBitmapSource(buffer);
                this.LastImageEndpoint = this.ImageEndpoint;

                client.Dispose();
                client = null;

                InvalidateVisual();

            }, DispatcherPriority.ApplicationIdle);
        }

        private static void OnImageEndpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WebImageControl;
            var imageFile = e.NewValue as string;

            if (control != null && !string.IsNullOrEmpty(imageFile) && imageFile != control.LastImageEndpoint)
            {
                control.LoadImageAsync();
            }
        }
    }
}
