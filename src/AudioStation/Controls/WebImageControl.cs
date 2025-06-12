using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;

using SimpleWpf.IocFramework.Application;

using TagLib;

namespace AudioStation.Controls
{
    public class WebImageControl : Image
    {
        public static readonly DependencyProperty ImageEndpointProperty =
            DependencyProperty.Register("ImageEndpoint", typeof(string), typeof(WebImageControl), new PropertyMetadata(string.Empty));

        public string ImageEndpoint
        {
            get { return (string)GetValue(ImageEndpointProperty); }
            set { SetValue(ImageEndpointProperty, value); }
        }

        /// <summary>
        /// Sets the (static) image size based on an enumeration. (see ImageCacheType / ImageSize for sizes)
        /// </summary>
        public ImageCacheType ImageSize { get; private set; }

        private readonly PictureType _cacheType;
        private readonly IImageCacheController _cacheController;

        public WebImageControl()
        {
            _cacheController = IocContainer.Get<IImageCacheController>();

            // This may help to detail web images for some services that deal with mp3 tags
            _cacheType = PictureType.FrontCover;

            this.Unloaded += WebImageControl_Unloaded;
            this.Loaded += WebImageControl_Loaded;
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

        private void WebImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void WebImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Source = null;
        }

        private void WebImageControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Reload();
        }

        private async void Reload()
        {
            // Don't await the BeginInvoke
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(Reload, DispatcherPriority.Background);

            else
            {
                // Go ahead and dump the source data until we've been reloaded by the container
                this.Source = null;

                if (double.IsNaN(this.Width) ||
                    double.IsNaN(this.Height))
                    return;

                if (string.IsNullOrEmpty(this.ImageEndpoint) ||
                   !this.IsVisible)
                    return;

                switch (_cacheType)
                {
                    case PictureType.FrontCover:
                        this.Source = await _cacheController.GetFromEndpoint(this.ImageEndpoint, _cacheType, this.ImageSize);
                        break;
                    default:
                        throw new Exception("Unhandled Picture Type:  WebImageControl.cs");
                }
            }
        }
    }
}
