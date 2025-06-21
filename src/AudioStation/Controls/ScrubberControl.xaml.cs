using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controls
{
    public partial class ScrubberControl : UserControl
    {
        // NOTE:  This may only be used as a one-way binding. The event is used to complete the update circuit.
        public static readonly DependencyProperty ScrubbedRatioProperty = 
            DependencyProperty.Register("ScrubbedRatio", typeof(float), typeof(ScrubberControl), new PropertyMetadata(OnScrubberChanged));

        public static readonly DependencyProperty ScrubberHandleBrushProperty =
            DependencyProperty.Register("ScrubberHandleBrush", typeof(Brush), typeof(ScrubberControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ScrubberTimelineBrushProperty =
            DependencyProperty.Register("ScrubberTimelineBrush", typeof(Brush), typeof(ScrubberControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ScrubberTimelineBrushScrubbedProperty =
            DependencyProperty.Register("ScrubberTimelineBrushScrubbed", typeof(Brush), typeof(ScrubberControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ScrubberTimelineSizeProperty =
            DependencyProperty.Register("ScrubberTimelineSize", typeof(int), typeof(ScrubberControl));

        public static readonly DependencyProperty ScrubberHandleSizeProperty =
            DependencyProperty.Register("ScrubberHandleSize", typeof(int), typeof(ScrubberControl));

        public static readonly DependencyProperty ScrubberPreviewVisibleProperty =
            DependencyProperty.Register("ScrubberPreviewVisible", typeof(bool), typeof(ScrubberControl));


        public Brush ScrubberHandleBrush
        {
            get { return (Brush)GetValue(ScrubberHandleBrushProperty); }
            set { SetValue(ScrubberHandleBrushProperty, value); }
        }
        public Brush ScrubberTimelineBrush
        {
            get { return (Brush)GetValue(ScrubberTimelineBrushProperty); }
            set { SetValue(ScrubberTimelineBrushProperty, value); }
        }
        public Brush ScrubberTimelineBrushScrubbed
        {
            get { return (Brush)GetValue(ScrubberTimelineBrushScrubbedProperty); }
            set { SetValue(ScrubberTimelineBrushScrubbedProperty, value); }
        }
        public int ScrubberTimelineSize
        {
            get { return (int)GetValue(ScrubberTimelineSizeProperty); }
            set { SetValue(ScrubberTimelineSizeProperty, value); }
        }
        public int ScrubberHandleSize
        {
            get { return (int)GetValue(ScrubberHandleSizeProperty); }
            set { SetValue(ScrubberHandleSizeProperty, value); }
        }
        public bool ScrubberPreviewVisible
        {
            get { return (bool)GetValue(ScrubberPreviewVisibleProperty); }
            set { SetValue(ScrubberPreviewVisibleProperty, value); }
        }
        public float ScrubbedRatio
        {
            get { return (float)GetValue(ScrubbedRatioProperty); }
            set { SetValue(ScrubbedRatioProperty, value); }
        }

        public event SimpleEventHandler<float> ScrubbedRatioChanged;

        public ScrubberControl()
        {
            InitializeComponent();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var scrubbedRatio = 0.0f;
            var offset = CalculateScrubberOffset(e.GetPosition(this).X, out scrubbedRatio);

            // Mouse position must be offset to center of scrubber (in X)
            this.ScrubberPreviewCursor.Margin = new Thickness(offset, 0, 0, 0);

            // -> SetScrubberOffset()
            if (this.IsMouseCaptured && this.ScrubbedRatioChanged != null && scrubbedRatio != this.ScrubbedRatio)
                this.ScrubbedRatioChanged(scrubbedRatio);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            this.ScrubberPreviewVisible = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            this.ScrubberPreviewVisible = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.CaptureMouse();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            this.ReleaseMouseCapture();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            SetScrubberOffset(true);
        }
        private double CalculateScrubberOffset(double currentPosition, out float scrubbedRatio)
        {
            // This is the usable UI with respect to the center of the scrubber handle
            var widthHandle = this.ScrubberHandleSize + 2;                              // Stroke Thickness

            var offset = currentPosition - (widthHandle / 2.0f);
            var offsetMin = 0;
            var offsetMax = this.RenderSize.Width - widthHandle;

            scrubbedRatio = (float)Math.Clamp(currentPosition / this.RenderSize.Width, 0, 1);

            return Math.Clamp(offset, offsetMin, offsetMax);
        }
        private void SetScrubberOffset(bool fromSizeChange)
        {
            // Already Changed ScrubbedRatio
            var scrubbedRatio = 0.0f;
            var offset = CalculateScrubberOffset(this.ScrubbedRatio * this.RenderSize.Width, out scrubbedRatio);

            this.ScrubberCursor.Margin = new Thickness(offset, 0, 0, 0);

            // Raise Event (not all listeners will bind to the ratio)
            if (this.ScrubbedRatioChanged != null && !fromSizeChange && scrubbedRatio != this.ScrubbedRatio)
                this.ScrubbedRatioChanged(scrubbedRatio);

            InvalidateVisual();
        }
        private static void OnScrubberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // NOTE*** WE CAN ONLY SET THIS AS A ONE-WAY BINDING! Otherwise, the framework removes updates (haven't found out why yet)
            //         So, we're using the event to fire back changes.
            var control = d as ScrubberControl;

            if (control != null &&
                control.IsVisible)
            {
                control.SetScrubberOffset(false);
            }
            
        }

        private void ScrubberLine_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ScrubberPreviewVisible = true;

            InvalidateVisual();
        }

        private void ScrubberLine_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ScrubberPreviewVisible = false;

            InvalidateVisual();
        }
    }
}
