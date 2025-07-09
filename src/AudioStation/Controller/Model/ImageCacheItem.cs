using System.Runtime.CompilerServices;
using System.Windows.Media;

using AudioStation.Component.Model;

using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

using TagLib;

namespace AudioStation.Controller.Model
{
    public class ImageCacheItem
    {
        public readonly SimpleDictionary<PictureType, BitmapImageData> Images;

        public BitmapImageData GetArtistImage()
        {
            return this.Images.GetValue(PictureType.LeadArtist) ??
                   this.Images.GetValue(PictureType.Artist) ??
                   this.Images.GetValue(PictureType.Band) ??
                   this.Images.GetValue(PictureType.Composer) ??
                   this.Images.GetValue(PictureType.FrontCover) ?? null;
        }

        public BitmapImageData GetFirstImage()
        {
            return this.Images.GetFirstValue();
        }

        public BitmapImageData GetAlbumImage()
        {
            return this.Images.GetValue(PictureType.FrontCover) ?? null;
        }
        public ImageCacheItem(PictureType type, BitmapImageData image)
        {
            this.Images = new SimpleDictionary<PictureType, BitmapImageData>() { { type, image } };
        }
        public ImageCacheItem(IDictionary<PictureType, BitmapImageData> images)
        {
            this.Images = new SimpleDictionary<PictureType, BitmapImageData>(images);
        }
    }
}
