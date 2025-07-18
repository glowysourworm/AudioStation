using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.CDPlayer.Interface
{
    public interface ICDDrive
    {
        event SimpleEventHandler<CDDeviceTracksLoadedEventArgs> TracksLoadedEvent;

        void ReadTrack(int trackNumber, SimpleEventHandler<CDDataReadEventArgs> progressCallback);
        void SetDevice(char driveLetter, DeviceChangeEventType changeType);
    }
}
