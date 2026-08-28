using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Service.Payload
{
    public class TagSmallPayload : PayloadBase
    {
        public ITagSmall Data { get; }

        public TagSmallPayload(ITagSmall data)
        {
            this.Data = data;
        }
    }
}
