using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public EqualizerView()
        {
            _barSizes = new List<Size>();
        }

        // This method is provided because the observable collection is not updating with the dependency
        // property binding. Also, performance with binding was terrible.
        //
        public void SetEqualizer(float[] values)
        {
            if (double.IsNaN(this.RenderSize.Width) ||
                double.IsNaN(this.RenderSize.Height) ||
                this.RenderSize.Width <= 0 ||
                this.RenderSize.Height <= 0)
                return;

            if (_barSizes.Count != values.Length)
                _barSizes.Clear();

            var maxRatio = values.Max();

            for (int index = 0; index < values.Length; index++)
            {
                var ratio = values[index];
                var scaledRatio = (ratio / maxRatio) * 0.75;            // Setting the max bar height at 3/4's the height

                var width = (this.RenderSize.Width / values.Length) - this.BarPadding.Left - this.BarPadding.Right;
                var height = (this.RenderSize.Height * scaledRatio) - this.BarPadding.Top - this.BarPadding.Bottom;

                width = Math.Clamp(width, 0, this.RenderSize.Width);
                height = Math.Clamp(height, 0, this.RenderSize.Height);

                if (_barSizes.Count == values.Length)
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
            var barWidth = (this.RenderSize.Width / _barSizes.Count);
            var barHeight = this.RenderSize.Height;

            var point = new Point();

            for (int index = 0; index < _barSizes.Count; index++)
            {
                point.X = barWidth * index;
                point.Y = barHeight - _barSizes[index].Height;

                drawingContext.DrawRectangle(Brushes.Gray, new Pen(Brushes.White, this.BarPadding.Left), new Rect(point, _barSizes[index]));
            }
        }
    }
}
