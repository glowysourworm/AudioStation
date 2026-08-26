using System.Globalization;
using System.Windows.Data;

using ATL;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.ImageCacheModel;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Converts an IPicuture (TagLib) into an ImageSource
    /// </summary>    
    public class TagIPictureConverter : IValueConverter
    {
        readonly IBitmapConverter _bitmapConverter;

        public TagIPictureConverter()
        {
            _bitmapConverter = IocContainer.Get<IBitmapConverter>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var picture = value as PictureInfo;
            var cacheType = (ImageCacheType)parameter;

            if (picture == null)
                return Binding.DoNothing;

            return _bitmapConverter.BitmapDataToBitmapSource(picture.PictureData, new ImageSize(cacheType), picture.MimeType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
