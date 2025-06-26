using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AudioStation.Controls
{
    public class ProgressBar : Shape
    {
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(ProgressBar), new PropertyMetadata(OnProgressChanged));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(ProgressBar), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(ProgressBar));

        public static readonly DependencyProperty ProgressBrushProperty =
            DependencyProperty.Register("ProgressBrush", typeof(Brush), typeof(ProgressBar), new PropertyMetadata(Brushes.LawnGreen));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public Brush ProgressBrush
        {
            get { return (Brush)GetValue(ProgressBrushProperty); }
            set { SetValue(ProgressBrushProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get { return _geometry; }
        }

        RectangleGeometry _geometry;
        Typeface _typeface;

        public ProgressBar()
        {
            _geometry = new RectangleGeometry(Rect.Empty);
            _typeface = new Typeface("Consolas");
        }

        private void SetGeometry(Size size)
        {
            _geometry = new RectangleGeometry(new Rect(size));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            SetGeometry(constraint);

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ProgressBar;
            if (control != null)
            {
                control.SetGeometry(control.RenderSize);
                control.InvalidateMeasure();
                control.InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            if (double.IsNaN(this.RenderSize.Width) ||
                double.IsNaN(this.RenderSize.Height) ||
                this.RenderSize.Width == 0 ||
                this.RenderSize.Height == 0)
                return;

            var progressRect = new Rect(0, 0, this.RenderSize.Width * this.Progress, this.RenderSize.Height);
            var renderRect = new Rect(this.RenderSize);
            var text = this.Progress.ToString("N2");
            var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, 1.25, this.Foreground);

            var offsetX = (this.RenderSize.Width - formattedText.Width) / 2.0f;
            var offsetY = (this.RenderSize.Height - formattedText.Height) / 2.0f;

            drawingContext.DrawRectangle(this.Background, null, renderRect);
            drawingContext.DrawRectangle(this.ProgressBrush, null, progressRect);
            drawingContext.DrawText(formattedText, new Point(offsetX, offsetY));
        }
    }
}
