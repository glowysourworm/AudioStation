using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.CDPlayer.Interface
{
    public interface ICDDrive
    {
        event SimpleEventHandler<CDDeviceTracksLoadedEventArgs> TracksLoadedEvent;

        int GetTrackCount();
        void ReadTrack(int trackNumber, SimpleEventHandler<CDDataReadEventArgs> progressCallback);
        void SetDevice(char driveLetter, DeviceChangeEventType changeType);
    }
}
