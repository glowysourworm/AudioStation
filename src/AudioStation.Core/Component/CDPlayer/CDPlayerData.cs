using System.Runtime.InteropServices;

namespace AudioStation.Core.Component.CDPlayer
{
    [StructLayout(LayoutKind.Sequential)]
    public class CDPlayerData
    {
        public ushort Length;
        public byte FirstTrack = 0;
        public byte LastTrack = 0;

        public CDPlayerTrack.CDPlayerTrackList TrackData;

        public CDPlayerData()
        {
            TrackData = new CDPlayerTrack.CDPlayerTrackList();
            Length = (ushort)Marshal.SizeOf(this);
        }
    }
}
