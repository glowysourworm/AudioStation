using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Controls.Animation;

using SimpleWpf.IocFramework.Application;

using TagLib;

namespace AudioStation.Controls
{
    public class WebImageCarouselControl : ContentControl, IDisposable
    {
        public static readonly DependencyProperty ArtworkProperty =
            DependencyProperty.Register("Artwork", typeof(IEnumerable<string>), typeof(WebImageCarouselControl), new PropertyMetadata(OnInputChanged));

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(WebImageCarouselControl), new PropertyMetadata(3000, OnInputChanged));

        public static readonly DependencyProperty ImageSizeProperty =
            DependencyProperty.Register("ImageSize", typeof(ImageCacheType), typeof(WebImageCarouselControl));

        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(ContentTransition), typeof(WebImageCarouselControl));

        public ContentTransition Transition
        {
            get { return (ContentTransition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }
        public IEnumerable<string> Artwork
        {
            get { return (IEnumerable<string>)GetValue(ArtworkProperty); }
            set { SetValue(ArtworkProperty, value); }
        }
        public ImageCacheType ImageSize
        {
            get { return (ImageCacheType)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        protected Timer Timer { get; private set; }
        protected List<Image> CarouselImages { get; private set; }
        protected int CarouselIndex { get; private set; }

        private readonly IImageCacheController _imageCacheController;

        public WebImageCarouselControl()
        {
            _imageCacheController = IocContainer.Get<IImageCacheController>();

            // State carries the current index
            this.Timer = new Timer(TimerTick, 0, 0, this.Interval <= 0 ? Timeout.Infinite : this.Interval);
            this.CarouselImages = new List<Image>();

            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded;
            this.IsVisibleChanged += OnIsVisibleChanged;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReInitialize(0);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReInitialize(0);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ReInitialize(0);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReInitialize(0);
        }

        ~WebImageCarouselControl()
        {
            Application.Current.Dispatcher.BeginInvoke(Dispose, DispatcherPriority.Background);
        }
        private async Task ReInitialize(int carouselIndex)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(ReInitialize, DispatcherPriority.Background, carouselIndex);

            else
            {

                Dispose();

                // Make sure we're rendering
                if (double.IsNaN(this.RenderSize.Width) ||
                    double.IsNaN(this.RenderSize.Height) ||
                    !this.IsVisible ||
                    this.Artwork == null ||
                    !this.Artwork.Any())
                {
                    return;
                }

                this.CarouselImages = new List<Image>();
                this.CarouselIndex = 0;

                foreach (var imageSource in this.Artwork)
                {
                    this.CarouselImages.Add(new Image()
                    {
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.Both,
                        Source = await _imageCacheController.GetFromEndpoint(imageSource, PictureType.Artist, this.ImageSize)
                    });
                }

                this.Content = this.CarouselImages[0];          // Just set the first

                // CRITICAL!  The Timer tick dispatcher invoke does not wait for this 
                //            method to finish! So, this must happen after the artwork
                //            has been waited on!

                if (this.Artwork.Count() > 1)
                {
                    // Time begins immediately! 
                    this.Timer = new Timer(TimerTick, 0, 0, this.Interval <= 0 ? Timeout.Infinite : this.Interval);
                }
            }
        }

        private void TimerTick(object? state)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.Artwork == null || this.CarouselImages == null || this.CarouselImages.Count <= 0)
                    return;

                if (this.CarouselIndex < this.CarouselImages.Count - 1)
                    this.TransitionTo(this.CarouselIndex + 1);
                else
                    this.TransitionTo(0);

            }, DispatcherPriority.Background);
        }
        private void TransitionTo(int index)
        {
            this.CarouselIndex = index;

            if (this.Content == this.CarouselImages[index])
                return;

            if (this.Content == null ||
                this.Transition == null)
                this.Content = this.CarouselImages[index];

            else
                this.Transition.BeginTransition(this, this.Content as FrameworkElement, this.CarouselImages[index]);
        }
        private static void OnInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WebImageCarouselControl;

            if (control != null && control.Artwork != null && control.Artwork.Any())
            {
                control.ReInitialize(0);
            }
        }

        public void Dispose()
        {
            if (this.Timer != null)
            {
                this.Timer.Dispose();
                this.Timer = null;

                // MEMORY LEAK!
                foreach (var image in this.CarouselImages)
                {
                    image.Source = null;
                }
                this.CarouselIndex = 0;
                this.CarouselImages.Clear();
                this.CarouselImages = null;
                this.Artwork = null;
            }
        }
    }
}
