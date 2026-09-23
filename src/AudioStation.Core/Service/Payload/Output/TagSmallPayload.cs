using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Payload.Interface;

namespace AudioStation.Core.Service.Payload.Output
{
    public class TagSmallPayload : ITagServiceOutputPayload
    {
        public ITagSmall Data { get; }

        public TagSmallPayload(ITagSmall data)
        {
            this.Data = data;
        }
    }
}
