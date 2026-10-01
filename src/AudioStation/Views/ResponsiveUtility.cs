using System.Windows;

namespace AudioStation.Views
{
    public enum ResponsiveSize
    {
        Small,
        Medium,
        Large
    }

    public static class ResponsiveUtility
    {
        public static double WindowSizeSmall;
        public static double WindowSizeMedium;
        public static double WindowSizeLarge;

        static ResponsiveUtility()
        {
            WindowSizeSmall = (double)Application.Current.FindResource("ScreenSmallWidth");
            WindowSizeMedium = (double)Application.Current.FindResource("ScreenMediumWidth");
            WindowSizeLarge = (double)Application.Current.FindResource("ScreenLargeWidth");
        }

        public static ResponsiveSize GetWindowSize()
        {
            var windowWidth = Application.Current.MainWindow.ActualWidth;

            if (windowWidth < WindowSizeSmall)
                return ResponsiveSize.Small;

            else if (windowWidth < WindowSizeMedium)
                return ResponsiveSize.Medium;

            else
                return ResponsiveSize.Large;
        }
    }
}
