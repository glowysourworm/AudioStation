namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderResultStep
    {
        public bool Completed { get; private set; }
        public bool Result { get; private set; }
        public int StepNumber { get; private set; }
        public string Message { get; private set; }

        public LibraryLoaderResultStep(bool completed, bool result, int stepNumber, string message)
        {
            this.Completed = completed;
            this.Result = result;
            this.StepNumber = stepNumber;
            this.Message = message;
        }
    }
}
