using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service.Payload.Output
{
    public class ArtworkPayload : ITagServiceOutputPayload
    {
        ATL.PictureInfo _data;

        public byte[] GetBuffer()
        {
            return _data.PictureData;
        }
        public string GetMimeType()
        {
            return _data.MimeType;
        }
        public ATL.PictureInfo GetPayload()
        {
            return _data;
        }


        public ArtworkPayload(ATL.PictureInfo data)
        {
            _data = data;
        }
    }
}
