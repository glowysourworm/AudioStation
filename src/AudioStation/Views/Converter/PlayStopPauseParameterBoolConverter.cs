using System.Globalization;
using System.Windows.Data;

using AudioStation.Core.Component;

namespace AudioStation.Views.Converter
{
    public class PlayStopPauseParameterBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            var playState = (PlayStopPause)value;
            var buttonState = (PlayStopPause)parameter;

            switch (buttonState)
            {
                case PlayStopPause.Play:
                    return playState == PlayStopPause.Play;
                case PlayStopPause.Pause:
                    return playState == PlayStopPause.Pause;
                case PlayStopPause.Stop:
                    return playState == PlayStopPause.Stop;
                default:
                    return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            var playState = (bool)value;

            if (!playState)
                return Binding.DoNothing;

            return (PlayStopPause)parameter;
        }
    }
}
