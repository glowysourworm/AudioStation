using System.Globalization;
using System.Windows.Data;

using AudioStation.Core.Service.Interface;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.Converter
{
    public class ArtworkFileConverter : IValueConverter
    {
        private readonly ITagCache _tagCacheController;

        public ArtworkFileConverter()
        {
            _tagCacheController = IocContainer.Get<ITagCache>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var fileName = (string)value;

            if (string.IsNullOrEmpty(fileName))
                return null;

            var fileRef = _tagCacheController.Get(fileName);

            return null;

            // Creates IImage "source" for the Avalonia Image control
            //if (fileRef.EmbeddedPictures.Any())
            //{
            //    //using (var stream = new MemoryStream(fileRef.EmbeddedPictures.First().DecodeContent()))
            //    //{
            //    //    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            //    //    return decoder.Frames[0];
            //    //}
            //    return null;
            //}

            //else
            //    return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
