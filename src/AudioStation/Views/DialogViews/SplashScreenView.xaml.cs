using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MathNet.Numerics;

namespace AudioStation.Views.DialogViews
{
    /// <summary>
    /// Interaction logic for SplashScreenView.xaml
    /// </summary>
    public partial class SplashScreenView : UserControl
    {
        private const int NUMBER_OF_BARS = 15;
        private const double BESSEL_SCALE_X = 5.5;          // Scaling of the J_1 Bessel function based on x=[0,1] (gets a couple oscillations in range)
        private const int BESSEL_SHIFT_BARS = 3;            // Shift the J_1 Bessel over by a bit to get the right part of the swoop

        public SplashScreenView()
        {
            InitializeComponent();

            var stroke = (Brush)FindResource("LoadingBarStroke");
            var fill = (Brush)FindResource("LoadingBarFill");
            var barWidth = this.BarCanvas.Width / NUMBER_OF_BARS;

            for (int index = 0; index < NUMBER_OF_BARS; index++)
            {
                var barHeight = CalculateBarHeight(index + BESSEL_SHIFT_BARS);
                var barTop = this.BarCanvas.Height - barHeight;

                var rectangle = new Rectangle();
                rectangle.Width = barWidth;
                rectangle.Height = barHeight;
                rectangle.Stroke = stroke;
                rectangle.Fill = fill;
                rectangle.StrokeThickness = 1;

                Canvas.SetLeft(rectangle, index * barWidth);
                Canvas.SetTop(rectangle, barTop);

                this.BarCanvas.Children.Add(rectangle);
            }
        }

        private double CalculateBarHeight(int barIndex)
        {
            // Bottom of Bessel_J0 = -0.4

            // Amplitude <= 1
            return Math.Clamp(180 * SpecialFunctions.BesselJ(1, (barIndex / (double)NUMBER_OF_BARS) * BESSEL_SCALE_X) + 90, 0, 200);
        }
    }
}
