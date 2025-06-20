using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AudioStation.Controls
{
    public class ScrubberRectangle : Shape
    {
        public static readonly DependencyProperty ScrubbedRatioProperty =
            DependencyProperty.Register("ScrubbedRatio", typeof(float), typeof(ScrubberRectangle), new PropertyMetadata(OnRatioChanged));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ScrubberRectangle));

        public static readonly DependencyProperty ScrubbedBrushProperty =
            DependencyProperty.Register("ScrubbedBrush", typeof(Brush), typeof(ScrubberRectangle));

        public static readonly DependencyProperty NonScrubbedBrushProperty =
            DependencyProperty.Register("NonScrubbedBrush", typeof(Brush), typeof(ScrubberRectangle));

        public float ScrubbedRatio
        {
            get { return (float)GetValue(ScrubbedRatioProperty); }
            set { SetValue(ScrubbedRatioProperty, value); }
        }
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public Brush ScrubbedBrush
        {
            get { return (Brush)GetValue(ScrubbedBrushProperty); }
            set { SetValue(ScrubbedBrushProperty, value); }
        }
        public Brush NonScrubbedBrush
        {
            get { return (Brush)GetValue(NonScrubbedBrushProperty); }
            set { SetValue(NonScrubbedBrushProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get { return _geometry; }
        }

        RectangleGeometry _geometry;

        public ScrubberRectangle()
        {
            _geometry = new RectangleGeometry(Rect.Empty);
        }

        private void SetGeometry()
        {
            _geometry = new RectangleGeometry(new Rect(this.RenderSize));

            InvalidateVisual();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            SetGeometry();
        }

        private static void OnRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ScrubberRectangle;
            control?.SetGeometry();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            Rect scrubbedRect, nonScrubbedRect;

            if (this.Orientation == Orientation.Horizontal)
            {
                scrubbedRect = new Rect(0, 0, this.RenderSize.Width * this.ScrubbedRatio, this.RenderSize.Height);
                nonScrubbedRect = new Rect(scrubbedRect.Width, 0, this.RenderSize.Width * (1 - this.ScrubbedRatio), this.RenderSize.Height);
            }
            else
            {
                nonScrubbedRect = new Rect(0, 0, this.RenderSize.Width, this.RenderSize.Height * this.ScrubbedRatio);
                scrubbedRect = new Rect(0, nonScrubbedRect.Height, this.RenderSize.Width, this.RenderSize.Height * (1 - this.ScrubbedRatio));                
            }

            drawingContext.DrawRectangle(this.ScrubbedBrush, null, scrubbedRect);
            drawingContext.DrawRectangle(this.NonScrubbedBrush, null, nonScrubbedRect);
        }
    }
}
