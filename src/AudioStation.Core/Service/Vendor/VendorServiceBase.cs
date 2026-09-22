using AudioStation.Core.Service.Interface;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Service.Vendor
{
    public abstract class VendorServiceBase : IAudioStationDataService
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;
        private uint _throttleLimitMilliseconds;
        private DateTime _lastServiceCall;
        private const int SERVICE_WAIT_MILLISEC = 100;
        private const int SERVICE_THROTTLE_MILLISEC_MIN = 500;
        private const int SERVICE_THROTTLE_MILLISEC_MAX = 5000;

        private readonly string _serviceName;
        private readonly string _serviceDisplayName;

        protected VendorServiceBase(string serviceName, string serviceDisplayName)
        {
            _status = IAudioStationDataService.Status.Disabled;
            _throttleLimitMilliseconds = 3000;                      // Default at 3000 (ms)
            _lastServiceCall = DateTime.MinValue;
            _serviceName = serviceName;
            _serviceDisplayName = serviceDisplayName;
        }

        /// <summary>
        /// Forces the service to wait the configured amount of time before making another service call. This wait
        /// happens on the thread that calls it.
        /// </summary>
        protected void ServiceWait()
        {
            // Throttle limit for service calls
            while (DateTime.Now < _lastServiceCall.AddMilliseconds(_throttleLimitMilliseconds))
            {
                Thread.Sleep(SERVICE_WAIT_MILLISEC);
            }

            // UPDATE SERVICE WAIT
            _lastServiceCall = DateTime.Now;
        }

        protected void SetThrottleLimit(uint milliseconds)
        {
            if (milliseconds < SERVICE_THROTTLE_MILLISEC_MIN ||
                milliseconds > SERVICE_THROTTLE_MILLISEC_MAX)
                throw new ArgumentException(string.Format("Throttle limit must be between {0} and {1}", SERVICE_THROTTLE_MILLISEC_MIN, SERVICE_THROTTLE_MILLISEC_MAX));

            _throttleLimitMilliseconds = milliseconds;
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return _serviceName;
        }
        public string GetDisplayName()
        {
            return _serviceDisplayName;
        }
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public virtual IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            return _status;
        }

        public virtual Task<IAudioStationDataService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public virtual IAudioStationDataService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationDataService.Status.Idle;
        }

        public virtual Task<IAudioStationDataService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationDataService.Status.Idle);
        }

        public virtual string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationDataService.GetDefaultStatusMessage(_status);
        }
        protected void OnStatusChanged(IAudioStationDataService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}
