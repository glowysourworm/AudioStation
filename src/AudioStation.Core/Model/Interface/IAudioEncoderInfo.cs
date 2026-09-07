using CSCore;

namespace AudioStation.Core.Model.Interface
{
    public interface IAudioEncoderInfo
    {
        string Name { get; set; }
        string Extension { get; set; }
        string Filter { get; set; }
        AudioEncoding Encoding { get; set; }
    }
}
