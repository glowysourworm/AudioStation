using AudioStation.Core.Component.BitmapConverterComponent;

using IdSharp.Tagging.ID3v2;

using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

namespace AudioStation.Core.Service.ImageCacheModel
{
    public class ImageCacheItem : IDisposable
    {
        public SimpleDictionary<PictureType, BitmapImageData> Images;

        public BitmapImageData GetArtistImage()
        {
            return this.Images.GetValue(PictureType.LeadArtistPerformer) ??
                   this.Images.GetValue(PictureType.ArtistPerformer) ??
                   this.Images.GetValue(PictureType.BandArtistLogo) ??
                   this.Images.GetValue(PictureType.Composer) ??
                   this.Images.GetValue(PictureType.CoverFront) ?? null;
        }

        public BitmapImageData GetFirstImage()
        {
            return this.Images.GetFirstValue();
        }

        public BitmapImageData GetAlbumImage()
        {
            return this.Images.GetValue(PictureType.CoverFront) ?? null;
        }

        public ImageCacheItem(PictureType type, BitmapImageData image)
        {
            this.Images = new SimpleDictionary<PictureType, BitmapImageData>() { { type, image } };
        }
        public ImageCacheItem(IDictionary<PictureType, BitmapImageData> images)
        {
            this.Images = new SimpleDictionary<PictureType, BitmapImageData>(images);
        }

        public void Dispose()
        {
            foreach (var pair in this.Images)
            {
                // TODO:  THREADING PROBLEM! (This is being somehow "disposed" elsewhere.. may have been a framework bug)
                if (pair.Value != null)
                    pair.Value.Dispose();
            }

            this.Images.Clear();
            this.Images = null;
        }
    }
}
