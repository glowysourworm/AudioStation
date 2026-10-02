using AudioStation.Core.Model;
using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service.Payload.Output
{
    public class ArtworkPayload : ITagServiceOutputPayload
    {
        LibraryImage _image;

        public LibraryImage Payload { get { return _image; } }

        public ArtworkPayload(LibraryImage image)
        {
            _image = image;
        }
    }
}
