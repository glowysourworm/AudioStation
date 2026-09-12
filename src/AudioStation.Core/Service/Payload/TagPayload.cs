using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Service.Payload
{
    public class TagPayload : PayloadBase
    {
        public ITagFull Data { get; }

        public TagPayload(ITagFull data)
        {
            this.Data = data;
        }
    }
}
