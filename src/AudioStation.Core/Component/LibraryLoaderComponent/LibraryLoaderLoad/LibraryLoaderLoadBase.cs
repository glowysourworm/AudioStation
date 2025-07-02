namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public abstract class LibraryLoaderLoadBase
    {
        public abstract double GetProgress();
        public abstract bool HasErrors();
        public abstract int GetFailureCount();
        public abstract int GetSuccessCount();
    }
}
