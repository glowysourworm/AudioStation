﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Utility;

using SimpleWpf.IocFramework.Application;

using PictureType = ATL.PictureInfo.PIC_TYPE;

namespace AudioStation.Controls
{
    public class WebImageControl : Image
    {
        public static readonly DependencyProperty ImageEndpointProperty =
            DependencyProperty.Register("ImageEndpoint", typeof(string), typeof(WebImageControl), new PropertyMetadata(string.Empty, OnEndpointChanged));

        public string ImageEndpoint
        {
            get { return (string)GetValue(ImageEndpointProperty); }
            set { SetValue(ImageEndpointProperty, value); }
        }

        /// <summary>
        /// Sets the (static) image size based on an enumeration. (see ImageCacheType / ImageSize for sizes)
        /// </summary>
        public ImageCacheType ImageSize { get; set; }

        private readonly PictureType _cacheType;
        private readonly IImageCacheController _cacheController;

        public WebImageControl()
        {
            _cacheController = IocContainer.Get<IImageCacheController>();

            // This may help to detail web images for some services that deal with mp3 tags
            _cacheType = PictureType.Front;

            this.Unloaded += WebImageControl_Unloaded;
            this.Loaded += WebImageControl_Loaded;
            this.IsVisibleChanged += WebImageControl_IsVisibleChanged;
        }
        ~WebImageControl()
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.ApplicationClosing)
                return;

            // DESTRUCTOR CALED FROM NON-DISPATCHER ?!?
            ApplicationHelpers.BeginInvokeDispatcher(() =>
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

        private static void OnEndpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (WebImageControl)d;
            control?.Reload();
        }

        private async Task Reload()
        {
            // Don't await the BeginInvoke
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(Reload, DispatcherPriority.Background);

            else
            {
                // Go ahead and dump the source data until we've been reloaded by the container
                this.Source = null;

                if (double.IsNaN(this.RenderSize.Width) ||
                    double.IsNaN(this.RenderSize.Height))
                    return;

                if (string.IsNullOrEmpty(this.ImageEndpoint) /*|| !this.IsVisible*/)
                    return;

                switch (_cacheType)
                {
                    case PictureType.Front:
                        this.Source = (await _cacheController.GetFromEndpoint(this.ImageEndpoint, _cacheType, this.ImageSize))?.Source;
                        break;
                    default:
                        throw new Exception("Unhandled Picture Type:  WebImageControl.cs");
                }

                if (this.Source == null)
                    this.Source = _cacheController.GetDefaultImage(this.ImageSize).Source;
            }
        }
    }
}
