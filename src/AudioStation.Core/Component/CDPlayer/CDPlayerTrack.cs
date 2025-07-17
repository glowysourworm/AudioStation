using System.Runtime.InteropServices;

namespace AudioStation.Core.Component.CDPlayer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CDPlayerTrack
    {
        public byte TrackNumber;
        public byte Reserved1;
        public byte Address_0;
        public byte Address_1;
        public byte Address_2;
        public byte Address_3;

        public byte Reserved;
        private byte BitMapped;
        public byte Control
        {
            get
            {
                return (byte)(BitMapped & 0x0F);
            }
            set
            {
                BitMapped = (byte)((BitMapped & 0xF0) | (value & (byte)0x0F));
            }
        }
        public byte Adr
        {
            get
            {
                return (byte)((BitMapped & (byte)0xF0) >> 4);
            }
            set
            {
                BitMapped = (byte)((BitMapped & (byte)0x0F) | (value << 4));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class CDPlayerTrackList
        {
            public const int MAXIMUM_NUMBER_TRACKS = 100;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXIMUM_NUMBER_TRACKS * 8)]
            private byte[] Data;

            public CDPlayerTrack this[int Index]
            {
                get
                {
                    if ((Index < 0) | (Index >= MAXIMUM_NUMBER_TRACKS))
                    {
                        throw new IndexOutOfRangeException();
                    }

                    CDPlayerTrack result;
                    GCHandle handle = GCHandle.Alloc(Data, GCHandleType.Pinned);

                    try
                    {
                        IntPtr buffer = handle.AddrOfPinnedObject();
                        buffer = (IntPtr)(buffer.ToInt32() + (Index * Marshal.SizeOf(typeof(CDPlayerTrack))));
                        result = (CDPlayerTrack)Marshal.PtrToStructure(buffer, typeof(CDPlayerTrack));
                    }
                    finally
                    {
                        handle.Free();
                    }
                    return result;
                }
            }
        }
    }
}
