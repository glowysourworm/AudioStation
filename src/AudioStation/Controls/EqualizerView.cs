using System.Windows;
using System.Windows.Media;

using AudioStation.Component.AudioProcessing;

namespace AudioStation.Controls
{
    // This control must be statically sized
    public class EqualizerView : FrameworkElement
    {
        public static readonly DependencyProperty BarPaddingProperty =
            DependencyProperty.Register("BarPadding", typeof(Thickness), typeof(EqualizerView), new PropertyMetadata(new Thickness(0)));

        public Thickness BarPadding
        {
            get { return (Thickness)GetValue(BarPaddingProperty); }
            set { SetValue(BarPaddingProperty, value); }
        }

        List<Size> _barSizes;
        StreamGeometry _outputGeometry;

        public EqualizerView()
        {
            _barSizes = new List<Size>();
            _outputGeometry = new StreamGeometry();
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
                _barSizes.Clear();

            var maxRatio = resultSet.Result.Max();

            for (int index = 0; index < resultSet.Result.Length; index++)
            {
                var ratio = resultSet.Result[index];
                var scaledRatio = ratio / maxRatio;           // Normalizing the bar size

                var width = (this.RenderSize.Width / resultSet.Result.Length) - this.BarPadding.Left - this.BarPadding.Right;
                var height = (this.RenderSize.Height * scaledRatio) - this.BarPadding.Top - this.BarPadding.Bottom;

                width = Math.Clamp(width, 0, this.RenderSize.Width);
                height = Math.Clamp(height, 0, this.RenderSize.Height);

                if (_barSizes.Count == resultSet.Result.Length)
                    _barSizes[index] = new Size(width, height);

                else
                    _barSizes.Add(new Size(width, height));
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brushes.White, null, new Rect(this.RenderSize));

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

            var brush = new SolidColorBrush(Color.FromArgb(0x0F, 0x00, 0x00, 0x00));

            drawingContext.DrawGeometry(brush, new Pen(Brushes.White, 1), _outputGeometry);
        }
    }
}
