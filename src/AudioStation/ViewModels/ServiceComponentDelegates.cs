namespace AudioStation.ViewModels
{
    public class ServiceComponentDelegates
    {
        public delegate void ServiceComponentStatusUpdateHandler(ServiceComponentPartViewModelBase sender, bool working, bool loaded);
    }
}
