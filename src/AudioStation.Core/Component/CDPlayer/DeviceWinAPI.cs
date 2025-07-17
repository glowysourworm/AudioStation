using System.Runtime.InteropServices;

namespace AudioStation.Core.Component.CDPlayer
{
    public class DeviceWinAPI()
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public extern static DriveTypes GetDriveType(string sDrive);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static int CloseHandle(IntPtr hObject);
        public const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static int DeviceIoControl(IntPtr hDevice,
                                                 uint IoControlCode,
                                                 IntPtr lpInBuffer, uint InBufferSize,
                                                 IntPtr lpOutBuffer, uint nOutBufferSize,
                                                 ref uint lpBytesReturned,
                                                 IntPtr lpOverlapped);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                                 IntPtr InBuffer, uint InBufferSize,
                                                 [Out] CDPlayerData OutTOC, uint OutBufferSize,
                                                 ref uint BytesReturned,
                                                 IntPtr Overlapped);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                                 [In] PREVENT_MEDIA_REMOVAL InMediaRemoval, uint InBufferSize,
                                                 IntPtr OutBuffer, uint OutBufferSize,
                                                 ref uint BytesReturned,
                                                 IntPtr Overlapped);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static int DeviceIoControl(IntPtr hDevice,
                                                  uint IoControlCode,
                                                  [In] RAW_READ_INFO rri, uint InBufferSize,
                                                  [In, Out] byte[] OutBuffer, uint OutBufferSize,
                                                  ref uint BytesReturned,
                                                  IntPtr Overlapped);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public extern static IntPtr CreateFile(string FileName,
                                               uint DesiredAccess,
                                               uint ShareMode, IntPtr lpSecurityAttributes,
                                               uint CreationDisposition, uint dwFlagsAndAttributes,
                                               IntPtr hTemplateFile);

        public const uint IOCTL_CDROM_READ_TOC = 0x00024000;
        public const uint IOCTL_STORAGE_CHECK_VERIFY = 0x002D4800;
        public const uint IOCTL_CDROM_RAW_READ = 0x0002403E;
        public const uint IOCTL_STORAGE_LOAD_MEDIA = 0x002D480C;
        public const uint IOCTL_STORAGE_EJECT_MEDIA = 0x002D4808;

        public const uint GENERIC_READ = 0x80000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint OPEN_EXISTING = 3;


        public enum DriveTypes : uint
        {
            DRIVE_UNKNOWN = 0,
            DRIVE_NO_ROOT_DIR,
            DRIVE_REMOVABLE,
            DRIVE_FIXED,
            DRIVE_REMOTE,
            DRIVE_CDROM,
            DRIVE_RAMDISK
        };

        public enum TRACK_MODE_TYPE { YellowMode2, XAForm2, CDDA }

        [StructLayout(LayoutKind.Sequential)]
        public class RAW_READ_INFO
        {
            public long DiskOffset = 0;
            public uint SectorCount = 0;
            public TRACK_MODE_TYPE TrackMode = TRACK_MODE_TYPE.CDDA;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PREVENT_MEDIA_REMOVAL
        {
            public byte PreventMediaRemoval = 0;
        }

        public enum DeviceType : uint
        {
            DBT_DEVTYP_OEM = 0x00000000,
            DBT_DEVTYP_DEVNODE = 0x00000001,
            DBT_DEVTYP_VOLUME = 0x00000002,
            DBT_DEVTYP_PORT = 0x00000003,
            DBT_DEVTYP_NET = 0x00000004
        }

        public enum VolumeChangeFlags : ushort
        {
            DBTF_MEDIA = 0x0001,
            DBTF_NET = 0x0002
        }

        public struct DEV_BROADCAST_HDR
        {
            public uint dbch_size;
            public DeviceType dbch_devicetype;
            uint dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public uint dbcv_size;
            public DeviceType dbcv_devicetype;
            uint dbcv_reserved;
            uint dbcv_unitmask;
            public char[] Drives
            {
                get
                {
                    string drvs = "";
                    for (char c = 'A'; c <= 'Z'; c++)
                    {
                        if ((dbcv_unitmask & (1 << (c - 'A'))) != 0)
                        {
                            drvs += c;
                        }
                    }
                    return drvs.ToCharArray();
                }
            }
            public VolumeChangeFlags dbcv_flags;
        }

        public static char[] DriveLetters()
        {
            string result = string.Empty;

            for (char driveLetter = 'C'; driveLetter <= 'Z'; driveLetter++)
            {
                if (DeviceWinAPI.GetDriveType(driveLetter + ":") == DeviceWinAPI.DriveTypes.DRIVE_CDROM)
                {
                    result += driveLetter;
                }
            }

            return result.ToCharArray();
        }

        public static bool IsCDDrive(char driveLetter)
        {
            return DeviceWinAPI.GetDriveType(driveLetter + ":") == DeviceWinAPI.DriveTypes.DRIVE_CDROM;
        }
    }
}
