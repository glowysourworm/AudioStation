using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderImportWorker : LibraryLoaderWorker
    {
        private readonly ILibraryImporter _libraryImporter;

        private const int WORK_STEPS = 6;

        public LibraryLoaderImportWorker(LibraryLoaderWorkItem workItem,
                                         ILibraryImporter libraryImporter)
            : base(workItem)
        {
            _libraryImporter = libraryImporter;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }
        public static int GetNumberSteps()
        {
            return WORK_STEPS;
        }

        protected override bool Work(int workStep, ref string message)
        {
            // Steps:
            //
            // 1) AcoustID
            // 2) Music Brainz
            // 3) Embed Tag File
            // 4) Import Entity
            // 5) Migrate File (optional)
            // 

            var load = this.Load.Get<LibraryLoaderImportLoad>();
            var output = this.Output.Get<LibraryLoaderImportOutput>();

            switch (workStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    return _libraryImporter.WorkAcoustID(load, output).Result;
                }
                case 2:
                {
                    return _libraryImporter.WorkMusicBrainzDetail(load, output).Result;
                }
                case 3:
                {
                    return _libraryImporter.WorkMusicBrainzCompleteRecord(load, output).Result;
                }
                case 4:
                {
                    return _libraryImporter.WorkEmbedTag(load, output);
                }
                case 5:
                {
                    return _libraryImporter.WorkImportEntity(load, output);
                }
                case 6:
                {
                    return _libraryImporter.WorkMigrateFile(load, output);
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
            }
        }
    }
}
