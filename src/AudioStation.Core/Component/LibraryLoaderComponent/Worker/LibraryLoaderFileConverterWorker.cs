using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderFileConverterWorker : LibraryLoaderWorker<LibraryLoaderFileConverterPayload, LibraryLoaderNoOutput>
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
                _audioConverter.ConvertTo(this.Load.Payload.FileIn, this.Load.Payload.FileOut, this.Load.Payload.EncoderInfo);

                return LibraryWorkerStepResult.Success(stepNumber, "File conversion successful:  " + this.Load.Payload.FileOut);
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Success(stepNumber, "File Reference check error: " + ex.Message);
            }
        }
    }
}
