using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderFileConverterWorker : LibraryLoaderWorker
    {
        private readonly IAudioConverter _audioConverter;

        private const int WORK_STEPS = 1;

        public LibraryLoaderFileConverterWorker(
                IAudioConverter audioConverter,
                LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _audioConverter = audioConverter;
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
                    return WorkFileConvert(step);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkFileConvert(int stepNumber)
        {
            try
            {
                // Load
                var load = this.Load.Get<LibraryLoaderFileConverterLoad>();

                _audioConverter.ConvertTo(load.FileIn, load.FileOut, load.EncoderInfo);

                return LibraryWorkerStepResult.Success(stepNumber, "File conversion successful:  " + load.FileOut);
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Success(stepNumber, "File Reference check error: " + ex.Message);
            }
        }
    }
}
