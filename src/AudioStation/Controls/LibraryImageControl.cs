using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Utility;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Controls
{
    public class LibraryImageControl : Image
    {
        private readonly IImageCacheController _cacheController;

        /// <summary>
        /// Sets the (static) image size based on an enumeration. (see ImageCacheType / ImageSize for sizes)
        /// </summary>
        public ImageCacheType ImageSize { get; set; }
        protected EntityViewModel ViewModel { get; private set; }

        public LibraryImageControl()
        {
            // IocRegion doesn't have support for templates (easily)
            _cacheController = IocContainer.Get<IImageCacheController>();

            this.Unloaded += LibraryImageControl_Unloaded;
            this.Loaded += LibraryArtistImage_Loaded;
            this.IsVisibleChanged += LibraryImageControl_IsVisibleChanged;
            this.DataContextChanged += LibraryArtistImage_DataContextChanged;
        }

        ~LibraryImageControl()
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.ApplicationClosing)
                return;

            // DESTRUCTOR CALED FROM NON-DISPATCHER ?!?
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Unloaded -= LibraryImageControl_Unloaded;
                this.IsVisibleChanged -= LibraryImageControl_IsVisibleChanged;
                this.DataContextChanged -= LibraryArtistImage_DataContextChanged;

            }, DispatcherPriority.ApplicationIdle);
        }
        private void LibraryArtistImage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel = e.NewValue as EntityViewModel;

            Reload();
        }
        private void LibraryArtistImage_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }
        private void LibraryImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Source = null;
        }
        private void LibraryImageControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Reload();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Reload();
        }

        private async Task Reload()
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(Reload, DispatcherPriority.Background);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                // Go ahead and dump the source data until we've been reloaded by the container
                this.Source = null;

                if (this.ViewModel == null ||
                   !this.IsVisible)
                    return;

                switch (this.ViewModel.Type)
                {
                    case Core.Model.LibraryEntityType.Album:
                    {
                        var imageData = await _cacheController.GetForAlbum(this.ViewModel.Id, this.ImageSize);
                        this.Source = imageData?.Source;
                    }
                        break;
                    case Core.Model.LibraryEntityType.Artist:
                    {
                        var imageData = await _cacheController.GetForArtist(this.ViewModel.Id, this.ImageSize);
                        this.Source = imageData?.Source;
                    }
                        break;
                    case Core.Model.LibraryEntityType.Track:
                    case Core.Model.LibraryEntityType.Genre:
                    default:
                        throw new Exception("Unhandled LibraryEntityType:  LibraryImageControl.cs");
                }

                if (this.Source == null)
                {
                    var imageData = _cacheController.GetDefaultImage(this.ImageSize);
                    this.Source = imageData?.Source;
                }
                    

                InvalidateVisual();
            }
        }
    }
}
