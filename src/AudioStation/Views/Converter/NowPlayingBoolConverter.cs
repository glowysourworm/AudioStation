using System.Globalization;
using System.Windows.Data;

using AudioStation.Event;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.Converter
{
    public class NowPlayingBoolConverter : IValueConverter
    {
        private readonly IIocEventAggregator _eventAggregator;

        private string _nowPlayingSource;

        public NowPlayingBoolConverter()
        {
            _eventAggregator = IocContainer.Get<IIocEventAggregator>();

            _eventAggregator.GetEvent<LoadPlaybackEvent>().Subscribe(eventData =>
            {
                _nowPlayingSource = eventData.Source;
            });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(_nowPlayingSource))
                return Binding.DoNothing;

            return _nowPlayingSource == (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
