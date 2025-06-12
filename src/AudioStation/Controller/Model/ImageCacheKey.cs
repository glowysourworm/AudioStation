using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Controller.Model
{
    public struct ImageCacheKey
    {
        /// <summary>
        /// Id of the associated entity. Image caches are also indexed by file name (from the mp3 file)
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Scaled width of the retrieved image sources
        /// </summary>
        public int DesiredWidth { get; set; }

        /// <summary>
        /// Scaled height of the retrieved image sources
        /// </summary>
        public int DesiredHeight { get; set; }

        public ImageCacheKey(int entityId, int desiredWidth, int desiredHeight)
        {
            this.EntityId = entityId;
            this.DesiredWidth = desiredWidth;
            this.DesiredHeight = desiredHeight;
        }

        public ImageCacheKey(int entityId, ImageSize imageSize) : this(entityId, imageSize.Width, imageSize.Height)
        {

        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var key = (ImageCacheKey)obj;

            return key.EntityId == this.EntityId &&
                   key.DesiredWidth == this.DesiredWidth &&
                   key.DesiredHeight == this.DesiredHeight;
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.EntityId, this.DesiredWidth, this.DesiredHeight);
        }
    }
}
