using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderImportWorker : LibraryLoaderWorker<LibraryLoaderImportLoad, LibraryLoaderImportOutput>
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

            switch (workStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    return _libraryImporter.WorkAcoustID(this.Load, this.Output).Result;
                }
                case 2:
                {
                    return _libraryImporter.WorkMusicBrainzDetail(this.Load, this.Output).Result;
                }
                case 3:
                {
                    return _libraryImporter.WorkMusicBrainzCompleteRecord(this.Load, this.Output).Result;
                }
                case 4:
                {
                    return _libraryImporter.WorkEmbedTag(this.Load, this.Output);
                }
                case 5:
                {
                    return _libraryImporter.WorkImportEntity(this.Load, this.Output);
                }
                case 6:
                {
                    return _libraryImporter.WorkMigrateFile(this.Load, this.Output);
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
            }
        }
    }
}
