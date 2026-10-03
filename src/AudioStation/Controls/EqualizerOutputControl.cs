using System.Windows;
using System.Windows.Media;

using AudioStation.Model.AudioProcessing;

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
        List<Size> _barHalfSizes;
        List<Size> _barFullSizes;
        List<Size> _peakSizes;
        StreamGeometry _outputGeometry;
        StreamGeometry _outputHalfGeometry;
        StreamGeometry _outputFullGeometry;
        StreamGeometry _peakGeometry;

        public EqualizerOutputControl()
        {
            _barSizes = new List<Size>();
            _barHalfSizes = new List<Size>();
            _barFullSizes = new List<Size>();
            _peakSizes = new List<Size>();
            _outputGeometry = new StreamGeometry();
            _outputHalfGeometry = new StreamGeometry();
            _outputFullGeometry = new StreamGeometry();
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

            if (_barSizes.Count != resultSet.GetWindowedResultLength(EqualizerResultSet.ResultWindowType.SymmetricQuarter))
            {
                _barSizes.Clear();
                _barHalfSizes.Clear();
                _barFullSizes.Clear();
                _peakSizes.Clear();
            }

            // The FFT output will be symmetric. We'll take the first quarter of the output, which should
            // cover most audible frequencies. Otherwise, there's very little to look at.
            //
            resultSet.Iterate(EqualizerResultSet.ResultWindowType.SymmetricQuarter, (int index,
                                                                                     int length,
                                                                                     float result,
                                                                                     float resultReleaseHalf,
                                                                                     float resultReleaseFull,
                                                                                     float resultPeak,
                                                                                     float maxValue,
                                                                                     float maxValueReleaseHalf,
                                                                                     float maxValueReleaseFull,
                                                                                     float maxPeak) =>
            {
                // Normalizing the bar size: Not sure what to do here. Going to try Db scale..(?)
                var scaledPeakRatio = maxPeak <= 0 ? 0 : resultPeak / maxPeak;
                var scaledRatio = maxValue <= 0 ? 0 : result / maxValue;
                var scaledRatioHalf = maxValueReleaseHalf <= 0 ? 0 : resultReleaseHalf / maxValueReleaseHalf;
                var scaledRatioFull = maxValueReleaseFull <= 0 ? 0 : resultReleaseFull / maxValueReleaseFull;

                var width = CalculateBarWidth(scaledRatio, length);
                var height = CalculateBarHeight(scaledRatio);

                var widthHalf = CalculateBarWidth(scaledRatioHalf, length);
                var heightHalf = CalculateBarHeight(scaledRatioHalf);

                var widthFull = CalculateBarWidth(scaledRatioFull, length);
                var heightFull = CalculateBarHeight(scaledRatioFull);

                var peakWidth = CalculateBarWidth(scaledPeakRatio, length);
                var peakHeight = CalculateBarHeight(scaledPeakRatio);

                if (index < _barSizes.Count - 1)
                {
                    _barSizes[index] = new Size(width, height);
                    _barHalfSizes[index] = new Size(widthHalf, heightHalf);
                    _barFullSizes[index] = new Size(widthFull, heightFull);
                    _peakSizes[index] = new Size(peakWidth, peakHeight);
                }
                else
                {
                    _barSizes.Add(new Size(width, height));
                    _barHalfSizes.Add(new Size(widthHalf, heightHalf));
                    _barFullSizes.Add(new Size(widthFull, heightFull));
                    _peakSizes.Add(new Size(peakWidth, peakHeight));
                }
            });

            InvalidateVisual();
        }

        private double CalculateBarWidth(double scaledRatio, int numberOfBars)
        {
            var width = (this.RenderSize.Width / numberOfBars) - this.BarPadding.Left - this.BarPadding.Right;

            width = Math.Clamp(width, 0, this.RenderSize.Width);

            return width;
        }
        private double CalculateBarHeight(double scaledRatio)
        {
            var height = (this.RenderSize.Height * scaledRatio) - this.BarPadding.Top - this.BarPadding.Bottom;

            height = Math.Clamp(height, 0, this.RenderSize.Height);

            return height;
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
            _outputHalfGeometry.Clear();
            _outputFullGeometry.Clear();

            //// Full (release coefficient)
            //using (var stream = _outputFullGeometry.Open())
            //{
            //    for (int index = 0; index < _barFullSizes.Count; index++)
            //    {
            //        DrawBar(stream, _barFullSizes[index], barWidth, barHeight, index, pointTL, pointTR, pointBR, pointBL);
            //    }
            //}

            //// Half (release coefficient)
            //using (var stream = _outputHalfGeometry.Open())
            //{
            //    for (int index = 0; index < _barHalfSizes.Count; index++)
            //    {
            //        DrawBar(stream, _barHalfSizes[index], barWidth, barHeight, index, pointTL, pointTR, pointBR, pointBL);
            //    }
            //}

            // Normal
            using (var stream = _outputGeometry.Open())
            {
                for (int index = 0; index < _barSizes.Count; index++)
                {
                    DrawBar(stream, _barSizes[index], barWidth, barHeight, index, pointTL, pointTR, pointBR, pointBL);
                }
            }

            // This code differs from the DrawBar() subroutine
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
            //drawingContext.DrawGeometry(Brushes.Blue, new Pen(this.BarBorder, 1), _outputFullGeometry);
            //drawingContext.DrawGeometry(this.BarBrush, new Pen(this.BarBorder, 1), _outputHalfGeometry);
            drawingContext.DrawGeometry(this.BarBrush, new Pen(this.BarBorder, 1), _outputGeometry);
        }

        private void DrawBar(StreamGeometryContext stream,
                             Size barSize,
                             double barRenderWidth,
                             double barRenderHeight,
                             int barIndex,
                             Point pointTL, Point pointTR,
                             Point pointBR, Point pointBL)
        {
            pointTL.X = barRenderWidth * barIndex;
            pointTL.Y = barRenderHeight - barSize.Height;

            pointTR.X = pointTL.X + barRenderWidth;
            pointTR.Y = pointTL.Y;

            pointBR.X = pointTL.X + barRenderWidth;
            pointBR.Y = pointTL.Y + barSize.Height;

            pointBL.X = pointTL.X;
            pointBL.Y = pointBR.Y;

            stream.BeginFigure(pointTL, true, true);
            stream.LineTo(pointTR, true, true);
            stream.LineTo(pointBR, true, true);
            stream.LineTo(pointBL, true, true);
            stream.LineTo(pointTL, true, true);
        }
    }
}
