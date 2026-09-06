using NAudio.MediaFoundation;

namespace AudioStation.Core.Component
{
    /// <summary>
    /// Foreach type (identified by the guid, name, and extension), the supported types have to do
    /// with output encodings.
    /// </summary>
    public class AudioEncoderInfo
    {
        public string Name { get; set; }
        public Guid TypeId { get; set; }
        public Guid SubTypeId { get; set; }
        public string Extension { get; set; }
        public MediaType NAudioType { get; set; }
    }
}
