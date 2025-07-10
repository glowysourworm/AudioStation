using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using AudioStation.Core.Component.Interface;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.Converter
{
    public class ArtworkFileConverter : IValueConverter
    {
        private readonly ITagCacheController _tagCacheController;

        public ArtworkFileConverter()
        {
            _tagCacheController = IocContainer.Get<ITagCacheController>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var fileName = (string)value;

            if (string.IsNullOrEmpty(fileName))
                return null;

            var fileRef = _tagCacheController.Get(fileName);

            // Creates IImage "source" for the Avalonia Image control
            if (fileRef.Tag.Pictures.Any())
            {
                using (var stream = new MemoryStream(fileRef.Tag.Pictures.First().Data.Data))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return decoder.Frames[0];
                }
            }

            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
