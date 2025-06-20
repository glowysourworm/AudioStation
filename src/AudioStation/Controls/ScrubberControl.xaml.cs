using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AudioStation.Controls
{
    public partial class ScrubberControl : UserControl
    {
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

        public ScrubberControl()
        {
            InitializeComponent();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Mouse position must be offset to center of scrubber (in X)
            this.ScrubberPreviewCursor.Margin = new Thickness(CalculateScrubberOffset(e.GetPosition(this).X - (this.ScrubberHandleSize / 2.0)), 0, 0, 0);
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            SetScrubberOffset();
        }
        private double CalculateScrubberOffset(double currentPosition)
        {
            var offset = (currentPosition / this.RenderSize.Width) * this.RenderSize.Width;
            var offsetMin = 0;
            var offsetMax = this.RenderSize.Width - this.ScrubberHandleSize - 2;        // Stroke Thickness

            if (offset > 0)
                return Math.Clamp(offset, offsetMin, offsetMax);

            return offsetMin;
        }
        private void SetScrubberOffset()
        {
            this.ScrubberCursor.Margin = new Thickness(CalculateScrubberOffset(this.ScrubbedRatio * this.RenderSize.Width), 0, 0, 0);

            InvalidateVisual();
        }
        private static void OnScrubberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ScrubberControl;
            control?.SetScrubberOffset();
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
