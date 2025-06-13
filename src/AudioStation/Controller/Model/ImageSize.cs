namespace AudioStation.Controller.Model
{
    /// <summary>
    /// Size structure to handle the image size enumerating for the cache. For full sized images
    /// we need to wait on image creation or retrieval. So, the size, and desired size, will be -1.
    /// </summary>
    public struct ImageSize
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ImageCacheType CacheAsType { get; private set; }

        public bool IsFullSized()
        {
            // Any of these conditions will cause a default to full sized
            return this.CacheAsType == ImageCacheType.FullSize || this.Width == -1 || this.Height == -1;
        }

        public ImageSize(ImageCacheType cacheAsType)
        {
            this.CacheAsType = cacheAsType;

            switch (cacheAsType)
            {
                case ImageCacheType.Thumbnail:
                    this.Width = 24;
                    this.Height = 24;
                    break;
                case ImageCacheType.Small:
                    this.Width = 50;
                    this.Height = 50;
                    break;
                case ImageCacheType.Medium:
                    this.Width = 300;
                    this.Height = 300;
                    break;
                case ImageCacheType.FullSize:
                    this.Width = -1;
                    this.Height = -1;
                    break;
                default:
                    throw new Exception("Unhandled ImageCacheType:  ImageSize.cs");
            }
        }
    }
}
