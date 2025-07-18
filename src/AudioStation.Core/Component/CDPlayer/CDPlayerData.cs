using System.Net.Mail;
using System.Runtime.InteropServices;

namespace AudioStation.Core.Component.CDPlayer
{
    [StructLayout(LayoutKind.Sequential)]
    public class CDPlayerData
    {
        //public const int MAXIMUM_NUMBER_TRACKS = 100;

        public ushort Length;
        public byte FirstTrack = 0;
        public byte LastTrack = 0;
        public CDPlayerTrackList TrackData;

        public CDPlayerData()
        {
            // 100 tracks is some sort of windows maximum constant
            //
            TrackData = new CDPlayerTrackList();
            Length = (ushort)Marshal.SizeOf(this);
        }
    }
}
