using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service.Payload.Output
{
    public class TagPayload : ITagServiceOutputPayload
    {
        public ITagFull Data { get; }

        public TagPayload(ITagFull data)
        {
            this.Data = data;
        }
    }
}
