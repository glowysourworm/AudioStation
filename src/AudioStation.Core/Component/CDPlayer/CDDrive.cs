using System.Runtime.InteropServices;

using AudioStation.Core.Component.CDPlayer.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.CDPlayer
{
    [IocExport(typeof(ICDDrive))]
    public class CDDrive : ICDDrive, IDisposable
    {
        public event SimpleEventHandler<CDDeviceTracksLoadedEventArgs> TracksLoadedEvent;

        private IntPtr _handleCDDrive;
        private bool _hasData = false;
        private CDPlayerData _trackData = null;

        protected const int NSECTORS = 13;
        protected const int CB_CDDASECTOR = 2368;
        protected const int CB_CDROMSECTOR = 2048;
        protected const int CB_QSUBCHANNEL = 16;
        protected const int CB_AUDIO = CB_CDDASECTOR - CB_QSUBCHANNEL;

        public CDDrive()
        {
            _trackData = new CDPlayerData();
            _handleCDDrive = IntPtr.Zero;
        }

        public int GetTrackCount()
        {
            if (!DeviceHandleAvailable() &&
                !_hasData)
                throw new Exception("CD-ROM device not initialized and not yet read");

            if (_hasData)
            {
                return _trackData.LastTrack - _trackData.FirstTrack + 1;
            }
            else return -1;
        }
        public void ReadTrack(int trackNumber, SimpleEventHandler<CDDataReadEventArgs> progressCallback)
        {
            if (!DeviceHandleAvailable() &&
                !_hasData)
                throw new Exception("CD-ROM device not initialized and not yet read");

            ReadTrack(trackNumber, 0, 0, progressCallback);
        }
        public void SetDevice(char drive, DeviceChangeEventType changeType)
        {
            if (Initialize(drive))
            {
                switch (changeType)
                {
                    case DeviceChangeEventType.DeviceInserted:
                    {
                        var ready = Load() && Ready();
                        var trackCount = GetTrackCount();
                        OnTracksLoaded(drive, ready, trackCount);
                    }
                    break;
                    case DeviceChangeEventType.DeviceRemoved:
                    {
                        Dispose();
                        OnTracksLoaded(drive, false, 0);
                    }
                    break;
                }
            }
        }


        private bool Initialize(char drive)
        {
            if (!DeviceWinAPI.IsCDDrive(drive))
            {
                throw new ArgumentException("Provided drive is not a CD-ROM drive:  " + drive);
            }

            Dispose();

            _handleCDDrive = DeviceWinAPI.CreateFile(@"\\.\" + drive.ToString() + ":",
                                                     DeviceWinAPI.GENERIC_READ, DeviceWinAPI.FILE_SHARE_READ,
                                                     IntPtr.Zero, DeviceWinAPI.OPEN_EXISTING, 0,
                                                     IntPtr.Zero);

            if (DeviceHandleAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void Close()
        {
            UnLock();

            if (DeviceHandleAvailable())
            {
                DeviceWinAPI.CloseHandle(_handleCDDrive);
            }

            _handleCDDrive = IntPtr.Zero;
            _hasData = false;
        }
        private bool DeviceHandleAvailable()
        {
            return ((int)_handleCDDrive != -1) && ((int)_handleCDDrive != 0);
        }
        private bool UnLock()
        {
            if (!DeviceHandleAvailable())
                throw new Exception("CD-ROM device not initialized");

            uint Dummy = 0;
            var preventMediaRemoval = new DeviceWinAPI.PREVENT_MEDIA_REMOVAL();
            preventMediaRemoval.PreventMediaRemoval = 0;

            return DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                                DeviceWinAPI.IOCTL_STORAGE_MEDIA_REMOVAL, preventMediaRemoval,
                                                (uint)Marshal.SizeOf(preventMediaRemoval), IntPtr.Zero, 0,
                                                ref Dummy, IntPtr.Zero) != 0;
        }
        private bool Load()
        {
            if (!DeviceHandleAvailable())
                throw new Exception("CD-ROM device not initialized");

            uint Dummy = 0;
            var result = DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                                      DeviceWinAPI.IOCTL_STORAGE_LOAD_MEDIA, IntPtr.Zero, 0,
                                                      IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0;

            // If native call succeeds -> Load Data (Native)
            return result && LoadData();
        }
        private bool Eject()
        {
            if (!DeviceHandleAvailable())
                throw new Exception("CD-ROM device not initialized");

            uint Dummy = 0;
            return DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                                DeviceWinAPI.IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0,
                                                IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0;
        }
        private bool Ready()
        {
            if (!DeviceHandleAvailable())
                throw new Exception("CD-ROM device not initialized");

            uint Dummy = 0;

            if (DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                             DeviceWinAPI.IOCTL_STORAGE_CHECK_VERIFY, IntPtr.Zero, 0,
                                             IntPtr.Zero, 0, ref Dummy, IntPtr.Zero) != 0)
            {
                return true;
            }
            else
            {
                Dispose();
                return false;
            }
        }

        private bool ReadSector(int sector, byte[] bBuffer, int iSectors)
        {
            if (_hasData && 
               ((sector + iSectors) <= EndSector(_trackData.LastTrack)) && 
               (bBuffer.Length >= CB_AUDIO * iSectors))
            {
                var rawReadInfo = new DeviceWinAPI.RAW_READ_INFO();

                rawReadInfo.TrackMode = DeviceWinAPI.TRACK_MODE_TYPE.CDDA;
                rawReadInfo.SectorCount = (uint)iSectors;
                rawReadInfo.DiskOffset = sector * CB_CDROMSECTOR;

                uint uiRead = 0;

                if (DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                                 DeviceWinAPI.IOCTL_CDROM_RAW_READ, rawReadInfo,
                                                 (uint)Marshal.SizeOf(rawReadInfo), bBuffer, (uint)iSectors *
                                                 CB_AUDIO, ref uiRead, IntPtr.Zero) != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private int ReadTrack(int trackNumber, uint startSecond, uint secondsRead, SimpleEventHandler<CDDataReadEventArgs> progressCallback)
        {
            if (_hasData && (trackNumber >= _trackData.FirstTrack) && (trackNumber <= _trackData.LastTrack))
            {
                int startSector = StartSector(trackNumber);
                int endSector = EndSector(trackNumber);

                if ((startSector += (int)startSecond * 75) >= endSector)
                {
                    startSector -= (int)startSecond * 75;
                }

                if ((secondsRead > 0) && ((int)(startSector +
                   (secondsRead * 75)) < endSector))
                {
                    endSector = startSector + ((int)secondsRead * 75);
                }

                uint bytesToRead = (uint)(endSector - startSector) * CB_AUDIO;
                uint bytesRead = 0;
                byte[] data = new byte[CB_AUDIO * NSECTORS];
                bool continueRead = true;
                bool readOK = true;

                // 0% Progress
                progressCallback(new CDDataReadEventArgs(Array.Empty<byte>(), 0, bytesToRead, 0));

                for (int sector = startSector; (sector < endSector) && continueRead && readOK; sector += NSECTORS)
                {
                    int sectorsToRead = ((sector + NSECTORS) < endSector) ? NSECTORS : (endSector - sector);

                    readOK = ReadSector(sector, data, sectorsToRead);

                    if (readOK)
                    {
                        // Bytes Read
                        bytesRead += (uint)(CB_AUDIO * sectorsToRead);

                        // % Progress
                        progressCallback(new CDDataReadEventArgs(data, (uint)(CB_AUDIO * sectorsToRead), bytesToRead, bytesRead));
                    }
                }
                if (readOK)
                {
                    return (int)bytesRead;
                }
                else
                {
                    return -1;
                }

            }
            else
            {
                return -1;
            }
        }
        private int StartSector(int track)
        {
            if (_hasData && 
               (track >= _trackData.FirstTrack) &&
               (track <= _trackData.LastTrack))
            {
                var trackData = _trackData.TrackData[track - 1];

                return (trackData.Address_1 * 60 * 75) + (trackData.Address_2 * 75) + trackData.Address_3 - 150;
            }
            else
            {
                return -1;
            }
        }
        private int EndSector(int track)
        {
            if (_hasData && 
               (track >= _trackData.FirstTrack) &&
               (track <= _trackData.LastTrack))
            {
                // Allocate Track Data
                var trackData = _trackData.TrackData[track];

                return (trackData.Address_1 * 60 * 75) + (trackData.Address_2 * 75) + trackData.Address_3 - 151;
            }
            else
            {
                return -1;
            }
        }

        private bool LoadData()
        {
            if (!DeviceHandleAvailable())
                throw new Exception("CD-ROM device not initialized");

            uint bytesRead = 0;

            _hasData = DeviceWinAPI.DeviceIoControl(_handleCDDrive,
                                                    DeviceWinAPI.IOCTL_CDROM_READ_TOC, IntPtr.Zero, 0, _trackData,
                                                    (uint)Marshal.SizeOf(_trackData), ref bytesRead,
                                                    IntPtr.Zero) != 0;
            return _hasData;
        }

        private void OnTracksLoaded(char driveLetter, bool isReady, int trackCount)
        {
            if (this.TracksLoadedEvent != null)
                this.TracksLoadedEvent(new CDDeviceTracksLoadedEventArgs(driveLetter, trackCount, isReady));
        }

        public void Dispose()
        {
            if (DeviceHandleAvailable())
            {
                Close();
                GC.SuppressFinalize(this);
            }
        }
    }
}
