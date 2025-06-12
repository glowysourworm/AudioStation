using System.Runtime.CompilerServices;
using System.Windows.Media;

using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

using TagLib;

namespace AudioStation.Controller.Model
{
    public class ImageCacheItem
    {
        public readonly SimpleDictionary<PictureType, ImageSource> Images;

        public ImageSource GetArtistImage()
        {
            return this.Images.GetValue(PictureType.LeadArtist) ??
                   this.Images.GetValue(PictureType.Artist) ??
                   this.Images.GetValue(PictureType.Band) ??
                   this.Images.GetValue(PictureType.Composer) ??
                   this.Images.GetValue(PictureType.FrontCover) ?? null;
        }

        public ImageSource GetAlbumImage()
        {
            return this.Images.GetValue(PictureType.FrontCover) ?? null;
        }
        public ImageCacheItem(IDictionary<PictureType, ImageSource> images)
        {
            this.Images = new SimpleDictionary<PictureType, ImageSource>(images);
        }
    }
}
