using System.IO;

using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Utility.FileUtility;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderFileCheckerWorker : LibraryLoaderWorker
    {
        private readonly IAudioStationDbClient _audioStationDbClient;

        private const int WORK_STEPS = 1;

        public LibraryLoaderFileCheckerWorker(
                IAudioStationDbClient audioStationDbClient,
                LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _audioStationDbClient = audioStationDbClient;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }
        public static int GetNumberSteps()
        {
            return WORK_STEPS;
        }

        protected override bool Work(int step, ref string message)
        {
            // Steps: 
            //
            // 1) FileReference file integrity (file exists, CRC32 rehash)
            //

            switch (step)
            {
                case 1:
                    return WorkFileCheck(ref message);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkFileCheck(ref string message)
        {
            try
            {
                // Load
                var load = this.Load.Get<LibraryLoaderEntityLoad<FileReference>>();

                // Entity
                var entity = _audioStationDbClient.GetEntity<FileReference>(load.Entity.Id);

                if (entity == null)
                {
                    message = "File Reference check database error:  (see Database log)";
                    return false;
                }

                var exists = File.Exists(entity.FileName);

                var created = exists ? File.GetCreationTime(entity.FileName).ToUniversalTime() : DateTime.MinValue.ToUniversalTime();
                var modified = exists ? File.GetLastWriteTime(entity.FileName).ToUniversalTime() : DateTime.MinValue.ToUniversalTime();
                var crc32 = exists ? FileHelpers.CalculateCRC32(entity.FileName) : 0;
                var corruptCRC = (crc32 != entity.CRC32 && entity.CRC32 != 0) || crc32 == 0;
                var buffer = exists ? File.ReadAllBytes(entity.FileName) : new byte[] { };

                entity.CRC32 = crc32;
                entity.Created = created;
                entity.FileCorruptMessage = corruptCRC ? "CRC32 does not match previous CRC32" : entity.FileCorruptMessage;
                entity.FileErrorMessage = exists ? null : "File does not exist";
                entity.IsFileAvailable = exists;
                entity.IsFileCorrupt = corruptCRC || entity.IsFileCorrupt;
                entity.IsFileLoadError = buffer.Length == 0;
                entity.LastModified = modified;

                _audioStationDbClient.UpdateEntity(entity);

                var invalid = !exists || corruptCRC || entity.IsFileCorrupt;

                message = "File Reference check successful:  ";
                message += invalid ? "(file load error, corrupt, or missing)" : "(file integrity OK)";

                return true;
            }
            catch (Exception ex)
            {
                message = "File Reference check error: " + ex.Message;
                return false;
            }
        }
    }
}
