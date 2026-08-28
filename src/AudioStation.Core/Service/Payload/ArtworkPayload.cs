namespace AudioStation.Core.Service.Payload
{
    public class ArtworkPayload : PayloadBase
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
