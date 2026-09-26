using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderAudioEncodingWorker : LibraryLoaderWorker<LibraryLoaderFilePayload, LibraryLoaderAudioEncodingOutputPayload>
    {
        private readonly IAudioConverter _audioConverter;

        public static int NUMBER_STEPS = 1;

        public LibraryLoaderAudioEncodingWorker(IAudioConverter audioConverter, LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _audioConverter = audioConverter;
        }

        public override int GetNumberOfWorkSteps()
        {
            return NUMBER_STEPS;
        }
        public static int GetNumberSteps()
        {
            return NUMBER_STEPS;
        }
        protected override LibraryWorkerStepResult Work(int stepNumber)
        {
            switch (stepNumber)
            {
                case 1:
                    return WorkGetDuration(stepNumber);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkGetDuration(int stepNumber)
        {
            try
            {
                Log("Calculating audio file codec information");

                var duration = TimeSpan.Zero;
                var audioEncoding = _audioConverter.GetAudioEncoding(this.Load.Payload.File, out duration);

                this.Output.Payload.Duration = duration;
                this.Output.Payload.CodecInfo = audioEncoding;

                Log("Audio file codec information complete:  {0}, {1}, {2}, {3}", audioEncoding.Name, audioEncoding.Encoding, audioEncoding.Extension, duration);

                return LibraryWorkerStepResult.Success(stepNumber, "AcoustID fingerprint service call successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "AcoustID fingerprint service error: " + ex.Message);
            }
        }
    }
}
