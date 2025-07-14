using System.Windows;
using System.Windows.Media;

using AudioStation.Component.AudioProcessing;

namespace AudioStation.Controls
{
    // This control must be statically sized
    public class EqualizerOutputControl : FrameworkElement
    {
        public static readonly DependencyProperty BarPaddingProperty =
            DependencyProperty.Register("BarPadding", typeof(Thickness), typeof(EqualizerOutputControl), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register("BarBrush", typeof(Brush), typeof(EqualizerOutputControl), new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty BarBorderProperty =
            DependencyProperty.Register("BarBorder", typeof(Brush), typeof(EqualizerOutputControl), new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty PeakBrushProperty =
            DependencyProperty.Register("PeakBrush", typeof(Brush), typeof(EqualizerOutputControl), new PropertyMetadata(Brushes.Red));

        public Brush PeakBrush
        {
            get { return (Brush)GetValue(PeakBrushProperty); }
            set { SetValue(PeakBrushProperty, value); }
        }

        public Brush BarBrush
        {
            get { return (Brush)GetValue(BarBrushProperty); }
            set { SetValue(BarBrushProperty, value); }
        }
        public Brush BarBorder
        {
            get { return (Brush)GetValue(BarBorderProperty); }
            set { SetValue(BarBorderProperty, value); }
        }

        public Thickness BarPadding
        {
            get { return (Thickness)GetValue(BarPaddingProperty); }
            set { SetValue(BarPaddingProperty, value); }
        }

        List<Size> _barSizes;
        List<Size> _peakSizes;
        StreamGeometry _outputGeometry;
        StreamGeometry _peakGeometry;

        public EqualizerOutputControl()
        {
            _barSizes = new List<Size>();
            _peakSizes = new List<Size>();
            _outputGeometry = new StreamGeometry();
            _peakGeometry = new StreamGeometry();
        }

        // This method is provided because the observable collection is not updating with the dependency
        // property binding. Also, performance with binding was terrible.
        //
        public void SetEqualizer(EqualizerResultSet resultSet)
        {
            if (double.IsNaN(this.RenderSize.Width) ||
                double.IsNaN(this.RenderSize.Height) ||
                this.RenderSize.Width <= 0 ||
                this.RenderSize.Height <= 0)
                return;

            if (_barSizes.Count != resultSet.Result.Length)
            {
                _barSizes.Clear();
                _peakSizes.Clear();
            }    

            //var maxRatio = resultSet.ResultPeaks.Max();
            var maxRatio = 1.0f;

            for (int index = 0; index < resultSet.Result.Length; index++)
            {
                var peakRatio = resultSet.ResultPeaks[index];
                var ratio = resultSet.Result[index];

                // Normalizing the bar size: Not sure what to do here. Going to try Db scale.
                var scaledPeakRatio = peakRatio / maxRatio;
                var scaledRatio = ratio / maxRatio;             

                var width = (this.RenderSize.Width / resultSet.Result.Length) - this.BarPadding.Left - this.BarPadding.Right;
                var height = (this.RenderSize.Height * scaledRatio) - this.BarPadding.Top - this.BarPadding.Bottom;

                var peakWidth = (this.RenderSize.Width / resultSet.Result.Length) - this.BarPadding.Left - this.BarPadding.Right;
                var peakHeight = (this.RenderSize.Height * scaledPeakRatio) - this.BarPadding.Top - this.BarPadding.Bottom;

                width = Math.Clamp(width, 0, this.RenderSize.Width);
                height = Math.Clamp(height, 0, this.RenderSize.Height);

                if (_barSizes.Count == resultSet.Result.Length)
                {
                    _barSizes[index] = new Size(width, height);
                    _peakSizes[index] = new Size(peakWidth, peakHeight);
                }
                else
                {
                    _barSizes.Add(new Size(width, height));
                    _peakSizes.Add(new Size(peakWidth, peakHeight));
                }
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(this.RenderSize));

            // Un-Padded
            var barWidth = this.RenderSize.Width / _barSizes.Count;
            var barHeight = this.RenderSize.Height;

            var pointTL = new Point();
            var pointTR = new Point();
            var pointBL = new Point();
            var pointBR = new Point();

            _outputGeometry.Clear();

            using (var stream = _outputGeometry.Open())
            {
                for (int index = 0; index < _barSizes.Count; index++)
                {
                    pointTL.X = barWidth * index;
                    pointTL.Y = barHeight - _barSizes[index].Height;

                    pointTR.X = pointTL.X + barWidth;
                    pointTR.Y = pointTL.Y;

                    pointBR.X = pointTL.X + barWidth;
                    pointBR.Y = pointTL.Y + _barSizes[index].Height;

                    pointBL.X = pointTL.X;
                    pointBL.Y = pointBR.Y;

                    stream.BeginFigure(pointTL, true, true);
                    stream.LineTo(pointTR, true, true);
                    stream.LineTo(pointBR, true, true);
                    stream.LineTo(pointBL, true, true);
                    stream.LineTo(pointTL, true, true);
                }
            }

            using (var stream = _peakGeometry.Open())
            {
                for (int index = 0; index < _barSizes.Count; index++)
                {
                    pointTL.X = barWidth * index;
                    pointTL.Y = barHeight - _peakSizes[index].Height;

                    pointTR.X = pointTL.X + barWidth;
                    pointTR.Y = pointTL.Y;

                    pointBR.X = pointTL.X + barWidth;
                    pointBR.Y = pointTL.Y + 1;                         

                    pointBL.X = pointTL.X;
                    pointBL.Y = pointBR.Y;

                    stream.BeginFigure(pointTL, true, false);
                    stream.LineTo(pointTR, true, true);
                    stream.LineTo(pointBR, true, true);
                    stream.LineTo(pointBL, true, true);
                    stream.LineTo(pointTL, true, true);
                }
            }

            // Peaks
            drawingContext.DrawGeometry(this.PeakBrush, new Pen(this.BarBorder, 1), _peakGeometry);

            // Bars
            drawingContext.DrawGeometry(this.BarBrush, new Pen(this.BarBorder, 1), _outputGeometry);
        }
    }
}
