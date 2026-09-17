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

        protected override LibraryWorkerStepResult Work(int step)
        {
            // Steps: 
            //
            // 1) FileReference file integrity (file exists, CRC32 rehash)
            //

            switch (step)
            {
                case 1:
                    return WorkFileCheck(step);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkFileCheck(int stepNumber)
        {
            try
            {
                // Load
                var load = this.Load.Get<LibraryLoaderEntityLoad<FileReference>>();

                // Entity
                var entity = _audioStationDbClient.GetEntity<FileReference>(load.Entity.Id);

                if (entity == null)
                {
                    return LibraryWorkerStepResult.Failure(stepNumber, "File Reference check database error:  (see Database log)");
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

                var message = "File Reference check successful:  ";
                message += invalid ? "(file load error, corrupt, or missing)" : "(file integrity OK)";

                return new LibraryWorkerStepResult()
                {
                    Completed = true,
                    Message = message,
                    StepNumber = stepNumber,
                    Result = invalid ? LibraryWorkerResultLevel.DataError : LibraryWorkerResultLevel.Success
                };
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "File Reference check error: " + ex.Message);
            }
        }
    }
}
