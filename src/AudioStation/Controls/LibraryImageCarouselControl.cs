using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using AudioStation.Controls.Animation;

namespace AudioStation.Controls
{
    public class LibraryImageCarouselControl : ContentControl
    {
        public static readonly DependencyProperty ArtworkProperty =
            DependencyProperty.Register("Artwork", typeof(IEnumerable<ImageSource>), typeof(LibraryImageCarouselControl), new PropertyMetadata(OnInputChanged));

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(LibraryImageCarouselControl), new PropertyMetadata(3, OnInputChanged));

        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(ContentTransition), typeof(LibraryImageCarouselControl));

        public ContentTransition Transition
        {
            get { return (ContentTransition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public IEnumerable<ImageSource> Artwork
        {
            get { return (IEnumerable<ImageSource>)GetValue(ArtworkProperty); }
            set { SetValue(ArtworkProperty, value); }
        }
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        protected Timer Timer { get; private set; }
        protected List<Image> CarouselImages { get; private set; }

        public LibraryImageCarouselControl()
        {
            // State carries the current index
            this.Timer = new Timer(TimerTick, 0, 0, this.Interval <= 0 ? Timeout.Infinite : this.Interval);
            this.CarouselImages = new List<Image>();
        }
        ~LibraryImageCarouselControl()
        {
            this.Timer.Dispose();
            this.Timer = null;
            this.CarouselImages.Clear();
            this.CarouselImages = null;
            this.Artwork = null;
        }
        private void ReInitialize(int carouselIndex)
        {
            // CRITICAL!  CHECK FOR VISUAL RENDERING - OTHERWISE WE'RE RENDERING OFF SCREEN! There are memory
            //            leaks everywhere for when memory isn't disposed of for each virtualized control!
            //
            if (!this.IsHitTestVisible)
            {
                this.Timer.Dispose();
                this.Timer = null;
                return;
            }
            else if (this.Timer == null)
            {
                this.Timer = new Timer(TimerTick, 0, 0, this.Interval <= 0 ? Timeout.Infinite : this.Interval);
            }

            this.CarouselImages.Clear();

            if (this.Artwork.Count() > 0)
            {
                foreach (var imageSource in this.Artwork)
                {
                    this.CarouselImages.Add(new Image()
                    {
                        Source = imageSource,
                    });
                }

                this.Content = this.CarouselImages[0];          // Just set the first

                // Infinite interval (turn off cycling)
                if (this.Interval <= 0 || this.Artwork.Count() == 1)
                {
                    this.Timer.Change(0, Timeout.Infinite);
                }
                else if (this.Artwork.Count() > 1)
                {
                    this.Timer.Change(0, this.Interval);
                }
            }
            else
            {
                this.Timer.Change(0, Timeout.Infinite);
            }
        }
        private void TimerTick(object? state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var index = (int)state;

                if (this.Artwork.Count() > 1)
                {
                    if (index == this.CarouselImages.Count - 1)
                    {
                        this.TransitionTo(0);
                        this.ReInitialize(index + 1);
                    }

                    else
                    {
                        this.TransitionTo(index + 1);
                        this.ReInitialize(index + 1);
                    }
                }
            });
        }
        private void TransitionTo(int index)
        {
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
            var control = d as LibraryImageCarouselControl;

            if (control != null && control.Artwork != null && control.Artwork.Any())
            {
                control.ReInitialize(0);
            }
        }
    }
}
